namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;

/// <summary>
/// Representa uma cláusula WHERE type-safe gerada pelo mapper.
/// Só pode ser criada internamente pelo DataModelMapperBase, garantindo segurança contra SQL injection.
/// </summary>
public readonly struct WhereClause
{
    // Fields
    internal readonly string Value;

    // Constructors
    internal WhereClause(string value)
    {
        Value = value;
    }

    // Operators
    public static WhereClause operator &(WhereClause left, WhereClause right)
    {
        return new WhereClause(string.Create(
            left.Value.Length + 5 + right.Value.Length,
            (left.Value, right.Value),
            static (span, state) =>
            {
                int pos = 0;
                state.Item1.AsSpan().CopyTo(span);
                pos += state.Item1.Length;
                " AND ".AsSpan().CopyTo(span[pos..]);
                pos += 5;
                state.Item2.AsSpan().CopyTo(span[pos..]);
            }));
    }

    public static WhereClause operator |(WhereClause left, WhereClause right)
    {
        return new WhereClause(string.Create(
            1 + left.Value.Length + 4 + right.Value.Length + 1,
            (left.Value, right.Value),
            static (span, state) =>
            {
                span[0] = '(';
                int pos = 1;
                state.Item1.AsSpan().CopyTo(span[pos..]);
                pos += state.Item1.Length;
                " OR ".AsSpan().CopyTo(span[pos..]);
                pos += 4;
                state.Item2.AsSpan().CopyTo(span[pos..]);
                pos += state.Item2.Length;
                span[pos] = ')';
            }));
    }

    // Methods
    public override string ToString() => Value;
}
