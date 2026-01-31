using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture;

/// <summary>
/// Contexto passado para <see cref="Rule.AnalyzeType"/> com todas as informações
/// do tipo sendo analisado e o contexto global pré-computado pela base.
/// </summary>
public sealed class TypeContext
{
    /// <summary>
    /// Nome do projeto que contém o tipo.
    /// </summary>
    public required string ProjectName { get; init; }

    /// <summary>
    /// Compilação do projeto.
    /// </summary>
    public required Compilation Compilation { get; init; }

    /// <summary>
    /// Símbolo do tipo sendo analisado.
    /// </summary>
    public required INamedTypeSymbol Type { get; init; }

    /// <summary>
    /// Localização do tipo no código-fonte.
    /// </summary>
    public required Location Location { get; init; }

    /// <summary>
    /// Caminho relativo do arquivo fonte em relação ao rootDir.
    /// </summary>
    public required string RelativeFilePath { get; init; }

    /// <summary>
    /// Número da linha onde o tipo é declarado.
    /// </summary>
    public required int LineNumber { get; init; }

    /// <summary>
    /// Diretório raiz do repositório.
    /// </summary>
    public required string RootDir { get; init; }

    /// <summary>
    /// Set global de tipos herdados (fully qualified names + simple names).
    /// Pré-computado pela base a partir de todas as compilações + grep fallback no source.
    /// Útil para regras que precisam saber se um tipo é herdado por outro.
    /// </summary>
    public required IReadOnlySet<string> GlobalInheritedTypes { get; init; }
}
