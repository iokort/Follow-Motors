"""Pruebas HTTP de Follow Motors sobre una base MySQL dedicada a pruebas."""

import concurrent.futures
import json
import os
import sys
import threading
import time
import uuid
from datetime import datetime, timedelta, timezone
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen


API = os.environ.get("FM_TEST_API", "http://127.0.0.1:5148/api")
CORRIDA = uuid.uuid4().hex[:12]
CLAVE_FIXTURE = uuid.uuid4().hex
TOTAL = 0
CONTADOR = threading.Lock()


def api(method, path, status, *, data=None, token=None):
    """Llama al endpoint, exige su código HTTP y devuelve su cuerpo JSON."""
    global TOTAL
    headers = {"Accept": "application/json"}
    if token:
        headers["Authorization"] = "Bearer " + token
    if data is not None:
        headers["Content-Type"] = "application/json"
    request = Request(API + path, json.dumps(data).encode() if data is not None else None, headers, method=method)
    try:
        with urlopen(request, timeout=15) as response:
            actual, body = response.status, response.read()
    except HTTPError as error:
        actual, body = error.code, error.read()
    esperados = {status} if isinstance(status, int) else status
    if actual not in esperados:
        detalle = body.decode(errors="replace")[:500] if actual >= 400 else ""
        raise AssertionError(f"{method} {path}: esperado {status}, recibido {actual}. {detalle}")
    with CONTADOR:
        TOTAL += 1
    payload = json.loads(body) if body else None
    return payload if isinstance(status, int) else (actual, payload)


def esperar_api():
    """Espera el arranque del backend antes de probar el contrato HTTP."""
    for _ in range(30):
        try:
            return api("GET", "/estado", 200)
        except URLError:
            time.sleep(1)
    raise RuntimeError("La API no respondió")


def inicio(identificador, contrasena, status=200):
    """Autentica una cuenta de prueba o verifica su rechazo."""
    return api("POST", "/autenticacion/iniciar-sesion", status,
               data={"identificador": identificador, "contrasena": contrasena})


def nuevo_cliente(gerente, etiqueta):
    """Registra un propietario identificable y abre su sesión."""
    identificador = f"{etiqueta}-{CORRIDA}@follow.motors"
    cliente = {"identificador": identificador, "contrasena": CLAVE_FIXTURE,
               "nombres": f"Prueba {etiqueta}", "apellidos": CORRIDA, "telefono": None}
    id_cliente = api("POST", "/clientes", 201, data=cliente, token=gerente)["id"]
    return id_cliente, identificador, inicio(identificador, CLAVE_FIXTURE)["token"], cliente


def probar_identidad_y_fichas(gerente):
    """Verifica roles, revocación, persistencia y propiedad de expedientes."""
    id_cliente, identificador, usuario, ficha = nuevo_cliente(gerente, "titular")
    id_otro, _, otro, _ = nuevo_cliente(gerente, "otro")
    inicio(identificador, "contraseña incorrecta", 401)
    api("POST", "/clientes", 409, data=ficha, token=gerente)
    api("POST", "/clientes", 403, data=ficha, token=usuario)
    api("GET", "/personal", 403, token=usuario)
    assert not api("GET", "/clientes/mi-expediente", 200, token=usuario)["motocicletas"]
    api("PUT", f"/clientes/{id_cliente}", 204, token=gerente,
        data={"nombres": "Prueba editada", "apellidos": CORRIDA, "telefono": None})
    assert api("GET", f"/clientes/{id_cliente}", 200, token=gerente)["nombres"] == "Prueba editada"
    assert any(c["id"] == id_cliente for c in api("GET", f"/clientes?buscar={CORRIDA}", 200, token=gerente))
    moto = {"clienteId": id_cliente, "numeroSerie": f"VIN-{CORRIDA}", "placa": None,
            "marca": "Marca de prueba", "modelo": "Prueba", "anio": 2024, "color": None,
            "kilometrajeActual": 5400}
    id_moto = api("POST", "/motocicletas", 201, data=moto, token=gerente)["id"]
    api("POST", "/motocicletas", 409, data=moto, token=gerente)
    assert api("GET", f"/motocicletas/{id_moto}", 200, token=usuario)["anio"] == 2024
    api("GET", f"/motocicletas/{id_moto}", 404, token=otro)
    assert not api("GET", "/motocicletas/mis-motocicletas", 200, token=otro)
    assert any(m["id"] == id_moto for m in api("GET", f"/motocicletas?buscar=VIN-{CORRIDA}", 200, token=gerente))
    api("PUT", f"/motocicletas/{id_moto}", 204, data={**moto, "color": "Negro"}, token=gerente)
    assert api("GET", f"/motocicletas/{id_moto}", 200, token=usuario)["color"] == "Negro"
    catalogo = api("GET", "/demo/catalogos", 200, token=gerente)
    assert any(m["id"] == id_moto for m in catalogo["motocicletas"])
    api("GET", "/demo/catalogos", 403, token=usuario)
    segunda_sesion = inicio(identificador, CLAVE_FIXTURE)["token"]
    api("POST", "/autenticacion/cerrar-sesion", 204, token=segunda_sesion)
    api("GET", "/motocicletas/mis-motocicletas", 401, token=segunda_sesion)
    api("GET", "/motocicletas/mis-motocicletas", 200, token=usuario)
    api("DELETE", f"/clientes/{id_otro}", 204, token=gerente)
    api("GET", "/clientes/mi-expediente", 401, token=otro)
    assert not api("GET", f"/clientes/{id_otro}", 200, token=gerente)["activo"]
    return id_cliente, id_moto, usuario


