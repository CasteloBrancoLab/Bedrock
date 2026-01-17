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
        return new WhereClause($"{left.Value} AND {right.Value}");
    }

    public static WhereClause operator |(WhereClause left, WhereClause right)
    {
        return new WhereClause($"({left.Value} OR {right.Value})");
    }

    // Methods
    public override string ToString() => Value;
}
