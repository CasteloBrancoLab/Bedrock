using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.MessagesRules;

/// <summary>
/// Classe base abstrata para regras de arquitetura aplicaveis a mensagens concretas.
/// <para>
/// Filtra apenas records concretos (sealed) que herdam de MessageBase
/// (via CommandBase, EventBase ou QueryBase). Apos o filtro, delega
/// a analise para <see cref="AnalyzeMessageType"/>.
/// </para>
/// </summary>
public abstract class MessageRuleBase : Rule
{
    public override string Category => "Messages";

    /// <summary>
    /// Nome do tipo base raiz de mensagens.
    /// </summary>
    protected const string MessageBaseTypeName = "MessageBase";

    /// <summary>
    /// Nomes dos tipos base tipados (CommandBase, EventBase, QueryBase).
    /// </summary>
    private static readonly string[] TypedBaseNames = ["CommandBase", "EventBase", "QueryBase"];

    /// <summary>
    /// Filtra tipos nao aplicaveis e delega para <see cref="AnalyzeMessageType"/>.
    /// Aceita apenas: records concretos (nao abstratos) que herdam de MessageBase.
    /// </summary>
    protected sealed override Violation? AnalyzeType(TypeContext context)
    {
        var type = context.Type;

        // Apenas records concretos (sealed record compila como class + IsRecord)
        if (!type.IsRecord || type.IsAbstract || type.TypeKind != TypeKind.Class)
            return null;

        if (!InheritsFromMessageBase(type))
            return null;

        return AnalyzeMessageType(context);
    }

    /// <summary>
    /// Analisa um record concreto de mensagem.
    /// Chamado apenas apos o filtro comum ter sido aplicado.
    /// </summary>
    protected abstract Violation? AnalyzeMessageType(TypeContext context);

    #region Helpers compartilhados

    /// <summary>
    /// Verifica se o tipo herda de MessageBase (direta ou indiretamente).
    /// </summary>
    public static bool InheritsFromMessageBase(INamedTypeSymbol type)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            if (current.Name == MessageBaseTypeName)
                return true;

            current = current.BaseType;
        }

        return false;
    }

    /// <summary>
    /// Retorna o tipo de mensagem (Command, Event, Query) com base na cadeia de heranca.
    /// Retorna null se nao herdar de nenhuma base tipada.
    /// </summary>
    public static string? GetMessageKind(INamedTypeSymbol type)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            foreach (var baseName in TypedBaseNames)
            {
                if (current.Name == baseName)
                    return baseName.Replace("Base", string.Empty);
            }

            current = current.BaseType;
        }

        return null;
    }

    /// <summary>
    /// Retorna o nome do tipo base tipado direto (CommandBase, EventBase, QueryBase)
    /// ou null se herdar diretamente de MessageBase.
    /// </summary>
    public static string? GetTypedBaseName(INamedTypeSymbol type)
    {
        var current = type.BaseType;
        while (current is not null)
        {
            foreach (var baseName in TypedBaseNames)
            {
                if (current.Name == baseName)
                    return baseName;
            }

            current = current.BaseType;
        }

        return null;
    }

    #endregion
}
