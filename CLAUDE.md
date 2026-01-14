# Instruções do Projeto Bedrock

## Gestão de Tarefas

- Todas as tarefas são gerenciadas por **issues no GitHub**
- Usar a CLI `gh` para todas as operações (já instalada)
- Comandos úteis:
  - `gh issue list` — listar issues
  - `gh issue view <number>` — ver detalhes de uma issue
  - `gh issue create` — criar nova issue
  - `gh issue close <number>` — fechar issue
  - `gh label list` — listar labels

### Campo Type (Obrigatório)

Toda issue deve ter o campo **Type** definido. Os tipos disponíveis são:

| Type | Descrição |
|------|-----------|
| `Bug` | Problema inesperado ou comportamento incorreto |
| `Feature` | Nova funcionalidade ou melhoria |
| `Task` | Tarefa específica de trabalho |

**Como definir o Type via GraphQL:**

```bash
# 1. Obter o ID da issue e os tipos disponíveis
gh api graphql -f query='
{
  repository(owner: "CasteloBrancoLab", name: "Bedrock") {
    issue(number: <NUMERO>) {
      id
    }
    issueTypes(first: 10) {
      nodes { id name }
    }
  }
}'

# 2. Atualizar o tipo da issue
gh api graphql -f query='
mutation {
  updateIssue(input: {
    id: "<ISSUE_ID>"
    issueTypeId: "<TYPE_ID>"
  }) {
    issue { issueType { name } }
  }
}'
```

> **Nota**: O campo Type não pode ser definido via `gh issue create` ou `gh issue edit`. É necessário usar a API GraphQL.

## Fluxo de Trabalho

### Branches
- Criar uma branch por issue: `<type>/<issue-number>-<descricao>`
- Exemplos: `feature/42-add-value-objects`, `migration/15-core-execution-context`

### Pipeline
```
Upstream          Middlestream              Downstream
─────────────────────────────────────────────────────────
backlog → ready → in-progress → review → approved → merged
```

### Ciclo de Vida da Issue
1. **Issue criada** → `status:backlog`
2. **Refinada/priorizada** → `status:ready`
3. **Branch criada, desenvolvimento iniciado** → `status:in-progress`
4. **PR aberto** → `status:review` (PR deve conter `Closes #<issue>`)
5. **PR aprovado** → `status:approved`
6. **Merge** → Issue fechada automaticamente

## Labels

### Tipo (`type:`)
| Label | Descrição |
|-------|-----------|
| `type:feature` | Nova funcionalidade |
| `type:refactor` | Refatoração de código |
| `type:migration` | Migração de código legado |
| `type:test` | Testes |
| `type:chore` | Manutenção e tarefas auxiliares |

### Componente (`component:`)
| Label | Descrição |
|-------|-----------|
| `component:core` | BuildingBlocks.Core |
| `component:domain` | BuildingBlocks.Domain |
| `component:data` | BuildingBlocks.Data |
| `component:persistence` | BuildingBlocks.Persistence |
| `component:serialization` | BuildingBlocks.Serialization |
| `component:observability` | BuildingBlocks.Observability |

### Prioridade (`priority:`)
| Label | Descrição |
|-------|-----------|
| `priority:high` | Alta prioridade |
| `priority:medium` | Média prioridade |
| `priority:low` | Baixa prioridade |

### Status (`status:`)
| Label | Estágio | Descrição |
|-------|---------|-----------|
| `status:backlog` | Upstream | Aguardando priorização |
| `status:ready` | Upstream | Pronto para iniciar (refinado) |
| `status:in-progress` | Middlestream | Em desenvolvimento |
| `status:review` | Middlestream | PR aberto, aguardando review |
| `status:approved` | Downstream | Aprovado, pronto para merge |
| `status:blocked` | — | Bloqueado por dependência |

### Padrão GitHub
Labels padrão do GitHub também disponíveis: `bug`, `documentation`, `enhancement`, `question`, etc.

## Convenções

- Nome do framework: **Bedrock**
- Namespace base: `Bedrock`
- Target Framework: .NET 10.0
- Diagramas nas issues devem ser feitos em **Mermaid**

## Testes

### Estrutura

```
src/BuildingBlocks/Testing/           # BuildingBlock base para testes
tests/UnitTests/BuildingBlocks/       # Testes unitários por componente
tests/MutationTests/BuildingBlocks/   # Testes de mutação por componente
```

### Convenções

- Relação **1:1** entre projeto `src` e projeto `tests`
- Nomenclatura UnitTests: `Bedrock.UnitTests.<namespace-do-src>`
- Nomenclatura MutationTests: `Bedrock.MutationTests.<namespace-do-src>`
- Padrão obrigatório: **AAA (Arrange, Act, Assert)**
- Motivo da relação 1:1: Compatibilidade com **Stryker.NET** (mutation testing)

### Bibliotecas Obrigatórias

| Biblioteca | Propósito |
|------------|-----------|
| xUnit | Framework de testes |
| Shouldly | Assertions fluentes |
| Moq | Mocking |
| Bogus | Geração de dados fake |
| Humanizer | Formatação humanizada de logs |
| Coverlet | Cobertura de código |
| Stryker.NET | Testes de mutação |

### Classes Base

#### TestBase
Classe base para todos os testes. Herdar para ter acesso a logging padronizado.

