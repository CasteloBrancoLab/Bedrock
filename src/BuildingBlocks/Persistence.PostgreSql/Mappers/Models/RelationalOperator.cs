namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;

/// <summary>
/// Operadores relacionais validados para uso em cláusulas WHERE.
/// Whitelist de operadores seguros contra SQL injection.
/// </summary>
public readonly struct RelationalOperator
{
    // Fields
    private readonly string _value;

    // Predefined operators
    public static readonly RelationalOperator Equal = new("=");
    public static readonly RelationalOperator NotEqual = new("<>");
    public static readonly RelationalOperator GreaterThan = new(">");
    public static readonly RelationalOperator GreaterThanOrEqual = new(">=");
    public static readonly RelationalOperator LessThan = new("<");
    public static readonly RelationalOperator LessThanOrEqual = new("<=");
    public static readonly RelationalOperator Like = new("LIKE");
    public static readonly RelationalOperator ILike = new("ILIKE");
    public static readonly RelationalOperator IsNull = new("IS NULL");
    public static readonly RelationalOperator IsNotNull = new("IS NOT NULL");

    // Constructors
    private RelationalOperator(string value)
    {
        _value = value;
    }

    // Methods
    public override string ToString() => _value;

    internal string ToSql() => _value;
}
