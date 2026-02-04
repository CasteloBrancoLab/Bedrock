using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-053: Classes abstratas de dominio com propriedades validaveis devem ter
/// classe Metadata aninhada para definir metadados de validacao.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Classes abstratas que herdam de EntityBase e tem metodos <c>Validate*</c>
///         (indicando que possuem propriedades validaveis) devem ter uma classe
///         <c>*Metadata</c> aninhada e estatica</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (nao verificados por esta regra):
/// <list type="bullet">
///   <item>Classes concretas (verificadas por outras regras)</item>
///   <item>Classes sem metodos Validate* (sem propriedades validaveis)</item>
///   <item>Classes que nao herdam de EntityBase</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE053_MetadataInAbstractClassesRule : Rule
{
    // Properties
    public override string Name => "DE053_MetadataInAbstractClasses";

    public override string Description =>
        "Classes abstratas com propriedades validaveis devem ter classe Metadata aninhada (DE-053)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-053-metadados-validacao-em-classes-abstratas.md";

    /// <summary>
    /// Sufixo esperado para a classe de metadados.
    /// </summary>
    private const string MetadataSuffix = "Metadata";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // So se aplica a classes abstratas
        if (!type.IsAbstract || type.TypeKind != TypeKind.Class)
            return null;

        // So se aplica a classes que herdam de EntityBase
        if (!DomainEntityRuleBase.InheritsFromEntityBase(type))
            return null;

        // Verificar se tem metodos Validate* (indicando propriedades validaveis)
        var hasValidateMethods = false;
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            if (method.IsImplicitlyDeclared)
                continue;

            // Ignorar metodos herdados
            if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, type))
                continue;

            if (method.Name.StartsWith("Validate", StringComparison.Ordinal) &&
                method.IsStatic && method.DeclaredAccessibility == Accessibility.Public)
            {
                hasValidateMethods = true;
                break;
            }
        }

        // Se nao tem Validate*, nao precisa de Metadata
        if (!hasValidateMethods)
            return null;

        // Verificar se tem classe Metadata aninhada
        var expectedMetadataName = type.Name + MetadataSuffix;
        var hasMetadataClass = false;

        foreach (var nestedType in type.GetTypeMembers())
        {
            if (nestedType.Name == expectedMetadataName &&
                nestedType.IsStatic &&
                nestedType.TypeKind == TypeKind.Class)
            {
                hasMetadataClass = true;
                break;
            }
        }

        if (!hasMetadataClass)
        {
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = context.LineNumber,
                Message = $"A classe abstrata '{type.Name}' tem metodos Validate* " +
                          $"(indicando propriedades validaveis) mas nao possui a classe " +
                          $"de metadados '{expectedMetadataName}' aninhada. " +
                          $"Classes abstratas com propriedades validaveis devem definir " +
                          $"seus proprios metadados (Single Source of Truth)",
                LlmHint = $"Adicionar 'public static class {expectedMetadataName}' " +
                          $"dentro de '{type.Name}' com propriedades como " +
                          $"PropertyNameIsRequired, PropertyNameMinLength, PropertyNameMaxLength. " +
                          $"Consultar ADR DE-053 para exemplos"
            };
        }

        return null;
    }
}
