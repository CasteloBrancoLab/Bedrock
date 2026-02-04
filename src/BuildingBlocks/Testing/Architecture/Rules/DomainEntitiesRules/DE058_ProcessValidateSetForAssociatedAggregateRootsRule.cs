using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-058: Entidades com Aggregate Roots associadas (1:1) devem ter
/// metodos Process*Internal, Validate*For*Internal e Set* para cada AR associada.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Entidades com propriedades cujo tipo herda de EntityBase (nao colecao)
///         devem ter pelo menos um metodo <c>Process*Internal</c> correspondente</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Propriedades em colecoes (List&lt;T&gt; — verificadas por DE-040)</item>
///   <item>Classes abstratas, estaticas, records</item>
///   <item>Propriedades herdadas</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE058_ProcessValidateSetForAssociatedAggregateRootsRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE058_ProcessValidateSetForAssociatedAggregateRoots";

    public override string Description =>
        "Entidades com ARs associadas devem ter metodos Process*Internal correspondentes (DE-058)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-058-padroes-process-validate-set-para-aggregate-roots-associadas.md";

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Encontrar propriedades cujo tipo herda de EntityBase (associacao 1:1)
        foreach (var member in type.GetMembers())
        {
            if (member is not IPropertySymbol property)
                continue;

            if (property.IsStatic || property.IsImplicitlyDeclared)
                continue;

            // Ignorar propriedades herdadas
            if (!SymbolEqualityComparer.Default.Equals(property.ContainingType, type))
                continue;

            // Verificar se o tipo da propriedade herda de EntityBase
            var propType = property.Type;

            // Remover nullable
            if (propType is INamedTypeSymbol namedPropType &&
                propType.NullableAnnotation == NullableAnnotation.Annotated)
            {
                propType = namedPropType.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            }

            if (propType is not INamedTypeSymbol entityType)
                continue;

            // Nao e uma colecao (List<T> e tratado por DE-040)
            if (entityType.IsGenericType && entityType.Name == "List")
                continue;

            // Verificar se herda de EntityBase
            if (!InheritsFromEntityBase(entityType))
                continue;

            // Esta e uma AR associada. Verificar se tem Process*Internal
            var propertyName = property.Name;
            var hasProcessMethod = false;

            foreach (var m in type.GetMembers())
            {
                if (m is not IMethodSymbol method)
                    continue;

                if (method.MethodKind != MethodKind.Ordinary)
                    continue;

                // Procurar Process*[PropertyName]*Internal
                if (method.Name.StartsWith("Process", StringComparison.Ordinal) &&
                    method.Name.EndsWith("Internal", StringComparison.Ordinal) &&
                    method.Name.Contains(propertyName, StringComparison.Ordinal))
                {
                    hasProcessMethod = true;
                    break;
                }
            }

            if (!hasProcessMethod)
            {
                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = context.LineNumber,
                    Message = $"A classe '{type.Name}' tem a propriedade " +
                              $"'{propertyName}' (AR associada) mas nao tem " +
                              $"metodo 'Process{propertyName}For*Internal'. " +
                              $"Aggregate Roots associadas devem seguir o padrao " +
                              $"Process/Validate/Set, assim como entidades filhas",
                    LlmHint = $"Adicionar metodo(s) " +
                              $"'Process{propertyName}For*Internal' a classe " +
                              $"'{type.Name}'. Cada operacao (RegisterNew, Change) " +
                              $"deve ter seu proprio Process*Internal. " +
                              $"Consultar ADR DE-058 para exemplos"
                };
            }
        }

        return null;
    }
}
