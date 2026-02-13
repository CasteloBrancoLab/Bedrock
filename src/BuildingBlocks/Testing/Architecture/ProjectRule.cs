using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Testing.Architecture;

/// <summary>
/// Classe base abstrata para regras de arquitetura de nivel projeto.
/// Diferente das regras de nivel tipo (que analisam classes/structs via Roslyn),
/// regras de projeto analisam ProjectReferences nos .csproj para validar
/// o grafo de dependencias entre camadas.
/// </summary>
public abstract class ProjectRule : Rule
{
    /// <summary>
    /// Nao aplicavel para regras de projeto — sempre retorna null.
    /// </summary>
    protected sealed override Violation? AnalyzeType(TypeContext context) => null;

    /// <summary>
    /// Override do Analyze da Rule base. Em vez de iterar tipos via Roslyn,
    /// itera os projetos, encontra os .csproj no filesystem, parseia
    /// ProjectReferences e chama <see cref="AnalyzeProjectReferences"/>.
    /// </summary>
    public override IReadOnlyList<RuleAnalysisResult> Analyze(
        IReadOnlyDictionary<string, Compilation> compilations,
        string rootDir)
    {
        var results = new List<RuleAnalysisResult>();
        var allProjectNames = new HashSet<string>(compilations.Keys, StringComparer.OrdinalIgnoreCase);

        foreach (var (projectName, _) in compilations)
        {
            var csprojPath = FindCsprojPath(projectName, rootDir);
            if (csprojPath is null)
                continue;

            var references = ParseProjectReferences(csprojPath);
            var relativeCsprojPath = GetRelativePath(csprojPath, rootDir);

            var context = new ProjectRuleContext
            {
                ProjectName = projectName,
                CsprojRelativePath = relativeCsprojPath,
                DirectProjectReferences = references,
                RootDir = rootDir,
                AllProjectNames = allProjectNames
            };

            var violations = AnalyzeProjectReferences(context);

            var typeResults = violations.Select(v => new TypeAnalysisResult
            {
                TypeName = v.Message,
                TypeFullName = $"{projectName} -> {v.Message}",
                File = relativeCsprojPath,
                Line = v.Line,
                Status = TypeAnalysisStatus.Failed,
                Violation = v
            }).ToList();

            results.Add(new RuleAnalysisResult
            {
                RuleName = Name,
                RuleDescription = Description,
                DefaultSeverity = DefaultSeverity,
                AdrPath = AdrPath,
                ProjectName = projectName,
                TypeResults = typeResults
            });
        }

        return results;
    }

    /// <summary>
    /// Analisa as ProjectReferences de um projeto e retorna violacoes encontradas.
    /// </summary>
    /// <param name="context">Contexto com informacoes do projeto e suas referencias.</param>
    /// <returns>Lista de violacoes encontradas (vazia se nenhuma).</returns>
    protected abstract IReadOnlyList<Violation> AnalyzeProjectReferences(ProjectRuleContext context);

    /// <summary>
    /// Busca o .csproj de um projeto pelo nome no diretorio src/.
    /// </summary>
    public static string? FindCsprojPath(string projectName, string rootDir)
    {
        var srcDir = Path.Combine(rootDir, "src");
        if (!Directory.Exists(srcDir))
            return null;

        var fileName = $"{projectName}.csproj";
        var files = Directory.GetFiles(srcDir, fileName, SearchOption.AllDirectories);

        return files.Length > 0 ? files[0] : null;
    }

    /// <summary>
    /// Parseia um .csproj e retorna os nomes dos projetos referenciados via ProjectReference.
    /// </summary>
    public static IReadOnlyList<string> ParseProjectReferences(string csprojPath)
    {
        var references = new List<string>();

        try
        {
            var doc = XDocument.Load(csprojPath);
            var projectRefs = doc.Descendants("ProjectReference");

            foreach (var pr in projectRefs)
            {
                var include = pr.Attribute("Include")?.Value;
                if (string.IsNullOrEmpty(include))
                    continue;

                // Extrair o nome do projeto do caminho (ex: ..\Domain\ShopDemo.Auth.Domain.csproj -> ShopDemo.Auth.Domain)
                var fileName = Path.GetFileNameWithoutExtension(include);
                if (!string.IsNullOrEmpty(fileName))
                    references.Add(fileName);
            }
        }
        catch
        {
            // Se nao conseguir parsear o .csproj, retorna lista vazia
        }

        return references;
    }
}
