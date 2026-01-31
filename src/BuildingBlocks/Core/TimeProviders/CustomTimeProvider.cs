namespace Bedrock.BuildingBlocks.Core.TimeProviders;

/// <summary>
/// TimeProvider customizável para testes e cenários que requerem controle de tempo.
/// </summary>
/// <remarks>
/// CASOS DE USO:
/// - Testes unitários com tempo controlado/fixo
/// - Simulação de cenários temporais (clock drift, fusos horários)
/// - Injeção de dependência de tempo em serviços
///
/// EXEMPLO DE USO EM TESTES:
/// <code>
/// var fixedTime = new DateTimeOffset(2025, 1, 15, 10, 30, 0, TimeSpan.Zero);
/// var timeProvider = new CustomTimeProvider(
///     utcNowFunc: _ => fixedTime,
///     localTimeZone: TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo")
/// );
/// var service = new MyService(timeProvider);
/// // Agora MyService sempre verá fixedTime como hora atual
/// </code>
/// </remarks>
public sealed class CustomTimeProvider : TimeProvider
{
    private readonly Func<TimeZoneInfo?, DateTimeOffset>? _utcNowFunc;

    /// <summary>
    /// Instância padrão que usa o tempo do sistema.
    /// Equivalente a usar TimeProvider.System, mas com a mesma interface.
    /// </summary>
    public static CustomTimeProvider DefaultInstance { get; } = new(utcNowFunc: null, localTimeZone: null);

    /// <summary>
    /// Fuso horário local configurado para este provider.
    /// </summary>
    public override TimeZoneInfo LocalTimeZone { get; }

    /// <summary>
    /// Cria um CustomTimeProvider com função de tempo e fuso horário customizados.
    /// </summary>
    /// <param name="utcNowFunc">
    /// Função que retorna o tempo UTC atual. Recebe o LocalTimeZone como parâmetro.
    /// Se null, usa DateTimeOffset.UtcNow.
    /// </param>
    /// <param name="localTimeZone">
    /// Fuso horário local. Se null, usa TimeZoneInfo.Utc.
    /// </param>
    public CustomTimeProvider(
        Func<TimeZoneInfo?, DateTimeOffset>? utcNowFunc,
        TimeZoneInfo? localTimeZone)
    {
        _utcNowFunc = utcNowFunc;
        LocalTimeZone = localTimeZone ?? TimeZoneInfo.Utc;
    }

    /// <summary>
    /// Retorna o tempo UTC atual.
    /// </summary>
    /// <returns>
    /// Se utcNowFunc foi fornecido no construtor, retorna o resultado dessa função.
    /// Caso contrário, retorna DateTimeOffset.UtcNow.
    /// </returns>
    public override DateTimeOffset GetUtcNow()
    {
        if (_utcNowFunc != null)
            return _utcNowFunc(LocalTimeZone);

        return DateTimeOffset.UtcNow;
    }
}
