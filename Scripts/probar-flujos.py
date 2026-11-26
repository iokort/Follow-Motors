"""Pruebas HTTP de Follow Motors sobre una base MySQL dedicada a pruebas."""

import concurrent.futures
import json
import os
import subprocess
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
            with urlopen(API + "/estado", timeout=4) as respuesta:
                if respuesta.status == 200:
                    return api("GET", "/estado", 200)
        except URLError:
            pass
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
    api("GET", "/estado", 200, token=segunda_sesion)
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


def ops(method, path, status, *, data=None, token=None):
    """Ejecuta una operación de taller con sus permisos habituales."""
    return api(method, "/operaciones" + path, status, data=data, token=token)


def nueva_revision(gerente, cliente, moto):
    """Registra, diagnostica y cobra una segunda atención de la misma motocicleta."""
    atencion = api("POST", "/atenciones", 201, token=gerente, data={
        "clienteId": cliente, "motocicletaId": moto, "motivo": "Segunda revisión", "kilometrajeIngreso": 5700})["id"]
    api("POST", f"/atenciones/{atencion}/diagnostico", 204,
        data={"hallazgos": "Revisión con diagnóstico verificable"}, token=gerente)
    api("POST", f"/atenciones/{atencion}/cobro", 200,
        data={"referenciaComprobante": "REV-PRUEBAS"}, token=gerente)
    return atencion


