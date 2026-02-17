using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using static Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules.IN001_CanonicalLayerDependenciesRule;

namespace Bedrock.BuildingBlocks.Testing.Architecture.Rules.InfrastructureRules;

/// <summary>
/// IN-018: Valida que projetos de bounded context estao posicionados
/// nos solution folders canonicos dentro do arquivo .sln.
///
/// Os folders seguem a numeracao definida em IN-001:
/// 1 - Api, 2 - Application, 3 - Domain, 4 - Infra (4.1 - Data, 4.2 - CrossCutting).
/// </summary>
public sealed class IN018_CanonicalSolutionFoldersRule : InfrastructureTypeRuleBase
{
    private const string SolutionFolderTypeGuid = "2150E333-8FDC-42A3-9474-1A3956D46DE8";
    private const string BuildingBlocksPrefix = "Bedrock.BuildingBlocks.";

    public override string Name => "IN018_CanonicalSolutionFolders";

    public override string Description =>
        "Projetos de bounded context devem estar nos solution folders canonicos do .sln (IN-018).";

    public override Severity DefaultSeverity => Severity.Error;

    public override string AdrPath => "docs/adrs/infrastructure/IN-018-solution-folders-canonicos-bounded-context.md";

    protected override Violation? AnalyzeType(TypeContext context) => null;

