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
```

### Convenções

- Relação **1:1** entre projeto `src` e projeto `tests`
- Nomenclatura: `Bedrock.UnitTests.<namespace-do-src>`
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
