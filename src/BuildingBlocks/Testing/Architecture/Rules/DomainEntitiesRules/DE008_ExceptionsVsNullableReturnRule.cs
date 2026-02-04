using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules;

/// <summary>
/// Regra DE-008: Métodos de entidades não devem lançar exceções para validação de negócio.
/// Validação deve usar retorno nullable + mensagens no ExecutionContext.
/// <para>
/// Exceções são permitidas APENAS para:
/// <list type="bullet">
///   <item>Guard clauses de dependências obrigatórias (<c>ArgumentNullException.ThrowIfNull</c>)</item>
///   <item>Guard clauses de strings (<c>ArgumentException.ThrowIfNullOrWhiteSpace</c>)</item>
/// </list>
/// </para>
/// <para>
/// O que NÃO é permitido:
/// <list type="bullet">
///   <item><c>throw new ValidationException(...)</c></item>
///   <item><c>throw new ArgumentException(...)</c> com mensagem customizada</item>
///   <item><c>throw new DomainException(...)</c></item>
///   <item>Qualquer <c>throw</c> statement ou expression para validação de negócio</item>
/// </list>
/// </para>
/// </summary>
public sealed class DE008_ExceptionsVsNullableReturnRule : DomainEntityRuleBase
{
    // Properties
    public override string Name => "DE008_ExceptionsVsNullableReturn";

    public override string Description =>
        "Entidades não devem lançar exceções para validação de negócio — usar retorno nullable + ExecutionContext (DE-008)";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath =>
        "docs/adrs/domain-entities/DE-008-excecoes-vs-retorno-nullable.md";

    /// <summary>
    /// Nomes de métodos estáticos de guard clause permitidos (ArgumentNullException.ThrowIfNull, etc.).
    /// Esses são guard clauses para dependências obrigatórias, não validação de negócio.
    /// </summary>
    private static readonly HashSet<string> AllowedGuardMethods =
    [
        "ThrowIfNull",
        "ThrowIfNullOrWhiteSpace",
        "ThrowIfNullOrEmpty"
    ];

    protected override Violation? AnalyzeEntityType(TypeContext context)
    {
        var type = context.Type;

        // Analisar cada método procurando throw statements/expressions
        foreach (var member in type.GetMembers())
        {
            if (member is not IMethodSymbol method)
                continue;

            // Ignorar métodos sem corpo (abstratos, extern, partial sem implementação)
            if (method.IsAbstract || method.IsExtern)
                continue;

            // Ignorar métodos herdados de object (ToString, Equals, GetHashCode)
            if (IsObjectMethod(method))
                continue;

            // Ignorar property accessors, construtores implícitos, operators
            if (method.MethodKind != MethodKind.Ordinary &&
                method.MethodKind != MethodKind.Constructor)
                continue;

            // Inspecionar o corpo do método procurando throw não permitido
            var throwLocation = FindDisallowedThrowInMethodBody(method);
            if (throwLocation is not null)
            {
                var lineNumber = throwLocation.GetLineSpan().StartLinePosition.Line + 1;

                return new Violation
                {
                    Rule = Name,
                    Severity = DefaultSeverity,
                    Adr = AdrPath,
                    Project = context.ProjectName,
                    File = context.RelativeFilePath,
                    Line = lineNumber,
                    Message = $"Método '{method.Name}' da classe '{type.Name}' lança exceção para validação de negócio. " +
                              $"Usar retorno nullable + mensagens no ExecutionContext ao invés de throw",
                    LlmHint = $"Remover throw do método '{method.Name}' da classe '{type.Name}'. " +
                              $"Usar retorno nullable (T?) e adicionar mensagens de erro no ExecutionContext " +
                              $"via ValidationUtils ou context.AddErrorMessage(). " +
                              $"Guard clauses (ArgumentNullException.ThrowIfNull) para dependências obrigatórias são permitidas"
                };
            }
        }

        return null;
    }

