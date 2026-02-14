using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-050: Classes abstratas de dominio nao devem expor metodos publicos de negocio.
/// <para>
/// O que e verificado:
/// <list type="bullet">
///   <item>Classes abstratas que herdam de EntityBase nao devem ter metodos publicos
///         de instancia (exceto overrides de EntityBase como Clone)</item>
/// </list>
/// </para>
/// <para>
/// Excecoes (metodos publicos permitidos):
/// <list type="bullet">
///   <item>Metodos estaticos (Validate*, IsValid, RegisterNewBase, etc.)</item>
///   <item>Metodos de override (Clone, ToString, Equals, GetHashCode)</item>
///   <item>Propriedades publicas (sao permitidas)</item>
///   <item>Classes concretas (nao verificadas por esta regra)</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE050_NoPublicBusinessMethodsInAbstractClassesRule : DomainEntitiesGeneralRuleBase
{
    // Properties
    public override string Name => "DE050_NoPublicBusinessMethodsInAbstractClasses";

    public override string Description =>
        "Classes abstratas nao devem expor metodos publicos de negocio. " +
        "Apenas Validate* (publicos estaticos), *Internal (protegidos) e Set* (privados) (DE-050)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-050-classe-abstrata-nao-expoe-metodos-publicos-negocio.md";

    protected override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // So se aplica a classes abstratas
        if (!type.IsAbstract || type.TypeKind != TypeKind.Class)
            return null;

        // So se aplica a classes que herdam de EntityBase
        if (!DomainEntityRuleBase.InheritsFromEntityBase(type))
            return null;

        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            if (method.MethodKind != MethodKind.Ordinary)
                continue;

            if (method.IsImplicitlyDeclared)
                continue;

            // Ignorar metodos estaticos (Validate*, IsValid, RegisterNewBase, etc.)
            if (method.IsStatic)
                continue;

            // Ignorar metodos que nao sao publicos
            if (method.DeclaredAccessibility != Accessibility.Public)
                continue;

            // Ignorar overrides (Clone, ToString, Equals, GetHashCode, etc.)
            if (method.IsOverride)
                continue;

            // Ignorar metodos abstratos (ex: IsValidConcreteInternal pode ser
            // protected abstract, mas se por algum motivo fosse public abstract, seria pego)
            // Na verdade, metodos abstratos publicos de instancia tambem nao devem existir
            // na classe abstrata (exceto overrides), entao vamos reportar

            // Se chegou aqui, e um metodo publico de instancia que nao e override
            return new Violation
            {
                Rule = Name,
                Severity = DefaultSeverity,
                Adr = AdrPath,
                Project = context.ProjectName,
                File = context.RelativeFilePath,
                Line = GetMethodLineNumber(method, context.LineNumber),
                Message = $"A classe abstrata '{type.Name}' tem o metodo publico de instancia " +
                          $"'{method.Name}'. Classes abstratas de dominio nao devem expor " +
                          $"metodos publicos de negocio. Devem fornecer apenas: " +
                          $"Validate* (public static), *Internal (protected), Set* (private)",
                LlmHint = $"Mover a logica do metodo '{method.Name}' para um metodo " +
                          $"*Internal protegido. A classe filha concreta (sealed) deve " +
                          $"definir sua propria API publica usando o *Internal. " +
                          $"Consultar ADR DE-050 para exemplos"
            };
        }

        return null;
    }

    /// <summary>
    /// Obtem o numero da linha onde o metodo e declarado.
    /// </summary>
    private static int GetMethodLineNumber(IMethodSymbol method, int fallbackLineNumber)
    {
        var location = method.Locations.FirstOrDefault(l => l.IsInSource);
        if (location is null)
            return fallbackLineNumber;

        return location.GetLineSpan().StartLinePosition.Line + 1;
    }
}