    public override IReadOnlyList<RuleAnalysisResult> Analyze(
        IReadOnlyDictionary<string, Compilation> compilations,
        string rootDir)
    {
        var results = new List<RuleAnalysisResult>();

        var slnPath = FindSolutionFile(rootDir);
        if (slnPath is null)
            return results;

        string slnContent;
        try
        {
            slnContent = File.ReadAllText(slnPath);
        }
        catch
        {
            return results;
        }

        var slnEntries = ParseSlnProjects(slnContent);
        var nestedMap = ParseNestedProjects(slnContent);

        foreach (var (projectName, _) in compilations)
        {
            if (!IsBoundedContextProject(projectName))
                continue;

            var layer = ClassifyLayer(projectName);
            var bcPrefix = ExtractBcPrefix(projectName, layer);

            // Find this project in the .sln
            var projectEntry = slnEntries
                .FirstOrDefault(e => string.Equals(e.Name, projectName, StringComparison.OrdinalIgnoreCase)
                    && !e.IsSolutionFolder);

            if (projectEntry is null)
                continue;

            // Apenas projetos em src/ — whitelist, nao blacklist
            if (!IsSourceProject(projectEntry.Path))
                continue;

            var typeResults = new List<TypeAnalysisResult>();

            // Walk up the NestedProjects chain to build folder ancestry
            var folderChain = BuildFolderChain(projectEntry.Guid, slnEntries, nestedMap);

            var violation = ValidateFolderPlacement(projectName, layer, bcPrefix, folderChain);

            if (violation is not null)
            {
                typeResults.Add(new TypeAnalysisResult
                {
                    TypeName = projectName,
                    TypeFullName = projectName,
                    File = projectEntry.Path,
                    Line = 1,
                    Status = TypeAnalysisStatus.Failed,
                    Violation = violation
                });
            }
            else
            {
                typeResults.Add(new TypeAnalysisResult
                {
                    TypeName = projectName,
                    TypeFullName = projectName,
                    File = projectEntry.Path,
                    Line = 1,
                    Status = TypeAnalysisStatus.Passed,
                    Violation = null
                });
            }

            results.Add(new RuleAnalysisResult
            {
                RuleCategory = Category,
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

    #region Validation

    private Violation? ValidateFolderPlacement(
        string projectName,
        BoundedContextLayer layer,
        string bcPrefix,
        List<SlnEntry> folderChain)
    {
        if (folderChain.Count == 0)
        {
            return CreateViolation(projectName,
                $"Projeto '{projectName}' nao esta em nenhum solution folder. " +
                $"Deveria estar no folder canonico para a camada {layer}.",
                $"Mover o projeto para o solution folder correto no .sln. " +
                $"Consulte a ADR IN-018.");
        }

        // folderChain is [immediate parent, grandparent, great-grandparent, ...]
        var parentFolder = folderChain[0];

        var (parentRegex, needsGrandparent) = GetExpectedFolderPattern(layer);

        if (!Regex.IsMatch(parentFolder.Name, parentRegex))
        {
            return CreateViolation(projectName,
                $"Projeto '{projectName}' ({layer}) esta no solution folder '{parentFolder.Name}', " +
                $"mas deveria estar em um folder que corresponda ao padrao '{parentRegex}'.",
                $"Mover o projeto para o solution folder correto (padrao: '{parentRegex}'). " +
                $"Consulte a ADR IN-018.");
        }

        if (needsGrandparent)
        {
            if (folderChain.Count < 2)
            {
                return CreateViolation(projectName,
                    $"Projeto '{projectName}' ({layer}) esta em '{parentFolder.Name}' " +
                    $"mas este folder nao esta aninhado em um folder Infra.",
                    $"O folder '{parentFolder.Name}' deve estar dentro de um folder " +
                    $"que corresponda ao padrao '^\\d+ - Infra$'. Consulte a ADR IN-018.");
            }

            var grandparentFolder = folderChain[1];
            if (!Regex.IsMatch(grandparentFolder.Name, @"^\d+ - Infra$"))
            {
                return CreateViolation(projectName,
                    $"Projeto '{projectName}' ({layer}) esta em '{parentFolder.Name}' " +
                    $"cujo pai e '{grandparentFolder.Name}', mas deveria ser um folder Infra " +
                    $"(padrao: '^\\d+ - Infra$').",
                    $"Aninhar '{parentFolder.Name}' dentro de um folder Infra. " +
                    $"Consulte a ADR IN-018.");
            }
        }

        // Validate BC folder is an ancestor
        var bcName = ExtractBcName(bcPrefix);
        var hasBcAncestor = folderChain.Any(f =>
            string.Equals(f.Name, bcName, StringComparison.OrdinalIgnoreCase));

        if (!hasBcAncestor)
        {
            return CreateViolation(projectName,
                $"Projeto '{projectName}' nao tem o folder do bounded context '{bcName}' " +
                $"como ancestral na cadeia de solution folders.",
                $"O projeto deve estar aninhado dentro do solution folder '{bcName}'. " +
                $"Consulte a ADR IN-018.");
        }

        return null;
    }

    private static (string Pattern, bool NeedsGrandparent) GetExpectedFolderPattern(BoundedContextLayer layer)
    {
        return layer switch
        {
            BoundedContextLayer.Api => (@"^\d+ - Api$", false),
            BoundedContextLayer.Application => (@"^\d+ - Application$", false),
            BoundedContextLayer.DomainEntities => (@"^\d+ - Domain$", false),
            BoundedContextLayer.Domain => (@"^\d+ - Domain$", false),
            BoundedContextLayer.InfraData => (@"^\d+\.\d+ - Data$", true),
            BoundedContextLayer.InfraDataTech => (@"^\d+\.\d+ - Data$", true),
            BoundedContextLayer.Configuration => (@"^\d+\.\d+ - CrossCutting$", true),
            BoundedContextLayer.Bootstrapper => (@"^\d+\.\d+ - CrossCutting$", true),
            _ => (".*", false),
        };
    }

    private static string ExtractBcName(string bcPrefix)
    {
        // bcPrefix is like "ShopDemo.Auth" → extract "Auth"
        var lastDot = bcPrefix.LastIndexOf('.');
        return lastDot >= 0 ? bcPrefix[(lastDot + 1)..] : bcPrefix;
    }

    private Violation CreateViolation(string projectName, string message, string llmHint)
    {
        return new Violation
        {
            Rule = Name,
            Severity = DefaultSeverity,
            Adr = AdrPath,
            Project = projectName,
            File = "",
            Line = 1,
            Message = message,
            LlmHint = llmHint
        };
    }

    #endregion

    #region .sln Parsing

    private static string? FindSolutionFile(string rootDir)
    {
        try
        {
            var files = Directory.GetFiles(rootDir, "*.sln", SearchOption.TopDirectoryOnly);
            return files.Length > 0 ? files[0] : null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Determina se um projeto pertence a um bounded context.
    /// Criterios:
    /// 1. Camada reconhecida (ClassifyLayer != Unknown)
    /// 2. Nao e BuildingBlock (prefixo "Bedrock.BuildingBlocks.")
    /// 3. BC prefix e composto (tem pelo menos um dot, ex: "ShopDemo.Auth")
    ///    — um prefixo simples como "SomeLib" nao e BC.
    /// </summary>
    private static bool IsBoundedContextProject(string projectName)
    {
        if (projectName.StartsWith(BuildingBlocksPrefix, StringComparison.Ordinal))
            return false;

        var layer = ClassifyLayer(projectName);
        if (layer == BoundedContextLayer.Unknown)
            return false;

        var bcPrefix = ExtractBcPrefix(projectName, layer);
        if (string.IsNullOrEmpty(bcPrefix))
            return false;

        // BC prefix deve ser composto (Company.BcName): conter pelo menos um dot
        if (!bcPrefix.Contains('.'))
            return false;

        return true;
    }

    /// <summary>
    /// Verifica se o path do projeto no .sln comeca com "src\" — whitelist.
    /// Qualquer projeto fora de src/ (tests, tools, playground, benchmarks, etc.)
    /// nao e validado por esta regra.
    /// </summary>
    private static bool IsSourceProject(string projectPath)
    {
        return projectPath.StartsWith("src\\", StringComparison.OrdinalIgnoreCase)
            || projectPath.StartsWith("src/", StringComparison.OrdinalIgnoreCase);
    }

    internal static List<SlnEntry> ParseSlnProjects(string slnContent)
    {
        var entries = new List<SlnEntry>();
        // Pattern: Project("{TYPE-GUID}") = "Name", "Path", "{PROJ-GUID}"
        var regex = new Regex(
            @"Project\(""\{(?<type>[^}]+)\}""\)\s*=\s*""(?<name>[^""]*)""\s*,\s*""(?<path>[^""]*)""\s*,\s*""\{(?<guid>[^}]+)\}""",
            RegexOptions.Compiled);

        foreach (Match match in regex.Matches(slnContent))
        {
            entries.Add(new SlnEntry
            {
                TypeGuid = match.Groups["type"].Value,
                Name = match.Groups["name"].Value,
                Path = match.Groups["path"].Value,
                Guid = match.Groups["guid"].Value,
                IsSolutionFolder = string.Equals(
                    match.Groups["type"].Value,
                    SolutionFolderTypeGuid,
                    StringComparison.OrdinalIgnoreCase)
            });
        }

        return entries;
    }

    internal static Dictionary<string, string> ParseNestedProjects(string slnContent)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Pattern: {CHILD-GUID} = {PARENT-GUID} inside GlobalSection(NestedProjects)
        var sectionRegex = new Regex(
            @"GlobalSection\(NestedProjects\)\s*=\s*preSolution(.*?)EndGlobalSection",
            RegexOptions.Compiled | RegexOptions.Singleline);

        var sectionMatch = sectionRegex.Match(slnContent);
        if (!sectionMatch.Success)
            return map;

        var entryRegex = new Regex(
            @"\{(?<child>[^}]+)\}\s*=\s*\{(?<parent>[^}]+)\}",
            RegexOptions.Compiled);

        foreach (Match match in entryRegex.Matches(sectionMatch.Groups[1].Value))
        {
            map[match.Groups["child"].Value] = match.Groups["parent"].Value;
        }

        return map;
    }

    private static List<SlnEntry> BuildFolderChain(
        string projectGuid,
        List<SlnEntry> entries,
        Dictionary<string, string> nestedMap)
    {
        var chain = new List<SlnEntry>();
        var guidToEntry = new Dictionary<string, SlnEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var entry in entries)
            guidToEntry[entry.Guid] = entry;

        var currentGuid = projectGuid;
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (nestedMap.TryGetValue(currentGuid, out var parentGuid))
        {
            if (!visited.Add(parentGuid))
                break; // cycle protection

            if (guidToEntry.TryGetValue(parentGuid, out var parentEntry) && parentEntry.IsSolutionFolder)
            {
                chain.Add(parentEntry);
            }

            currentGuid = parentGuid;
        }

        return chain;
    }

    internal sealed class SlnEntry
    {
        public required string TypeGuid { get; init; }
        public required string Name { get; init; }
        public required string Path { get; init; }
        public required string Guid { get; init; }
        public required bool IsSolutionFolder { get; init; }
    }

    #endregion
}