def probar_taller(gerente, cliente, moto, usuario, contexto):
    """Verifica anticipo, ejecución ponderada, refacciones, cancelación, entrega y garantía."""
    atencion, ajeno, asesor, tecnico, _, _ = contexto
    tipo = ops("POST", "/tipos-servicio", 200, token=gerente, data={
        "nombre": "Reparación " + CORRIDA, "garantiaDias": 30, "cobertura": "Mano de obra sobre la reparación"})["id"]
    assert any(t["id"] == tipo for t in ops("GET", "/tipos-servicio", 200, token=asesor))

    def orden_nueva(atencion_id, pesos, mano=1000, costo_piezas=0, probar_restricciones=False):
        """Cotiza y contrata una revisión pagada, probando incumplimientos si corresponde."""
        datos = {"atencionId": atencion_id, "tipoServicioId": tipo, "descripcion": "Reparación acordada",
                 "manoObra": mano, "refacciones": costo_piezas,
                 "tareas": [{"descripcion": f"Tarea {i}", "peso": peso} for i, peso in enumerate(pesos)]}
        if probar_restricciones:
            ops("POST", "/presupuestos", 400, token=gerente,
                data={**datos, "tareas": [{"descripcion": "Peso incorrecto", "peso": 90}]})
        presupuesto = ops("POST", "/presupuestos", 201, token=gerente, data=datos)["id"]
        decision = {"presupuestoId": presupuesto, "aceptar": True}
        pieza = None
        if costo_piezas:
            ops("POST", "/presupuestos/decision", 409, token=asesor, data=decision)
            pieza = ops("POST", "/refacciones", 200, token=asesor, data={"presupuestoId": presupuesto,
                        "descripcion": "Pieza cotizada en proveedor", "cantidad": 3, "precioUnitario": 80})["id"]
        ops("POST", "/presupuestos/decision", 403, token=usuario, data=decision)
        ops("POST", "/presupuestos/decision", 204, token=asesor, data=decision)
        anticipo = {"presupuestoId": presupuesto, "manoObra": mano,
                    "refacciones": costo_piezas, "comprobante": f"ANT-{CORRIDA}-{presupuesto}"}
        if probar_restricciones:
            ops("POST", "/ordenes", 409, token=asesor, data={**anticipo, "manoObra": mano - 1})
        orden = ops("POST", "/ordenes", 200, token=asesor, data=anticipo)["id"]
        if probar_restricciones:
            ops("POST", "/ordenes", 409, token=asesor, data=anticipo)
            ops("POST", "/anticipos", 409, token=asesor, data={**anticipo, "ordenId": orden})
        return orden, pieza

    def avanzar(orden, tarea, porcentaje, status=204, token=gerente, evidencia="Inspección del trabajo"):
        """Registra el avance de un paso del plan con su evidencia."""
        return ops("POST", "/ordenes/tareas/avance", status, token=token,
                   data={"ordenId": orden, "tareaId": tarea, "porcentaje": porcentaje, "evidencia": evidencia})

    orden, pieza = orden_nueva(atencion, [60, 40], costo_piezas=240, probar_restricciones=True)
    get_orden = f"/ordenes/{orden}"
    ops("GET", get_orden, 404, token=ajeno)
    vista = ops("GET", get_orden, 200, token=usuario)
    primera, segunda = [t["id"] for t in vista["tareas"]]
    assert vista["estado"] == "PENDIENTE_PIEZAS"
    assert ops("GET", f"/garantias/orden/{orden}", 200, token=usuario)["estado"] == "INFORMADA"
    ops("POST", "/ordenes/entrega", 409, token=asesor, data={"ordenId": orden})
    avanzar(orden, primera, 100, 409)
    entrega_pieza = {"refaccionId": pieza, "cantidad": 2, "costoUnitario": 92, "factura": "PROV-1"}
    ops("POST", "/refacciones/recepcion", 409, token=asesor, data=entrega_pieza)
    ops("POST", f"/refacciones/{pieza}/pedir", 204, token=asesor)
    ops("POST", "/refacciones/recepcion", 204, token=asesor, data=entrega_pieza)
    assert ops("GET", get_orden, 200, token=usuario)["estado"] == "PENDIENTE_PIEZAS"
    ops("POST", "/refacciones/recepcion", 204, token=asesor,
        data={**entrega_pieza, "cantidad": 1, "costoUnitario": 99, "factura": "PROV-2"})
    vista = ops("GET", get_orden, 200, token=usuario)
    assert vista["estado"] == "LISTA"
    assert vista["piezas"][0]["precioUnitario"] == 80
    ops("POST", "/refacciones/consumo", 204, token=tecnico, data={"refaccionId": pieza, "cantidad": 1})
    avanzar(orden, primera, 40, 403, asesor)
    avanzar(orden, primera, 40, 400, tecnico, None)
    avanzar(orden, primera, 40, token=tecnico)
    assert ops("GET", get_orden, 200, token=usuario)["avance"] == 24
    assert not any(n["tipo"] == "AVANCE_25" for n in ops("GET", "/notificaciones", 200, token=usuario))
    avanzar(orden, primera, 50, token=tecnico)
    avanzar(orden, primera, 50, token=tecnico)
    assert sum(n["tipo"] == "AVANCE_25" for n in ops("GET", "/notificaciones", 200, token=usuario)) == 1
    nueva_fecha = (datetime.now(timezone.utc) + timedelta(days=10)).isoformat()
    ops("PUT", f"{get_orden}/entrega-estimada", 204, token=tecnico, data={"entregaEstimada": nueva_fecha})
    assert ops("GET", get_orden, 200, token=usuario)["avance"] == 30
    avanzar(orden, primera, 100)
    assert ops("GET", get_orden, 200, token=usuario)["avance"] == 60
    ops("POST", "/cancelaciones", 404, token=ajeno, data={"ordenId": orden})
    solicitud = ops("POST", "/cancelaciones", 200, token=usuario, data={"ordenId": orden})["id"]
    assert ops("POST", "/cancelaciones", 200, token=usuario, data={"ordenId": orden})["id"] == solicitud
    resolver = {"solicitudId": solicitud, "aprobar": True}
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
        respuestas = list(pool.map(lambda _: ops("POST", "/cancelaciones/resolver", {204, 409},
                                                 token=gerente, data=resolver)[0], range(2)))
    assert sorted(respuestas) == [204, 409]
    cancelacion = f"/cancelaciones/{solicitud}"
    liquidacion = ops("GET", cancelacion, 200, token=usuario)
    assert liquidacion["estado"] == "APROBADA" and liquidacion["avance"] == 60
    assert (liquidacion["manoObraRealizada"], liquidacion["piezasUsadas"], liquidacion["reembolso"]) == (600, 80, 560)
    assert liquidacion["piezasPorDevolver"][0]["recibidas"] == 3 and liquidacion["reembolsadaEn"] is None
    avanzar(orden, segunda, 100, 409)
    ops("POST", "/refacciones/devolucion", 204, token=asesor,
        data={"refaccionId": pieza, "cantidad": 2, "sellada": True})
    assert not ops("GET", cancelacion, 200, token=usuario)["piezasPorDevolver"]
    for status in (204, 409):
        ops("POST", f"{cancelacion}/reembolso", status, token=gerente, data={"importe": 560, "referencia": "REI-1"})
    assert ops("GET", cancelacion, 200, token=usuario)["reembolsadaEn"]
    assert ops("GET", f"/garantias/orden/{orden}", 200, token=usuario)["estado"] == "SIN_COBERTURA"

    otra, _ = orden_nueva(nueva_revision(gerente, cliente, moto), [50, 50], mano=500)
    t1, t2 = [x["id"] for x in ops("GET", f"/ordenes/{otra}", 200, token=usuario)["tareas"]]
    avanzar(otra, t1, 100)
    avanzar(otra, t2, 20.02)
    tardia = ops("POST", "/cancelaciones", 200, token=usuario, data={"ordenId": otra})["id"]
    rechazada = ops("GET", f"/cancelaciones/{tardia}", 200, token=usuario)
    assert rechazada["estado"] == "RECHAZADA" and rechazada["avance"] == 60.01
    ops("POST", f"/ordenes/{otra}/finalizar", 409, token=gerente)
    avanzar(otra, t2, 100)
    ops("POST", f"/ordenes/{otra}/finalizar", 204, token=gerente)
    ops("POST", "/ordenes/entrega", 204, token=asesor, data={"ordenId": otra})
    garantia = ops("GET", f"/garantias/orden/{otra}", 200, token=usuario)
    assert garantia["estado"] == "VIGENTE" and garantia["iniciaEn"] < garantia["venceEn"]
    ops("GET", f"/garantias/orden/{otra}", 404, token=ajeno)

    concurrente, _ = orden_nueva(nueva_revision(gerente, cliente, moto), [80, 20], mano=500)
    tarea = ops("GET", f"/ordenes/{concurrente}", 200, token=usuario)["tareas"][0]["id"]
    id_solicitud = ops("POST", "/cancelaciones", 200, token=usuario, data={"ordenId": concurrente})["id"]
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
        futuro_avance = pool.submit(avanzar, concurrente, tarea, 100, {204, 409})
        futuro_resolver = pool.submit(ops, "POST", "/cancelaciones/resolver", 204, token=gerente,
                                      data={"solicitudId": id_solicitud, "aprobar": True})
        codigo = futuro_avance.result()[0]
        futuro_resolver.result()
    resultado = ops("GET", f"/cancelaciones/{id_solicitud}", 200, token=usuario)
    assert (codigo, resultado["estado"], resultado["avance"]) in [(204, "RECHAZADA", 80), (409, "APROBADA", 0)]
    return otra


