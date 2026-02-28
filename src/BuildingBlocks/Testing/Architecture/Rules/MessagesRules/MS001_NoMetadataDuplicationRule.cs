namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Regra MS-001: Mensagens concretas nao devem duplicar campos do MessageMetadata
/// (CorrelationId, TenantCode, ExecutionUser, etc.).
/// </summary>
public sealed class MS001_NoMetadataDuplicationRule : MessageRuleBase
{
    public override string Name => "MS001_NoMetadataDuplication";

    public override string Description =>
        "Mensagens concretas nao devem duplicar campos de MessageMetadata (MS-001)";

    public override Severity DefaultSeverity => Severity.Error;
    public override string AdrPath => "docs/adrs/messages/MS-001-envelope-encapsulado-message-metadata.md";

    /// <summary>
    /// Nomes dos parametros que pertencem exclusivamente ao MessageMetadata
    /// e nao devem ser duplicados em mensagens concretas.
    /// </summary>
    private static readonly HashSet<string> ForbiddenParameterNames = new(StringComparer.Ordinal)
    {
        "MessageId", "Timestamp", "SchemaName",
        "CorrelationId", "TenantCode",
        "ExecutionUser", "ExecutionOrigin", "BusinessOperationCode"
    };

    protected override Violation? AnalyzeMessageType(TypeContext context)
    {
        var type = context.Type;

        // Obter o construtor primario (parametros posicionais do record)
        var primaryCtor = type.InstanceConstructors
            .FirstOrDefault(c => c.Parameters.Length > 0 && !c.IsImplicitlyDeclared);

        primaryCtor ??= type.InstanceConstructors
            .Where(c => c.Parameters.Length > 0)
            .OrderByDescending(c => c.Parameters.Length)
            .FirstOrDefault();

        if (primaryCtor is null)
            return null;

        // Verificar todos os parametros exceto o primeiro (Metadata)
        foreach (var param in primaryCtor.Parameters.Skip(1))
        {
            if (ForbiddenParameterNames.Contains(param.Name))
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = context.LineNumber,
                    Message = $"O parametro '{param.Name}' de '{type.Name}' duplica um campo de MessageMetadata. " +
                              $"Esses campos ja estao no Metadata e nao devem ser repetidos",
                    LlmHint = $"Remover o parametro '{param.Name}' de '{type.Name}'. " +
                              $"Esse dado ja esta disponivel via Metadata.{param.Name}"
                };
            }
        }

        return null;
    }
}
