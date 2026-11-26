namespace FollowMotors.Presentation;

using FollowMotors.Application.Operaciones;

/// <summary>Ejecuta la generación periódica en la instancia local de la API.</summary>
public sealed class RecordatoriosProgramados(IServiceScopeFactory ambitos, ILogger<RecordatoriosProgramados> registro)
    : BackgroundService
{
    /// <summary>Publica avisos al arrancar y cada quince minutos, sin detener las operaciones si falla el canal.</summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var intervalo = new PeriodicTimer(TimeSpan.FromMinutes(15));
        do
        {
            try
            {
                await using var ambito = ambitos.CreateAsyncScope();
                int creados = await ambito.ServiceProvider.GetRequiredService<GeneradorRecordatorios>()
                    .GenerarAsync(stoppingToken);
                registro.LogInformation("Recordatorios internos generados: {Cantidad}", creados);
            }
            catch (Exception error) when (!stoppingToken.IsCancellationRequested)
            {
                registro.LogWarning("No se pudo generar el lote de recordatorios ({TipoError}).", error.GetType().Name);
            }
        }
        while (await intervalo.WaitForNextTickAsync(stoppingToken));
    }
}
