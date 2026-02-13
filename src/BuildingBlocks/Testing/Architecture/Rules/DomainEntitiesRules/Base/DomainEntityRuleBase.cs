using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Classe base abstrata para regras de arquitetura aplicáveis a domain entities.
/// <para>
/// Implementa o filtro comum: ignora abstratos, estáticos, records, enums, interfaces
/// e structs. Por padrão, também filtra classes que não herdam de EntityBase
/// (controlado por <see cref="RequiresEntityBaseInheritance"/>).
/// Após o filtro, delega a análise para <see cref="AnalyzeEntityType"/>.
/// </para>
/// <para>
/// Centraliza helpers reutilizáveis para inspeção de métodos Roslyn:
/// <see cref="IsObjectMethod"/>, <see cref="IsValidationMethod"/>,
/// <see cref="ReturnsNullableOfContainingType"/>.
/// </para>
/// </summary>
public abstract class DomainEntityRuleBase : Rule
{
    public override string Category => "Domain Entities";
    /// <summary>
    /// Nome do tipo base genérico de entidades.
    /// </summary>
    protected const string EntityBaseTypeName = "EntityBase";

    /// <summary>
    /// Indica se a regra se aplica apenas a classes que herdam de EntityBase.
    /// Default: <c>true</c>. Regras que se aplicam a todas as classes concretas
    /// devem sobrescrever para <c>false</c>.
    /// </summary>
    protected virtual bool RequiresEntityBaseInheritance => true;

    /// <summary>
    /// Filtra tipos não aplicáveis (abstratos, estáticos, records, não-classes,
    /// e opcionalmente classes que não herdam de EntityBase) e delega para <see cref="AnalyzeEntityType"/>.
    /// </summary>
    protected sealed override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // Ignorar: abstratos, estáticos, records, enums, interfaces, structs
        if (type.IsAbstract || type.IsStatic ||
            type.IsRecord || type.TypeKind != TypeKind.Class)
            return null;

        // Verificar se o tipo herda de EntityBase<T> (quando requerido pela regra)
        if (RequiresEntityBaseInheritance && !InheritsFromEntityBase(type))
            return null;

        return AnalyzeEntityType(context);
    }

    /// <summary>
    /// Analisa uma classe concreta de domínio.
    /// Chamado apenas após o filtro comum ter sido aplicado.
    /// </summary>
    /// <param name="context">Contexto com todas as informações do tipo sendo analisado.</param>
    /// <returns>Uma violação se o tipo viola a regra, ou null.</returns>
    protected abstract Violation? AnalyzeEntityType(TypeContext context);

    #region Helpers compartilhados

    /// <summary>
    /// Verifica se o tipo herda de EntityBase (genérico ou não).
    /// Público para permitir uso por regras que estendem <see cref="Rule"/> diretamente
    /// (ex: regras para classes abstratas que são filtradas por <see cref="DomainEntityRuleBase"/>).
    /// </summary>
    public static bool InheritsFromEntityBase(INamedTypeSymbol type)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            var baseName = current.Name;
            if (baseName == EntityBaseTypeName)
                return true;

            // Verificar também o nome genérico original
            if (current.IsGenericType && current.ConstructedFrom.Name == EntityBaseTypeName)
                return true;

            current = current.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Verifica se o método é um override de método de System.Object
    /// (ToString, Equals, GetHashCode, etc.).
    /// </summary>
    protected static bool IsObjectMethod(IMethodSymbol method)
    {
        if (!method.IsOverride)
            return false;

        var overridden = method.OverriddenMethod;
        while (overridden is not null)
        {
            if (overridden.ContainingType.SpecialType == SpecialType.System_Object)
                return true;
            overridden = overridden.OverriddenMethod;
        }

        return false;
    }

    /// <summary>
    /// Verifica se o método é de validação (Validate*, IsValid).
    /// Métodos de validação retornam bool por design e são exceções a diversas regras.
    /// </summary>
    protected static bool IsValidationMethod(IMethodSymbol method)
    {
        return method.Name.StartsWith("Validate", StringComparison.Ordinal) ||
               method.Name == "IsValid";
    }

    /// <summary>
    /// Verifica se o método retorna o tipo nullable do tipo contendo (T?).
    /// Para reference types, T? é representado com NullableAnnotation.Annotated.
    /// </summary>
    protected static bool ReturnsNullableOfContainingType(IMethodSymbol method, INamedTypeSymbol containingType)
    {
        var returnType = method.ReturnType;

        // Verificar se o retorno é nullable annotation (T?)
        if (returnType.NullableAnnotation == NullableAnnotation.Annotated)
        {
            if (returnType is INamedTypeSymbol namedReturn)
            {
                // Para reference types, T? é representado com NullableAnnotation.Annotated
                // O tipo subjacente deve ser o mesmo tipo da classe
                return SymbolEqualityComparer.Default.Equals(
                    namedReturn.WithNullableAnnotation(NullableAnnotation.NotAnnotated),
                    containingType);
            }
        }

        return false;
    }

    #endregion
}