def nuevo_empleado(gerente, etiqueta, mecanico=True):
    """Crea personal operativo para comprobar funciones y reparto de carga."""
    login = f"{etiqueta}-{CORRIDA}@follow.motors"
    datos = {"identificador": login, "contrasena": CLAVE_FIXTURE,
             "nombres": etiqueta, "apellidos": CORRIDA, "esMecanico": mecanico}
    id_personal = api("POST", "/personal", 201, data=datos, token=gerente)["id"]
    return id_personal, inicio(login, CLAVE_FIXTURE)["token"]


def probar_agenda_y_recepcion(gerente, cliente, moto, usuario):
    """Prueba capacidad simultánea, cambios de cita y atención de taller con dos mecánicos."""
    otro_cliente, _, otro, _ = nuevo_cliente(gerente, "agenda")
    otra_moto = api("POST", "/motocicletas", 201, token=gerente,
                    data={"clienteId": otro_cliente, "numeroSerie": f"AGENDA-{CORRIDA}",
                          "marca": "Prueba", "modelo": "Agenda", "kilometrajeActual": 100})["id"]
    inicio_franja = datetime.now(timezone.utc).replace(microsecond=0) + timedelta(hours=2)
    primera = {"inicio": inicio_franja.isoformat(),
               "fin": (inicio_franja + timedelta(hours=1)).isoformat(), "capacidad": 1, "habilitada": True}
    segunda = {**primera, "inicio": primera["fin"],
               "fin": (inicio_franja + timedelta(hours=2)).isoformat(), "capacidad": 2}
    franja = api("POST", "/agenda/franjas", 200, data=primera, token=gerente)["id"]
    destino = api("POST", "/agenda/franjas", 200, data=segunda, token=gerente)["id"]
    api("POST", "/agenda/franjas", 409, data={**primera, "fin": segunda["fin"]}, token=gerente)
    fecha = primera["inicio"].replace("+00:00", "Z")
    hasta = segunda["fin"].replace("+00:00", "Z")
    assert len(api("GET", f"/agenda/franjas?desde={fecha}&hasta={hasta}&disponibles=true", 200, token=gerente)) == 2
    citas = [{"franjaId": franja, "clienteId": c, "motocicletaId": m, "motivo": "Revisión"}
             for c, m in [(cliente, moto), (otro_cliente, otra_moto)]]
    api("POST", "/citas", 403, data=citas[0], token=usuario)
    api("POST", "/citas", 404, data={**citas[0], "motocicletaId": otra_moto}, token=gerente)
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
        carreras = [pool.submit(api, "POST", "/citas", {201, 409}, data=datos, token=gerente)
                    for datos in citas]
        resultados = [f.result() for f in carreras]
    assert sorted(x[0] for x in resultados) == [201, 409]
    ganador = 0 if resultados[0][0] == 201 else 1
    cita = resultados[ganador][1]["id"]
    titulares = (usuario, otro)
    assert api("GET", f"/citas/{cita}", 200, token=titulares[ganador])["franjaId"] == franja
    api("GET", f"/citas/{cita}", 404, token=titulares[1-ganador])
    api("PUT", f"/agenda/franjas/{franja}", 409, data={**primera, "habilitada": False}, token=gerente)
    api("PUT", f"/citas/{cita}/franja", 204, data={"franjaId": destino}, token=gerente)
    assert api("GET", f"/citas/{cita}", 200, token=titulares[ganador])["franjaId"] == destino
    api("DELETE", f"/citas/{cita}", 204, token=gerente)
    assert api("GET", f"/citas/{cita}", 200, token=titulares[ganador])["estado"] == "CANCELADA"
    cita_valida = api("POST", "/citas", 201, data=citas[0], token=gerente)["id"]

    id_primero, tecnico_uno = nuevo_empleado(gerente, "mecanico-uno")
    id_segundo, tecnico_dos = nuevo_empleado(gerente, "mecanico-dos")
    _, asesor = nuevo_empleado(gerente, "asesor", False)
    api("DELETE", f"/clientes/{cliente}", 403, token=asesor)
    api("PUT", "/gerencia/tarifa-revision", 403, data={"importe": 280}, token=asesor)
    api("PUT", "/gerencia/tarifa-revision", 200, data={"importe": 280}, token=gerente)
    recepciones = [
        {"clienteId": cliente, "motocicletaId": moto, "citaId": cita_valida},
        {"clienteId": otro_cliente, "motocicletaId": otra_moto, "citaId": None},
    ]
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
        en_curso = [pool.submit(api, "POST", "/atenciones", 201,
                                data={**datos, "motivo": "Revisión", "kilometrajeIngreso": 5500}, token=asesor)
                    for datos in recepciones]
        ids = [f.result()["id"] for f in en_curso]
    atencion, sin_contrato = ids
    asignaciones = [api("GET", f"/atenciones/{id_atencion}", 200, token=gerente)["mecanicoId"]
                    for id_atencion in ids]
    assert set(asignaciones) == {id_primero, id_segundo}
    assert api("GET", f"/citas/{cita_valida}", 200, token=usuario)["estado"] == "ATENDIDA"
    api("GET", f"/atenciones/{atencion}", 404, token=otro)
    tecnicos = {id_primero: tecnico_uno, id_segundo: tecnico_dos}
    diagnostico = {"hallazgos": "Inspección técnica registrada"}
    api("POST", f"/atenciones/{atencion}/diagnostico", 403, data=diagnostico, token=asesor)
    api("POST", f"/atenciones/{atencion}/diagnostico", 204, data=diagnostico, token=tecnicos[asignaciones[0]])
    api("POST", f"/atenciones/{sin_contrato}/diagnostico", 204, data=diagnostico, token=gerente)
    for id_atencion in ids:
        api("POST", f"/atenciones/{id_atencion}/cobro", 200, token=asesor,
            data={"referenciaComprobante": "REV-PRUEBA"})
    api("POST", f"/atenciones/{atencion}/cobro", 409, token=asesor, data={})
    api("POST", f"/atenciones/{sin_contrato}/cerrar-sin-reparacion", 204, token=asesor)
    assert api("GET", f"/atenciones/{sin_contrato}", 200, token=otro)["estado"] == "SIN_REPARACION"
    return atencion, otro, asesor, tecnicos[asignaciones[0]], sin_contrato, otra_moto


def main():
    """Ejecuta los escenarios de extremo a extremo con cuentas generadas para esta corrida."""
    if os.environ.get("FM_TEST_CONFIRMAR") != "mysql-de-pruebas":
        sys.exit("Define FM_TEST_CONFIRMAR=mysql-de-pruebas y usa una base dedicada a las pruebas.")
    assert esperar_api()["modo"] == "mysql"
    assert api("GET", "/demo/accesos", 200) == []
    api("GET", "/clientes", 401)
    gerente = inicio(os.environ["FM_TEST_GERENTE"], os.environ["FM_TEST_CLAVE"])["token"]
    cliente, moto, usuario = probar_identidad_y_fichas(gerente)
    probar_agenda_y_recepcion(gerente, cliente, moto, usuario)
    print(f"{TOTAL} respuestas HTTP verificadas contra MySQL", flush=True)


if __name__ == "__main__":
    main()
