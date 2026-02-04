using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-057: Metadata de Aggregate Roots associadas deve ter apenas IsRequired.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Para propriedades cujo tipo herda de EntityBase (associacao 1:1, nao colecao),
///         a classe Metadata nao deve conter campos Min/MaxLength ou Min/MaxValue
///         alem de IsRequired e PropertyName</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Propriedades de tipos primitivos/string (podem ter Min/Max)</item>
///   <item>Propriedades de colecoes de entidades (List&lt;T&gt;)</item>
///   <item>Classes abstratas, estaticas, records</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE057_AssociatedAggregateRootMetadataOnlyIsRequiredRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE057_AssociatedAggregateRootMetadataOnlyIsRequired";

    public override string Description =>
        "Metadata de Aggregate Roots associadas deve ter apenas IsRequired e PropertyName (DE-057)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-057-metadata-aggregate-roots-associadas-apenas-isrequired.md";

    /// <summary>
    /// Sufixos de metadata proibidos para propriedades de AR associada.
    /// </summary>
    private static readonly string[] ForbiddenMetadataSuffixes =
        ["MinLength", "MaxLength", "MinValue", "MaxValue"];

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

            // Nao e uma colecao (List<T> e tratado por outras regras)
            if (entityType.IsGenericType && entityType.Name == "List")
                continue;

            // Verificar se herda de EntityBase
            if (!InheritsFromEntityBase(entityType))
                continue;

            // Esta e uma AR associada. Verificar se o Metadata tem campos proibidos
            var propertyName = property.Name;
            var metadataClassName = type.Name + "Metadata";

            foreach (var nestedType in type.GetTypeMembers())
            {
                if (nestedType.Name != metadataClassName)
                    continue;

                // Verificar membros do Metadata para campos proibidos
                foreach (var metaMember in nestedType.GetMembers())
                {
                    if (metaMember is not IPropertySymbol metaProp)
                        continue;

                    if (!metaProp.Name.StartsWith(propertyName, StringComparison.Ordinal))
                        continue;

                    foreach (var suffix in ForbiddenMetadataSuffixes)
                    {
                        if (metaProp.Name == propertyName + suffix)
                        {
                            return new Violation
                            {
                                Rule = Name,
                                Severity = DefaultSeverity,
                                Adr = AdrPath,
                                Project = context.ProjectName,
                                File = context.RelativeFilePath,
                                Line = context.LineNumber,
                                Message = $"A classe '{type.Name}' tem metadata " +
                                          $"'{metaProp.Name}' para a Aggregate Root " +
                                          $"associada '{propertyName}'. " +
                                          $"Aggregate Roots associadas devem ter apenas " +
                                          $"'{propertyName}IsRequired' e " +
                                          $"'{propertyName}PropertyName' no metadata. " +
                                          $"A AR associada tem seu proprio ciclo de vida " +
                                          $"e suas proprias validacoes",
                                LlmHint = $"Remover '{metaProp.Name}' do metadata de " +
                                          $"'{type.Name}'. Para ARs associadas, a unica " +
                                          $"metadata relevante e IsRequired. " +
                                          $"Consultar ADR DE-057 para detalhes"
                            };
                        }
                    }
                }
            }
        }

        return null;
    }
}
