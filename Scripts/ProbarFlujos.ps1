# Prueba de integración de la API real con repositorios temporales de demostración.
# Debe ejecutarse después de iniciar.bat. Nunca utiliza una base de datos real.
$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:5148/api'
$codigo = [Guid]::NewGuid().ToString('N').Substring(0, 8)

function Solicitar([string]$metodo, [string]$ruta, $cuerpo = $null, [string]$token = '') {
    $parametros = @{ Uri = "$base$ruta"; Method = $metodo; ErrorAction = 'Stop' }
    if ($token) { $parametros.Headers = @{ Authorization = "Bearer $token" } }
    if ($null -ne $cuerpo) {
        $parametros.ContentType = 'application/json; charset=utf-8'
        $parametros.Body = ConvertTo-Json -InputObject $cuerpo -Depth 8 -Compress
    }
    return Invoke-RestMethod @parametros
}

try {
    $estado = Solicitar 'GET' '/estado'
    if ($estado.modo -ne 'demostracion') { throw 'La API no esta en modo demostracion.' }
    Write-Host '[OK] API de demostracion' -ForegroundColor Green

    $gerente = Solicitar 'POST' '/autenticacion/iniciar-sesion' @{ identificador = 'gerente@follow.motors'; contrasena = 'Gerente2026!' }
    if ($gerente.rol -ne 'GERENTE') { throw 'El rol esperado no es GERENTE.' }
    Write-Host '[OK] Login gerencial mediante JWT' -ForegroundColor Green
    $token = $gerente.token

    $perfil = Solicitar 'GET' '/gerencia/mi-perfil' $null $token
    if (!$perfil.id) { throw 'El perfil gerencial no fue devuelto.' }
    Write-Host '[OK] Perfil del gerente protegido' -ForegroundColor Green

    $cliente = Solicitar 'POST' '/clientes' @{
        identificador = "ejemplo-$codigo@follow.motors"; contrasena = 'ClienteTemporal2026!'
        nombres = 'Cliente'; apellidos = 'Prueba'; telefono = '4271000000'
    } $token
    if (!$cliente.id) { throw 'No se creo el cliente.' }
    Write-Host "[OK] Alta de cliente #$($cliente.id)" -ForegroundColor Green

    $moto = Solicitar 'POST' '/motocicletas' @{
        clienteId = $cliente.id; numeroSerie = "VIN-DEMO-$codigo"; marca = 'Honda'
        modelo = 'CB190'; anio = 2023; placa = 'DEMO123'; color = 'Rojo'; kilometrajeActual = 5120
    } $token
    if (!$moto.id) { throw 'No se registro la motocicleta.' }
    Write-Host "[OK] Vinculacion de motocicleta #$($moto.id)" -ForegroundColor Green

    $personal = Solicitar 'GET' '/personal' $null $token
    $mecanico = $personal | Where-Object { $_.esMecanico -eq $true } | Select-Object -First 1
    if (!$mecanico) { throw 'No existe mecanico de prueba.' }
    Write-Host '[OK] Listado de personal y mecánico' -ForegroundColor Green

    $atencion = Solicitar 'POST' '/atenciones' @{
        clienteId = $cliente.id; motocicletaId = $moto.id; mecanicoId = $mecanico.id
        motivo = 'Revision de demostracion'; kilometrajeIngreso = 5120; citaId = $null
    } $token
    if (!$atencion.id) { throw 'No se creo la atención.' }
    Write-Host "[OK] Recepcion de atencion #$($atencion.id)" -ForegroundColor Green

    Solicitar 'POST' "/atenciones/$($atencion.id)/diagnostico" @{ hallazgos = 'Se reviso freno delantero y nivel de aceite.' } $token | Out-Null
    $detalle = Solicitar 'GET' "/atenciones/$($atencion.id)" $null $token
    if ($detalle.estado -ne 'DIAGNOSTICADA') { throw 'El estado no cambio a diagnosticada.' }
    Write-Host '[OK] Diagnostico y consulta del estado' -ForegroundColor Green

    $pago = Solicitar 'POST' "/atenciones/$($atencion.id)/cobro" @{ referenciaComprobante = "REC-$codigo" } $token
    if (!$pago.id) { throw 'No se registro el cobro.' }
    Write-Host '[OK] Cobro de revision a tarifa fija' -ForegroundColor Green

    $audit = Solicitar 'GET' '/auditoria?limite=50' $null $token
    if (($audit | Measure-Object).Count -lt 3) { throw 'No se encontro auditoria suficiente.' }
    Write-Host '[OK] Auditoria gerencial' -ForegroundColor Green

    $clienteLogin = Solicitar 'POST' '/autenticacion/iniciar-sesion' @{
        identificador = "ejemplo-$codigo@follow.motors"; contrasena = 'ClienteTemporal2026!'
    }
    $expediente = Solicitar 'GET' '/clientes/mi-expediente' $null $clienteLogin.token
    if ($expediente.cliente.id -ne $cliente.id) { throw 'El cliente obtuvo otro expediente.' }
    Write-Host '[OK] Consulta de expediente privado del cliente' -ForegroundColor Green

    Write-Host "`nTODOS LOS FLUJOS EJECUTADOS CORRECTAMENTE" -ForegroundColor Green
    exit 0
} catch {
    Write-Host "`n[ERROR] $($_.Exception.Message)" -ForegroundColor Red
    Write-Host 'Verifica que iniciar.bat haya arrancado la API y que no existan errores de compilacion.'
    exit 1
}
