using Shouldly;
using Xunit.Abstractions;

namespace Bedrock.BuildingBlocks.Testing.Architecture;

/// <summary>
/// Classe base para testes de arquitetura.
/// Fornece acesso à fixture com compilações Roslyn e ao gerenciador de violações.
/// </summary>
/// <typeparam name="TFixture">Tipo da fixture concreta que herda de <see cref="RuleFixture"/>.</typeparam>
public abstract class RuleTestBase<TFixture> : TestBase
    where TFixture : RuleFixture
{
    private readonly TFixture _fixture;

    protected RuleTestBase(TFixture fixture, ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// Fixture com compilações e manager.
    /// </summary>
    protected TFixture Fixture => _fixture;

    /// <summary>
    /// Diretório raiz do repositório.
    /// </summary>
    protected string RootDir => _fixture.RootDir;

    /// <summary>
    /// Gerenciador de violações.
    /// </summary>
    public ViolationManager Manager => _fixture.Manager;

    /// <summary>
    /// Executa uma regra contra todos os projetos compilados e valida que não há violações.
    /// </summary>
    public void AssertNoViolations(Rule rule)
    {
        // Arrange
        LogArrange($"Preparando regra: {rule.Name}");

        // Act
        LogAct($"Analisando {_fixture.Compilations.Count} projetos");
        var ruleResults = rule.Analyze(_fixture.Compilations, RootDir);

        // Registrar resultados no manager (extrai violations automaticamente)
        Manager.AddRuleResults(ruleResults);

        // Sempre gerar relatório JSON (mesmo sem violações, para o HTML report)
        var jsonPath = Path.Combine(RootDir, "artifacts", "architecture", "architecture-report.json");
        Manager.WriteJsonReport(jsonPath);

        // Extrair violações dos resultados
        var allViolations = ruleResults
            .SelectMany(r => r.TypeResults)
            .Where(t => t.Violation is not null)
            .Select(t => t.Violation!)
            .ToList();

        // Gerar artefatos de pending se houver violações
        if (allViolations.Count > 0)
        {
            var pendingDir = Path.Combine(RootDir, "artifacts", "pending");
            Manager.WritePendingFiles(pendingDir);
        }

        // Assert
        LogAssert($"Verificando violações da regra {rule.Name}");

        var totalTypes = ruleResults.Sum(r => r.TypeResults.Count);
        var passedTypes = ruleResults.Sum(r => r.PassedCount);
        LogInfo($"Tipos analisados: {totalTypes} | Passou: {passedTypes} | Falhou: {allViolations.Count}");

        if (allViolations.Count > 0)
        {
            var errorViolations = allViolations.Where(v => v.Severity == Severity.Error).ToList();
            var warningViolations = allViolations.Where(v => v.Severity == Severity.Warning).ToList();

            foreach (var v in allViolations)
            {
                LogInfo($"[{v.Severity}] {v.File}:{v.Line} - {v.Message}");
            }

            // Só falha se houver erros (warnings são apenas reportados)
            errorViolations.Count.ShouldBe(
                0,
                $"Regra '{rule.Name}' encontrou {errorViolations.Count} erro(s) e {warningViolations.Count} aviso(s).\n" +
                string.Join("\n", errorViolations.Select(v => $"  - {v.File}:{v.Line} → {v.Message}")));
        }
    }
}
