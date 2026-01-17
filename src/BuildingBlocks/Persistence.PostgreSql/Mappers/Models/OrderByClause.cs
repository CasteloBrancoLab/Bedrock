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
        return new OrderByClause($"{left.Value}, {right.Value}");
    }

    // Methods
    public override string ToString() => Value;
}
