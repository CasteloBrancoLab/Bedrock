using Bedrock.BuildingBlocks.Core.Sortings.Enums;

namespace Bedrock.BuildingBlocks.Core.Sortings;

/// <summary>
/// Representa informações de ordenação para uma consulta.
/// </summary>
/// <remarks>
/// CARACTERÍSTICAS:
/// - Imutável: readonly struct para performance e segurança
/// - Validado: Field não pode ser nulo ou vazio
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
public readonly struct SortInfo : IEquatable<SortInfo>
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

    public override string ToString()
    {
        return $"{Field} {Direction}";
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
