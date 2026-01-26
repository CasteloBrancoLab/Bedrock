using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Core.Sortings.Enums;

namespace Bedrock.BuildingBlocks.Core.Sortings;

/// <summary>
/// Representa informações de ordenação para uma consulta.
/// </summary>
/// <remarks>
/// CARACTERÍSTICAS:
/// - Imutável: readonly struct para performance e segurança
/// - Validado: Field não pode ser nulo ou vazio
/// - Zero allocation: implementa ISpanFormattable para formatação sem alocação
///
/// EXEMPLO DE USO:
///   var sort = SortInfo.Create("FirstName", SortDirection.Ascending);
///
/// USO EM ARRAY (ordenação múltipla):
///   var sorts = new[]
///   {
///       SortInfo.Create("LastName", SortDirection.Ascending),
///       SortInfo.Create("FirstName", SortDirection.Ascending),
///       SortInfo.Create("CreatedAt", SortDirection.Descending)
///   };
///   // Resultado: ORDER BY LastName ASC, FirstName ASC, CreatedAt DESC
///
/// SEGURANÇA:
/// - O valor de Field deve ser validado contra uma whitelist (enum) na camada Infra.Data
/// - Este struct apenas transporta a informação, não valida se o campo é permitido
/// </remarks>
public readonly struct SortInfo : IEquatable<SortInfo>, ISpanFormattable
{
    /// <summary>
    /// Nome do campo para ordenação.
    /// </summary>
    /// <remarks>
    /// Este valor deve ser validado contra uma whitelist de campos permitidos
    /// na camada Infra.Data antes de ser usado em queries.
    /// </remarks>
    public string Field { get; }

    /// <summary>
    /// Direção da ordenação (Ascending ou Descending).
    /// </summary>
    public SortDirection Direction { get; }

    private SortInfo(string field, SortDirection direction)
    {
        Field = field;
        Direction = direction;
    }

    /// <summary>
    /// Cria uma nova instância de SortInfo com validação.
    /// </summary>
    /// <param name="field">Nome do campo para ordenação (não pode ser nulo ou vazio).</param>
    /// <param name="direction">Direção da ordenação.</param>
    /// <returns>Nova instância de SortInfo.</returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando field é nulo ou vazio.
    /// </exception>
    public static SortInfo Create(string field, SortDirection direction)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field, nameof(field));

        return new SortInfo(field, direction);
    }

    /// <summary>
    /// Cria uma instância de SortInfo a partir de valores existentes sem validação.
    /// </summary>
    /// <param name="field">Nome do campo para ordenação.</param>
    /// <param name="direction">Direção da ordenação.</param>
    /// <returns>Nova instância de SortInfo.</returns>
    /// <remarks>
    /// ATENÇÃO: Este método NÃO valida os parâmetros.
    /// Use apenas para reconstruir SortInfo a partir de valores conhecidos/armazenados.
    /// Para criar novas instâncias, use Create().
    /// </remarks>
    public static SortInfo CreateFromExistingInfo(string field, SortDirection direction)
    {
        return new SortInfo(field, direction);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Field, Direction);
    }

    public override bool Equals(object? obj)
    {
        return obj is SortInfo other && Equals(other);
    }

    public bool Equals(SortInfo other)
    {
        return Field == other.Field && Direction == other.Direction;
    }

    /// <summary>
    /// Retorna representação textual no formato "Field Direction".
    /// </summary>
    /// <returns>String no formato "Field Direction" (ex: "FirstName Ascending").</returns>
    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <summary>
    /// Formata o valor usando o formato e provider especificados.
    /// </summary>
    /// <param name="format">Formato (ignorado, apenas default suportado).</param>
    /// <param name="formatProvider">Provider de formatação (ignorado).</param>
    /// <returns>String no formato "Field Direction".</returns>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return string.Create(GetFormattedLength(), this, static (span, state) => state.FormatTo(span));
    }

    /// <summary>
    /// Tenta formatar o valor no span de destino.
    /// </summary>
    /// <param name="destination">Span de destino para escrita.</param>
    /// <param name="charsWritten">Quantidade de caracteres escritos.</param>
    /// <param name="format">Formato (ignorado, apenas default suportado).</param>
    /// <param name="provider">Provider de formatação (ignorado).</param>
    /// <returns>True se formatou com sucesso, false se o destino é pequeno demais.</returns>
    public bool TryFormat(Span<char> destination, out int charsWritten, [StringSyntax(StringSyntaxAttribute.NumericFormat)] ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        var totalLength = GetFormattedLength();

        if (destination.Length < totalLength)
        {
            charsWritten = 0;
            return false;
        }

        FormatTo(destination);
        charsWritten = totalLength;
        return true;
    }

    private int GetFormattedLength()
    {
        return Field.Length + 1 + GetDirectionLength();
    }

    private int GetDirectionLength()
    {
        return Direction == SortDirection.Ascending ? 9 : 10; // "Ascending" or "Descending"
    }

    private void FormatTo(Span<char> destination)
    {
        Field.AsSpan().CopyTo(destination);
        destination[Field.Length] = ' ';
        GetDirectionSpan().CopyTo(destination[(Field.Length + 1)..]);
    }

    private ReadOnlySpan<char> GetDirectionSpan()
    {
        return Direction == SortDirection.Ascending ? "Ascending" : "Descending";
    }

    public static bool operator ==(SortInfo left, SortInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SortInfo left, SortInfo right)
    {
        return !left.Equals(right);
    }
}