def probar_seguimiento(gerente, cliente, moto, usuario, contexto, orden_entregada):
    """Comprueba historial, validación del mantenimiento, recordatorios y reporte gerencial."""
    _, ajeno, asesor, _, _, moto_ajena = contexto
    historial = api("GET", f"/motocicletas/{moto}/historial", 200, token=usuario)
    assert {x["estadoOrden"] for x in historial} >= {"CANCELADA", "ENTREGADA"}
    assert all(x["tarifaCobrada"] == 280 for x in historial)
    assert any(x["estadoAtencion"] == "SIN_REPARACION"
               for x in api("GET", f"/motocicletas/{moto_ajena}/historial", 200, token=ajeno))
    api("GET", f"/motocicletas/{moto}/historial", 404, token=ajeno)

    ahora = datetime.now(timezone.utc)
    recordar = {"motocicletaId": moto, "descripcion": "Servicio preventivo acordado",
                "fechaEstimada": (ahora + timedelta(days=1)).date().isoformat()}
    completado = ops("POST", "/mantenimientos", 200, token=asesor, data=recordar)["id"]
    pendiente = ops("POST", "/mantenimientos", 200, token=asesor, data=recordar)["id"]
    ops("POST", "/mantenimientos", 400, token=asesor, data={**recordar, "fechaEstimada": None})
    ruta_estado = f"/mantenimientos/{completado}/estado"
    ops("PUT", ruta_estado, 403, token=usuario, data={"realizado": True, "ordenId": orden_entregada})
    ops("PUT", ruta_estado, 400, token=asesor, data={"realizado": True})
    ops("PUT", ruta_estado, 204, token=asesor, data={"realizado": True, "ordenId": orden_entregada})
    estado = {m["id"]: m["estado"] for m in ops("GET", f"/mantenimientos/motocicleta/{moto}", 200, token=usuario)}
    assert estado[completado] == "REALIZADO" and estado[pendiente] == "PENDIENTE"
    assert not ops("GET", f"/mantenimientos/motocicleta/{moto}", 200, token=ajeno)

    def generar(token=gerente):
        """Ejecuta el generador real y obtiene el número de avisos nuevos."""
        return api("POST", "/gerencia/recordatorios/generar", 200, token=token)["generados"]

    hora = ahora.replace(microsecond=0) + timedelta(hours=6)
    datos_franjas = [
        {"inicio": (hora + timedelta(hours=i)).isoformat(),
         "fin": (hora + timedelta(hours=i+1)).isoformat(), "capacidad": 1}
        for i in range(2)
    ]
    franja, destino = [api("POST", "/agenda/franjas", 200, data=datos, token=gerente)["id"]
                        for datos in datos_franjas]
    cita = api("POST", "/citas", 201, token=asesor, data={
        "franjaId": franja, "clienteId": cliente, "motocicletaId": moto, "motivo": "Mantenimiento preventivo"})["id"]
    api("POST", "/gerencia/recordatorios/generar", 403, token=asesor)
    assert not api("GET", "/recordatorios/preferencia", 200, token=usuario)["habilitado"]
    assert generar() == 0
    api("PUT", "/recordatorios/preferencia", 204, token=usuario, data={"habilitado": True})
    with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
        lotes = list(pool.map(lambda _: generar(), range(2)))
    assert sorted(lotes) == [0, 2]
    avisos = api("GET", "/recordatorios", 200, token=usuario)
    assert {a["tipo"]: a["eventoId"] for a in avisos} == {"CITA": cita, "MANTENIMIENTO": pendiente}
    assert all(a["motocicletaId"] == moto and a["servicio"] for a in avisos)
    assert not api("GET", "/recordatorios", 200, token=ajeno)
    api("PUT", f"/citas/{cita}/franja", 204, token=asesor, data={"franjaId": destino})
    assert all(a["tipo"] != "CITA" for a in api("GET", "/recordatorios", 200, token=usuario))
    assert generar() == 1 and generar() == 0
    assert any(a["tipo"] == "CITA" for a in api("GET", "/recordatorios", 200, token=usuario))
    api("DELETE", f"/citas/{cita}", 204, token=asesor)
    assert generar() == 0
    assert all(a["tipo"] != "CITA" for a in api("GET", "/recordatorios", 200, token=usuario))
    api("PUT", "/recordatorios/preferencia", 204, token=usuario, data={"habilitado": False})
    assert not api("GET", "/recordatorios", 200, token=usuario)

    desde = (ahora - timedelta(days=1)).replace(microsecond=0)
    hasta = (ahora + timedelta(days=14)).replace(microsecond=0)
    ruta = f"/gerencia/reporte-operativo?desde={desde.isoformat()}&hasta={hasta.isoformat()}".replace("+00:00", "Z")
    api("GET", ruta, 403, token=asesor)
    reporte = api("GET", ruta, 200, token=gerente)
    indicadores = {(f["modulo"], f["estado"]): f["total"] for f in reporte}
    assert indicadores["ORDENES", "ENTREGADA"] >= 1 and indicadores["MANTENIMIENTOS", "REALIZADO"] >= 1
    comparar_reporte_mysql(indicadores, desde, hasta)
    auditoria = api("GET", "/auditoria?limite=100", 200, token=gerente)
    assert any(a["accion"] == "REEMBOLSO_ENTREGADO" for a in auditoria)
    api("GET", "/auditoria", 403, token=asesor)


