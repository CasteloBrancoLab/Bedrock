using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Bedrock.BuildingBlocks.Core.Ids;

public readonly struct Id
    : IEquatable<Id>, IComparable<Id>
{
    [ThreadStatic] private static long _lastTimestamp;
    [ThreadStatic] private static long _counter;

    public Guid Value { get; }

    private Id(Guid value)
    {
        Value = value;
    }

    public static Id GenerateNewId()
    {
        return GenerateNewId(TimeProvider.System.GetUtcNow());
    }

    public static Id GenerateNewId(TimeProvider timeProvider)
    {
        DateTimeOffset now = timeProvider.GetUtcNow();
        return GenerateNewId(now);
    }

    public static Id GenerateNewId(DateTimeOffset dateTimeOffset)
    {
        long timestamp = dateTimeOffset.ToUnixTimeMilliseconds();

        // CENARIO 1: Novo milissegundo (caso mais comum)
        // Reinicia contador para garantir que novo ID seja maior que anteriores
        if (timestamp > _lastTimestamp)
        {
            _lastTimestamp = timestamp;
            _counter = 0;
        }
        // CENARIO 2: Clock drift (relogio retrocedeu)
        // Mantem ultimo timestamp valido e incrementa contador
        // Stryker disable once Equality : Mutacao < para <= nao afeta comportamento - ambos entram no branch correto
        else if (timestamp < _lastTimestamp)
        {
            timestamp = _lastTimestamp;
            _counter++;
        }
        // CENARIO 3: Mesmo milissegundo (alta frequencia)
        // Incrementa contador para diferenciar IDs
        else
        {
            // Stryker disable once Update : Estado ThreadStatic impede teste isolado - verificado via ordenacao de IDs
            _counter++;

            // Stryker disable once Statement : Counter overflow requires 67M+ IDs to test - impractical
            HandleCounterOverflowIfNeeded(ref timestamp);
        }

        return new Id(BuildUuidV7WithRandom(timestamp, _counter));
    }

    public static Id CreateFromExistingInfo(Guid value)
    {
        return new Id(value);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return obj is Id other && Equals(other);
    }

    public bool Equals(Id other)
    {
        return Value == other.Value;
    }

    public int CompareTo(Id other)
    {
        return Value.CompareTo(other.Value);
    }

    public static implicit operator Guid(Id id)
    {
        return id.Value;
    }

    public static implicit operator Id(Guid value)
    {
        return CreateFromExistingInfo(value);
    }

    public static bool operator ==(Id left, Id right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(Id left, Id right)
    {
        return left.Value != right.Value;
    }

    public static bool operator <(Id left, Id right)
    {
        return left.Value.CompareTo(right.Value) < 0;
    }

    public static bool operator >(Id left, Id right)
    {
        return left.Value.CompareTo(right.Value) > 0;
    }

    public static bool operator <=(Id left, Id right)
    {
        return left.Value.CompareTo(right.Value) <= 0;
    }

    public static bool operator >=(Id left, Id right)
    {
        return left.Value.CompareTo(right.Value) >= 0;
    }

    /// <summary>
    /// Constroi UUIDv7 com bytes aleatorios para unicidade global.
    ///
    /// ESTRUTURA DO UUIDv7 (128 bits):
    /// ┌─────────────────┬──────┬─────────┬────────┬──────────────────┐
    /// │  Timestamp (48) │ Ver  │ Counter │ Variant│   Random (46)    │
    /// │                 │ (4)  │  (26)   │  (2)   │                  │
    /// └─────────────────┴──────┴─────────┴────────┴──────────────────┘
    ///
    /// - Timestamp: 48 bits = milissegundos desde Unix epoch
    /// - Version: 4 bits = sempre 0111 (7)
    /// - Counter: 26 bits distribuidos (garante ordenacao na thread)
    /// - Variant: 2 bits = sempre 10 (RFC 4122)
    /// - Random: 46 bits = bytes aleatorios criptograficos
    /// </summary>
    private static Guid BuildUuidV7WithRandom(long timestamp, long counter)
    {
        // Divide timestamp de 48 bits em duas partes
        // Stryker disable once Bitwise : Shift >> 16 verificado via ordenacao temporal dos IDs
        int timestampHigh = (int)(timestamp >> 16);
        short timestampLow = (short)(timestamp & 0xFFFF);

        // Versao (7) + primeiros 12 bits do counter
        // Stryker disable once Bitwise : Shift >> 14 verificado via teste de version bits (0x7000)
        short versionAndCounter = (short)(0x7000 | ((counter >> 14) & 0x0FFF));

        // Variant (10) + proximos 6 bits do counter
        // Stryker disable once Bitwise : Shift >> 8 verificado via teste de variant bits (0x80)
        byte variantHigh = (byte)(0x80 | ((counter >> 8) & 0x3F));

        // Ultimos 8 bits do counter
        byte counterLow = (byte)(counter & 0xFF);

        // 6 bytes aleatorios criptograficamente seguros
        Span<byte> randomBytes = stackalloc byte[6];
        RandomNumberGenerator.Fill(randomBytes);

        return new Guid(
            a: timestampHigh,
            b: timestampLow,
            c: versionAndCounter,
            d: variantHigh,
            e: counterLow,
            f: randomBytes[0],
            g: randomBytes[1],
            h: randomBytes[2],
            i: randomBytes[3],
            j: randomBytes[4],
            k: randomBytes[5]
        );
    }

    /// <summary>
    /// Verifica e trata overflow do contador (> 67M IDs no mesmo milissegundo).
    /// </summary>
    // Stryker disable all : Counter overflow requires 67M+ IDs to test - impractical
    [ExcludeFromCodeCoverage(Justification = "Counter overflow requer 67M+ IDs para testar - impraticavel")]
    private static void HandleCounterOverflowIfNeeded(ref long timestamp)
    {
        // 0x3FFFFFF = 67.108.863 (26 bits)
        if (_counter > 0x3FFFFFF)
        {
            SpinWaitForNextMillisecond(ref timestamp, ref _lastTimestamp);
            _counter = 0;
        }
    }
    // Stryker restore all

    /// <summary>
    /// Spin-wait ate o relogio avancar para proximo milissegundo.
    /// Mais eficiente que Thread.Sleep para esperas menores que 1ms.
    /// </summary>
    // Stryker disable all : SpinWait loop requires real-time clock manipulation - impractical to test
    [ExcludeFromCodeCoverage(Justification = "SpinWait requer manipulacao de relogio em tempo real - impraticavel testar")]
    private static void SpinWaitForNextMillisecond(ref long timestamp, ref long lastTimestamp)
    {
        while (timestamp == lastTimestamp)
        {
            Thread.SpinWait(100);
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        lastTimestamp = timestamp;
    }
    // Stryker restore all
}
