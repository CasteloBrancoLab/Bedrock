namespace Bedrock.BuildingBlocks.Core.PhoneNumbers;

/// <summary>
/// Representa um número de telefone no formato internacional.
/// </summary>
/// <remarks>
/// ESTRUTURA:
/// - CountryCode: Código do país (ex: "55" para Brasil, "1" para EUA)
/// - AreaCode: Código de área/DDD (ex: "11" para São Paulo)
/// - Number: Número local (ex: "999998888")
///
/// FORMATOS SUPORTADOS:
/// - Formatado: +55 (11) 999998888 (para exibição)
/// - E.164: +5511999998888 (para APIs e armazenamento)
///
/// CASOS DE USO:
/// - Cadastro de clientes
/// - Integração com serviços de SMS/WhatsApp
/// - Validação de números de telefone
/// </remarks>
public readonly struct PhoneNumber : IEquatable<PhoneNumber>
{
    /// <summary>
    /// Código do país (sem o sinal +).
    /// Exemplos: "55" (Brasil), "1" (EUA/Canadá), "44" (UK).
    /// </summary>
    public string CountryCode { get; }

    /// <summary>
    /// Código de área ou DDD.
    /// Exemplos: "11" (São Paulo), "21" (Rio de Janeiro), "212" (Nova York).
    /// </summary>
    public string AreaCode { get; }

    /// <summary>
    /// Número local (sem código de país ou área).
    /// Exemplo: "999998888".
    /// </summary>
    public string Number { get; }

    private PhoneNumber(string countryCode, string areaCode, string number)
    {
        CountryCode = countryCode;
        AreaCode = areaCode;
        Number = number;
    }

    /// <summary>
    /// Cria um novo PhoneNumber com os componentes especificados.
    /// </summary>
    /// <param name="countryCode">Código do país (sem +).</param>
    /// <param name="areaCode">Código de área/DDD.</param>
    /// <param name="number">Número local.</param>
    /// <returns>Nova instância de PhoneNumber.</returns>
    public static PhoneNumber CreateNew(string countryCode, string areaCode, string number)
    {
        return new PhoneNumber(countryCode, areaCode, number);
    }

    /// <summary>
    /// Retorna o número formatado para exibição.
    /// Formato: +{CountryCode} ({AreaCode}) {Number}
    /// Exemplo: +55 (11) 999998888
    /// </summary>
    public string ToFormattedString()
    {
        return $"+{CountryCode} ({AreaCode}) {Number}";
    }

    /// <summary>
    /// Retorna o número no formato E.164.
    /// Formato: +{CountryCode}{AreaCode}{Number}
    /// Exemplo: +5511999998888
    /// </summary>
    /// <remarks>
    /// O formato E.164 é o padrão internacional para números de telefone,
    /// amplamente usado em APIs de telecomunicações (Twilio, WhatsApp, etc).
    /// </remarks>
    public string ToE164String()
    {
        return $"+{CountryCode}{AreaCode}{Number}";
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CountryCode, AreaCode, Number);
    }

    public override bool Equals(object? obj)
    {
        return obj is PhoneNumber other && Equals(other);
    }

    public bool Equals(PhoneNumber other)
    {
        return CountryCode == other.CountryCode
            && AreaCode == other.AreaCode
            && Number == other.Number;
    }

    public override string ToString()
    {
        return ToFormattedString();
    }

    public static bool operator ==(PhoneNumber left, PhoneNumber right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(PhoneNumber left, PhoneNumber right)
    {
        return !left.Equals(right);
    }
}