def comparar_reporte_mysql(indicadores, desde, hasta):
    """En CI contrasta cada grupo con un conteo SQL independiente del backend."""
    contenedor = os.getenv("FM_TEST_MYSQL_CONTAINER")
    if not contenedor:
        return
    origenes = {
        "CITAS": ("citas JOIN franjas_citas ON citas.franja_id = franjas_citas.id", "inicio"),
        "ORDENES": ("ordenes_servicio", "creada_en"),
        "CANCELACIONES": ("solicitudes_cancelacion", "solicitada_en"),
        "MANTENIMIENTOS": ("mantenimientos_programados", "fecha_estimada"),
    }
    inicio, fin = (d.strftime("%Y-%m-%d %H:%M:%S") for d in (desde, hasta))
    consultas = []
    for modulo, (tabla, fecha) in origenes.items():
        consultas.append(f"SELECT '{modulo}', estado, COUNT(*) FROM {tabla} "
                         f"WHERE {fecha} >= '{inicio}' AND {fecha} < '{fin}' GROUP BY estado;")
    conteos = subprocess.run(
        ["docker", "exec", "-i", contenedor, "sh", "-c",
         'MYSQL_PWD="$MYSQL_PASSWORD" mysql -NB -u "$MYSQL_USER" "$MYSQL_DATABASE"'],
        input="\n".join(consultas), capture_output=True, text=True, check=True, timeout=20)
    filas = [fila.split("\t") for fila in conteos.stdout.splitlines()]
    assert indicadores == {(modulo, estado): int(total) for modulo, estado, total in filas}
    print(f"{len(filas)} grupos del reporte comparados con un conteo MySQL independiente", flush=True)


def main():
    """Ejecuta los escenarios de extremo a extremo con cuentas generadas para esta corrida."""
    if os.environ.get("FM_TEST_CONFIRMAR") != "mysql-de-pruebas":
        sys.exit("Define FM_TEST_CONFIRMAR=mysql-de-pruebas y usa una base dedicada a las pruebas.")
    assert esperar_api()["modo"] == "mysql"
    assert api("GET", "/demo/accesos", 200) == []
    api("GET", "/clientes", 401)
    gerente = inicio(os.environ["FM_TEST_GERENTE"], os.environ["FM_TEST_CLAVE"])["token"]
    cliente, moto, usuario = probar_identidad_y_fichas(gerente)
    contexto = probar_agenda_y_recepcion(gerente, cliente, moto, usuario)
    orden = probar_taller(gerente, cliente, moto, usuario, contexto)
    probar_seguimiento(gerente, cliente, moto, usuario, contexto, orden)
    print(f"{TOTAL} respuestas HTTP verificadas contra MySQL", flush=True)


if __name__ == "__main__":
    main()