```csharp
public class MyTests : TestBase
{
    public MyTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void MyTest()
    {
        // Arrange
        LogArrange("Preparando dados");

        // Act
        LogAct("Executando ação");

        // Assert
        LogAssert("Verificando resultado");
    }
}
```

#### ServiceCollectionFixture
Fixture para testes que precisam de IoC. Herdar e implementar `ConfigureServices`.

```csharp
public class MyFixture : ServiceCollectionFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IMyService, MyService>();
    }
}

[CollectionDefinition("MyServices")]
public class MyCollection : ICollectionFixture<MyFixture> { }

[Collection("MyServices")]
public class MyTests : TestBase
{
    private readonly MyFixture _fixture;

    public MyTests(MyFixture fixture, ITestOutputHelper output) : base(output)
    {
        _fixture = fixture;
    }
}
```

### Testes de Mutação

Testes de mutação validam a qualidade dos testes unitários através do **Stryker.NET**.

#### Estrutura

Cada projeto de UnitTests tem um correspondente em MutationTests:

```
tests/MutationTests/BuildingBlocks/Core/
└── stryker-config.json
```

#### Configuração (stryker-config.json)

```json
{
  "$schema": "https://raw.githubusercontent.com/stryker-mutator/stryker-net/master/src/Stryker.Core/Stryker.Core/stryker-config.schema.json",
  "stryker-config": {
    "project": "Bedrock.BuildingBlocks.Core.csproj",
    "test-projects": [
      "../../../UnitTests/BuildingBlocks/Core/Bedrock.UnitTests.BuildingBlocks.Core.csproj"
    ],
    "reporters": ["html", "progress"],
    "thresholds": {
      "high": 100,
      "low": 100,
      "break": 100
    }
  }
}
```

#### Regras

- Threshold mínimo: **100%** (código desenvolvido com IA não aceita mutantes sobreviventes)
- Cada projeto de mutação referencia **apenas** seu projeto de UnitTests correspondente
- Relatórios HTML são armazenados como artifacts na pipeline (retenção: 3 dias)

#### Exclusões com Stryker Comments

Para código **impossível de testar** (ex: spin-wait que requer milhões de iterações), usar comentários do Stryker:

```csharp
// Stryker disable all : Reason for exclusion
if (_counter > 0x3FFFFFF)
{
    SpinWaitForNextMillisecond(ref timestamp, ref _lastTimestamp);
    _counter = 0;
}
// Stryker restore all
```

**Regras para exclusão:**
- Usar **apenas** quando for genuinamente impossível testar
- **Sempre** incluir justificativa após o `:`
- Preferir exclusões granulares (`disable once`) quando possível
- Documentar no PR o motivo da exclusão

**Comentários disponíveis:**
| Comentário | Uso |
|------------|-----|
| `// Stryker disable all : reason` | Desabilita todas as mutações até `restore` |
| `// Stryker restore all` | Restaura mutações |
| `// Stryker disable once all : reason` | Desabilita apenas na próxima linha |
| `// Stryker disable Equality,Arithmetic : reason` | Desabilita mutadores específicos |

### Pipeline Local

Scripts bash para execução local da pipeline, otimizados para uso pelo **code agent**.

#### Estrutura

```
scripts/
├── clean.sh          # Limpa bin/ e obj/ recursivamente
├── clean-artifacts.sh # Limpa artefatos gerados
├── build.sh          # Compila a solução
├── test.sh           # Executa testes com cobertura
├── mutate.sh         # Executa testes de mutação
└── pipeline.sh       # Executa pipeline completa
```

#### Comandos

```bash
# Pipeline completa (recomendado)
./scripts/pipeline.sh

# Comandos individuais
./scripts/clean.sh           # Limpar bin/obj
./scripts/clean-artifacts.sh # Limpar artefatos
./scripts/build.sh           # Compilar
./scripts/test.sh            # Testar com cobertura
./scripts/mutate.sh          # Testes de mutação
```

#### Artefatos Gerados

```
artifacts/
├── coverage/         # Relatórios de cobertura (Cobertura XML)
├── mutation/         # Relatórios de mutação (JSON)
└── summary.json      # Resumo consolidado da pipeline
```

#### Instruções para Code Agent

**IMPORTANTE**: Antes de commitar qualquer código, o code agent DEVE:

1. Executar `./scripts/pipeline.sh`
2. Verificar `artifacts/summary.json`
3. Se coverage ou mutation falhar:
   - Analisar os relatórios em `artifacts/`
   - Corrigir os testes
   - Repetir até atingir **100%**
4. Só commitar quando a pipeline passar completamente

```
┌─────────────────────────────────────────────────────┐
│  FLUXO OBRIGATÓRIO DO CODE AGENT                    │
├─────────────────────────────────────────────────────┤
│  1. Implementar código                              │
│  2. Implementar testes                              │
│  3. Executar: ./scripts/pipeline.sh                 │
│  4. Se FAILED:                                      │
│     - Ler artifacts/summary.json                    │
│     - Ler artifacts/mutation/*.json                 │
│     - Corrigir testes para matar mutantes           │
│     - Voltar ao passo 3                             │
│  5. Se SUCCESS: commitar                            │
└─────────────────────────────────────────────────────┘
```

> **Nota**: Os artefatos são gerados em formato JSON para consumo programático. Relatórios HTML são gerados apenas no GitHub Actions.
