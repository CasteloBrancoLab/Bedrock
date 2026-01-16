using Bedrock.BuildingBlocks.Core.Filterings.Enums;

namespace Bedrock.BuildingBlocks.Core.Filterings;

/// <summary>
/// Representa informações de filtro para uma consulta.
/// </summary>
/// <remarks>
/// CARACTERÍSTICAS:
/// - Imutável: readonly struct para performance e segurança
/// - Validado: Field não pode ser nulo ou vazio
/// - Flexível: Suporta valor único, intervalo (Between) e lista (In/NotIn)
///
/// EXEMPLO DE USO (valor único):
///   var filter = FilterInfo.Create("LastName", FilterOperator.Contains, "Silva");
///
/// EXEMPLO DE USO (intervalo - Between):
///   var filter = FilterInfo.CreateBetween("CreatedAt", "2024-01-01", "2024-12-31");
///
/// EXEMPLO DE USO (lista - In):
///   var filter = FilterInfo.CreateIn("Status", new[] { "Active", "Pending" });
///
/// SEGURANÇA:
/// - O valor de Field deve ser validado contra uma whitelist (enum) na camada Infra.Data
/// - Este struct apenas transporta a informação, não valida se o campo é permitido
/// </remarks>
public readonly struct FilterInfo : IEquatable<FilterInfo>
{
    /// <summary>
    /// Nome do campo para filtro.
    /// </summary>
    /// <remarks>
    /// Este valor deve ser validado contra uma whitelist de campos permitidos
    /// na camada Infra.Data antes de ser usado em queries.
    /// </remarks>
    public string Field { get; }

    /// <summary>
    /// Operador de filtro (Equals, Contains, GreaterThan, etc.).
    /// </summary>
    public FilterOperator Operator { get; }

    /// <summary>
    /// Valor do filtro (para operadores de valor único).
    /// </summary>
    /// <remarks>
    /// Usado para: Equals, NotEquals, Contains, StartsWith, EndsWith,
    /// GreaterThan, GreaterThanOrEquals, LessThan, LessThanOrEquals.
    /// Para Between, este é o valor inicial do intervalo.
    /// </remarks>
    public string? Value { get; }

    /// <summary>
    /// Valor final do intervalo (apenas para operador Between).
    /// </summary>
    public string? ValueEnd { get; }

    /// <summary>
    /// Lista de valores (apenas para operadores In e NotIn).
    /// </summary>
    public IReadOnlyList<string>? Values { get; }

    private FilterInfo(
        string field,
        FilterOperator filterOperator,
        string? value,
        string? valueEnd,
        IReadOnlyList<string>? values)
    {
        Field = field;
        Operator = filterOperator;
        Value = value;
        ValueEnd = valueEnd;
        Values = values;
    }

    /// <summary>
    /// Cria um filtro com valor único.
    /// </summary>
    /// <param name="field">Nome do campo para filtro.</param>
    /// <param name="filterOperator">Operador de filtro.</param>
    /// <param name="value">Valor do filtro.</param>
    /// <returns>Nova instância de FilterInfo.</returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando field é nulo ou vazio.
    /// </exception>
    public static FilterInfo Create(string field, FilterOperator filterOperator, string? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field, nameof(field));

        return new FilterInfo(field, filterOperator, value, null, null);
    }

    /// <summary>
    /// Cria um filtro de intervalo (Between).
    /// </summary>
    /// <param name="field">Nome do campo para filtro.</param>
    /// <param name="valueStart">Valor inicial do intervalo.</param>
    /// <param name="valueEnd">Valor final do intervalo.</param>
    /// <returns>Nova instância de FilterInfo com operador Between.</returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando field é nulo ou vazio.
    /// </exception>
    public static FilterInfo CreateBetween(string field, string? valueStart, string? valueEnd)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field, nameof(field));

        return new FilterInfo(field, FilterOperator.Between, valueStart, valueEnd, null);
    }

    /// <summary>
    /// Cria um filtro de lista (In).
    /// </summary>
    /// <param name="field">Nome do campo para filtro.</param>
    /// <param name="values">Lista de valores.</param>
    /// <returns>Nova instância de FilterInfo com operador In.</returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando field é nulo ou vazio.
    /// </exception>
    public static FilterInfo CreateIn(string field, IReadOnlyList<string> values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field, nameof(field));

        return new FilterInfo(field, FilterOperator.In, null, null, values);
    }

    /// <summary>
    /// Cria um filtro de lista negativa (NotIn).
    /// </summary>
    /// <param name="field">Nome do campo para filtro.</param>
    /// <param name="values">Lista de valores a excluir.</param>
    /// <returns>Nova instância de FilterInfo com operador NotIn.</returns>
    /// <exception cref="ArgumentException">
    /// Lançada quando field é nulo ou vazio.
    /// </exception>
    public static FilterInfo CreateNotIn(string field, IReadOnlyList<string> values)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(field, nameof(field));

        return new FilterInfo(field, FilterOperator.NotIn, null, null, values);
    }

    /// <summary>
    /// Cria uma instância de FilterInfo a partir de valores existentes sem validação.
    /// </summary>
    /// <remarks>
    /// ATENÇÃO: Este método NÃO valida os parâmetros.
    /// Use apenas para reconstruir FilterInfo a partir de valores conhecidos/armazenados.
    /// Para criar novas instâncias, use Create(), CreateBetween(), CreateIn() ou CreateNotIn().
    /// </remarks>
    public static FilterInfo CreateFromExistingInfo(
        string field,
        FilterOperator filterOperator,
        string? value,
        string? valueEnd,
        IReadOnlyList<string>? values)
    {
        return new FilterInfo(field, filterOperator, value, valueEnd, values);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Field, Operator, Value, ValueEnd);
    }

    public override bool Equals(object? obj)
    {
        return obj is FilterInfo other && Equals(other);
    }

    public bool Equals(FilterInfo other)
    {
        return Field == other.Field
            && Operator == other.Operator
            && Value == other.Value
            && ValueEnd == other.ValueEnd;
    }

    public override string ToString()
    {
        return Operator switch
        {
            FilterOperator.Between => $"{Field} {Operator} [{Value}, {ValueEnd}]",
            FilterOperator.In or FilterOperator.NotIn => $"{Field} {Operator} [{string.Join(", ", Values ?? [])}]",
            _ => $"{Field} {Operator} {Value}"
        };
    }

    public static bool operator ==(FilterInfo left, FilterInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(FilterInfo left, FilterInfo right)
    {
        return !left.Equals(right);
    }
}
