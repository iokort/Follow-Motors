"""Pruebas HTTP de Follow Motors sobre una base MySQL dedicada a pruebas."""

import concurrent.futures
import json
import os
import sys
import time
import uuid
from datetime import datetime, timedelta, timezone
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen


API = os.environ.get("FM_TEST_API", "http://127.0.0.1:5148/api")
CORRIDA = uuid.uuid4().hex[:12]
CLAVE_FIXTURE = uuid.uuid4().hex
TOTAL = 0


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
    if actual != status:
        detalle = body.decode(errors="replace")[:500] if actual >= 400 else ""
        raise AssertionError(f"{method} {path}: esperado {status}, recibido {actual}. {detalle}")
    TOTAL += 1
    return json.loads(body) if body else None


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


def main():
    """Ejecuta los escenarios de extremo a extremo con cuentas generadas para esta corrida."""
    if os.environ.get("FM_TEST_CONFIRMAR") != "mysql-de-pruebas":
        sys.exit("Define FM_TEST_CONFIRMAR=mysql-de-pruebas y usa una base dedicada a las pruebas.")
    assert esperar_api()["modo"] == "mysql"
    assert api("GET", "/demo/accesos", 200) == []
    api("GET", "/clientes", 401)
    gerente = inicio(os.environ["FM_TEST_GERENTE"], os.environ["FM_TEST_CLAVE"])["token"]
    probar_identidad_y_fichas(gerente)
    print(f"{TOTAL} respuestas HTTP verificadas contra MySQL", flush=True)


if __name__ == "__main__":
    main()