    /// <summary>
    /// Procura uso de throw statement ou throw expression no corpo de um método,
    /// ignorando guard clauses permitidos (ArgumentNullException.ThrowIfNull, etc.).
    /// </summary>
    /// <returns>Location do primeiro throw não permitido, ou null se não houver.</returns>
    private static Location? FindDisallowedThrowInMethodBody(IMethodSymbol method)
    {
        foreach (var syntaxRef in method.DeclaringSyntaxReferences)
        {
            var syntaxNode = syntaxRef.GetSyntax();

            foreach (var descendant in syntaxNode.DescendantNodes())
            {
                // Verificar throw statements: throw new Exception(...)
                if (descendant is ThrowStatementSyntax throwStatement)
                {
                    // Se não tem guard clause antes, é violação
                    if (!IsPrecededByGuardClause(throwStatement))
                        return throwStatement.ThrowKeyword.GetLocation();
                }

                // Verificar throw expressions: expr ?? throw new Exception(...)
                if (descendant is ThrowExpressionSyntax throwExpr)
                {
                    return throwExpr.ThrowKeyword.GetLocation();
                }

                // Verificar invocações de métodos estáticos de throw
                // (ex: ArgumentNullException.ThrowIfNull que NÃO está na lista de permitidos)
                if (descendant is InvocationExpressionSyntax invocation &&
                    IsDisallowedThrowInvocation(invocation))
                {
                    return invocation.GetLocation();
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Verifica se um throw statement é precedido por um guard clause pattern permitido.
    /// Guard clauses são: if (param == null) throw new ArgumentNullException(...)
    /// </summary>
    private static bool IsPrecededByGuardClause(ThrowStatementSyntax throwStatement)
    {
        // Verificar se o throw está dentro de um if statement
        if (throwStatement.Parent is not BlockSyntax block ||
            block.Parent is not IfStatementSyntax)
        {
            // Pode ser inline: if (...) throw ...
            if (throwStatement.Parent is IfStatementSyntax)
            {
                return IsGuardClauseException(throwStatement.Expression);
            }

            return false;
        }

        return IsGuardClauseException(throwStatement.Expression);
    }

    /// <summary>
    /// Verifica se a expressão do throw é uma exceção de guard clause permitida.
    /// Permitidas: ArgumentNullException, ArgumentException (sem mensagem de negócio).
    /// </summary>
    private static bool IsGuardClauseException(ExpressionSyntax? expression)
    {
        if (expression is not ObjectCreationExpressionSyntax creation)
            return false;

        var typeName = GetSimpleTypeName(creation.Type);

        // ArgumentNullException é sempre guard clause
        return typeName == "ArgumentNullException";
    }

    /// <summary>
    /// Verifica se uma invocação é um ThrowIf* estático NÃO permitido.
    /// Invocações permitidas: ArgumentNullException.ThrowIfNull,
    /// ArgumentException.ThrowIfNullOrWhiteSpace, ArgumentException.ThrowIfNullOrEmpty.
    /// </summary>
    private static bool IsDisallowedThrowInvocation(InvocationExpressionSyntax invocation)
    {
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return false;

        var methodName = memberAccess.Name.Identifier.Text;

        // Se o método é um dos guard clauses permitidos, não é violação
        if (AllowedGuardMethods.Contains(methodName))
            return false;

        // Verificar se é um método Throw* estático (ex: CustomException.ThrowIfInvalid)
        if (methodName.StartsWith("Throw", StringComparison.Ordinal))
            return true;

        return false;
    }

    /// <summary>
    /// Obtém o nome simples de um TypeSyntax (sem namespace).
    /// </summary>
    private static string? GetSimpleTypeName(TypeSyntax type)
    {
        return type switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.Text,
            QualifiedNameSyntax qualified => qualified.Right.Identifier.Text,
            _ => null
        };
    }
}
