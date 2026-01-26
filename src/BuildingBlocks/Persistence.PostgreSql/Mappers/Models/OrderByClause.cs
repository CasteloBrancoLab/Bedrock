namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;

/// <summary>
/// Representa uma cláusula ORDER BY type-safe gerada pelo mapper.
/// Só pode ser criada internamente pelo DataModelMapperBase, garantindo segurança contra SQL injection.
/// </summary>
public readonly struct OrderByClause
{
    // Fields
    internal readonly string Value;

    // Constructors
    internal OrderByClause(string value)
    {
        Value = value;
    }

    // Operators
    public static OrderByClause operator +(OrderByClause left, OrderByClause right)
    {
        return new OrderByClause(string.Create(
            left.Value.Length + 2 + right.Value.Length,
            (left.Value, right.Value),
            static (span, state) =>
            {
                int pos = 0;
                state.Item1.AsSpan().CopyTo(span);
                pos += state.Item1.Length;
                ", ".AsSpan().CopyTo(span[pos..]);
                pos += 2;
                state.Item2.AsSpan().CopyTo(span[pos..]);
            }));
    }

    // Methods
    public override string ToString() => Value;
}
