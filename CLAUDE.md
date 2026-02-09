# Instruções do Projeto Bedrock

## Prompts Reutilizáveis

Prompts padronizados em `.claude/prompts/`:

| Prompt | Descrição |
|--------|-----------|
| [review-zero-allocation.md](.claude/prompts/review-zero-allocation.md) | Revisão de performance focada em eliminar alocações |

**Uso:** Substituir variáveis `{{variavel}}` pelos valores reais e colar no chat.

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

## Configuração do Ambiente Local

### Arquivo .env

O projeto utiliza um arquivo `.env` na raiz para configurações sensíveis. Este arquivo **não é versionado** (está no `.gitignore`).

**Criar o arquivo `.env`:**

```bash
# Na raiz do projeto
touch .env
```

**Conteúdo do arquivo:**

```env
SONAR_TOKEN=<seu-token-do-sonarcloud>
```

**Onde obter o SONAR_TOKEN:**

1. Acesse [SonarCloud](https://sonarcloud.io)
2. Faça login com sua conta GitHub
3. Vá em **My Account** → **Security**
4. Gere um novo token em **Generate Tokens**
5. Copie o token e adicione ao `.env`

> **Nota**: Sem o `SONAR_TOKEN`, a pipeline local funcionará normalmente, mas a etapa de busca de issues do SonarCloud será ignorada (bypass). Isso permite que qualquer pessoa rode a pipeline local sem precisar de acesso ao SonarCloud.

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

#### Exclusões de Código Não-Testável

Para código **genuinamente impossível de testar** (ex: spin-wait, overflow que requer milhões de iterações), usar:

1. **Stryker comments** - para ignorar mutações
2. **`[ExcludeFromCodeCoverage]`** - para ignorar cobertura (Coverlet e SonarCloud)

**Exemplo completo:**

```csharp
/// <summary>
/// Método impossível de testar em tempo razoável.
/// </summary>
// Stryker disable all : Counter overflow requires 67M+ IDs to test - impractical
[ExcludeFromCodeCoverage(Justification = "Counter overflow requer 67M+ IDs para testar - impraticavel")]
private static void HandleCounterOverflowIfNeeded(ref long timestamp)
{
    if (_counter > 0x3FFFFFF)
    {
        SpinWaitForNextMillisecond(ref timestamp, ref _lastTimestamp);
        _counter = 0;
    }
}
// Stryker restore all
```

**Regras para exclusão:**
- Usar **apenas** quando for genuinamente impossível testar
- **Sempre** incluir justificativa em pt-BR
- Preferir extrair para método separado com `[ExcludeFromCodeCoverage]`
- Preferir exclusões granulares (`disable once`) quando possível
- Documentar no PR o motivo da exclusão

**Stryker comments disponíveis:**
| Comentário | Uso |
|------------|-----|
| `// Stryker disable all : reason` | Desabilita todas as mutações até `restore` |
| `// Stryker restore all` | Restaura mutações |
| `// Stryker disable once all : reason` | Desabilita apenas na próxima linha |
| `// Stryker disable once Statement : reason` | Desabilita remoção de statement |
| `// Stryker disable Equality,Arithmetic : reason` | Desabilita mutadores específicos |

**Requer `using System.Diagnostics.CodeAnalysis;`** para usar o atributo.

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
├── pending/          # Pendências extraídas para análise
│   ├── SUMMARY.txt           # Resumo consolidado das pendências
│   ├── mutant_*.txt          # Mutantes sobreviventes (um arquivo por mutante)
│   ├── coverage_*.txt        # Arquivos com cobertura insuficiente
│   └── sonar_*.txt           # Issues do SonarCloud (um arquivo por issue)
└── summary.json      # Resumo consolidado da pipeline
```

#### Formato dos Arquivos de Pendências

**Mutantes (`mutant_<projeto>_<numero>.txt`):**
```
PROJECT: <nome-do-projeto>
FILE: <caminho-do-arquivo>
LINE: <linha>
STATUS: Survived|NoCoverage
MUTATOR: <tipo-de-mutador>
DESCRIPTION: <descrição-da-mutação>
```

**Cobertura (`coverage_<projeto>_<numero>.txt`):**
```
PROJECT: <nome-do-projeto>
FILE: <caminho-do-arquivo>
UNCOVERED_LINES: <linhas-separadas-por-virgula>
COUNT: <quantidade-de-linhas>
```

**SonarCloud (`sonar_<tipo>_<numero>.txt`):**
```
TYPE: BUG|CODE_SMELL|VULNERABILITY|SECURITY_HOTSPOT
SEVERITY: BLOCKER|CRITICAL|MAJOR|MINOR|INFO
FILE: <caminho-do-arquivo>
LINE: <linha>
RULE: <regra-violada>
EFFORT: <esforço-estimado>
MESSAGE: <descrição-do-problema>
```

> **Tipos de arquivo SonarCloud**: `sonar_bug_*.txt`, `sonar_smell_*.txt`, `sonar_vuln_*.txt`, `sonar_hotspot_*.txt`

#### Instruções para Code Agent

**IMPORTANTE**: Antes de commitar qualquer código, o code agent DEVE:

1. Executar `./scripts/pipeline.sh`
2. Verificar `artifacts/pending/SUMMARY.txt`
3. Se houver pendências:
   - **Mutantes**: Ler `artifacts/pending/mutant_*.txt` e corrigir testes
   - **Cobertura**: Ler `artifacts/pending/coverage_*.txt` e adicionar testes
   - **SonarCloud**: Ler `artifacts/pending/sonar_*.txt` e corrigir issues
   - Repetir até atingir **100%** de cobertura
   - Repetir até atingir **100%** de mutação
   - Repetir até não ter mais issue do SonarCloud para resolver
4. Só commitar quando a pipeline passar completamente

### Limite de Retentativas

Para evitar loops infinitos, o code agent DEVE respeitar os seguintes limites:

| Etapa | Máximo de Tentativas |
|-------|---------------------|
| Pipeline local (passo 3-4) | 5 tentativas |
| Pipeline GitHub Actions (passo 7-9) | 5 tentativas |

**Ao atingir o limite**:
1. **Parar imediatamente** a execução
2. **Informar o usuário** com um resumo claro:
   - Quantas tentativas foram feitas
   - Quais pendências ainda existem
   - Últimos erros encontrados
3. **Solicitar intervenção humana** para decidir próximos passos

### Após Pipeline Local Passar

Quando a pipeline local passar com sucesso, o code agent DEVE:

1. **Criar a PR** usando `gh pr create`
   - Título descritivo seguindo o padrão do projeto
   - Body com `Closes #<issue>` para auto-close
2. **Aguardar a pipeline do GitHub Actions**
   - Verificar status com `gh pr checks <number>`
3. **Se a pipeline passar**:
   - Fazer merge com `gh pr merge <number> --squash --delete-branch`
   - Atualizar branch local: `git checkout main && git pull`
   - Limpar branches locais obsoletas: `git branch -d <branch-name>`
4. **Se a pipeline falhar** (máximo 5 tentativas):
   - Analisar os logs com `gh run view <run-id> --log-failed`
   - Corrigir os problemas localmente
   - Commitar e push (a PR será atualizada automaticamente)
   - Repetir até passar ou atingir o limite de tentativas

**Issues do SonarCloud que NÃO fazem sentido:**

Algumas issues do SonarCloud podem não fazer sentido no contexto do projeto. Nestes casos, o code agent DEVE:

1. **Avaliar criticamente** se a issue realmente se aplica ao contexto
2. **Não resolver** issues que:
   - Contradizem decisões arquiteturais documentadas em ADRs
   - São falsos positivos (ex: código gerado, testes, mocks)
   - Não se aplicam ao contexto específico do projeto
3. **Informar o usuário** sobre a decisão de não resolver, explicando:
   - Qual issue foi ignorada
   - Motivo da decisão (ex: "contradiz ADR-XXX", "falso positivo", etc.)
4. **Marcar como "Won't Fix"** no SonarCloud se tiver acesso, ou documentar no PR

```
┌──────────────────────────────────────────────────────────┐
│  FLUXO OBRIGATÓRIO DO CODE AGENT                         │
├──────────────────────────────────────────────────────────┤
│  1. Implementar código                                   │
│  2. Implementar testes                                   │
│  3. Executar: ./scripts/pipeline.sh                      │
│  4. Se FAILED ou com pendências (max 5 tentativas):      │
│     - Ler artifacts/pending/SUMMARY.txt                  │
│     - Ler artifacts/pending/mutant_*.txt                 │
│     - Ler artifacts/pending/coverage_*.txt               │
│     - Ler artifacts/pending/sonar_*.txt                  │
│     - Corrigir testes para matar mutantes                │
│     - Adicionar testes para cobertura                    │
│     - Resolver issues do SonarCloud (ou justificar)      │
│     - Voltar ao passo 3                                  │
│  5. Se SUCCESS: commitar e push                          │
│  6. Criar PR: gh pr create                               │
│  7. Verificar pipeline: gh pr checks <number>            │
│  8. Se pipeline PASSOU:                                  │
│     - gh pr merge <number> --squash --delete-branch      │
│     - git checkout main && git pull                      │
│     - git branch -d <branch-local>                       │
│  9. Se pipeline FALHOU (max 5 tentativas):               │
│     - Analisar: gh run view <run-id> --log-failed        │
│     - Corrigir e voltar ao passo 3                       │
│ 10. Se atingiu limite: PARAR e informar o usuário        │
└──────────────────────────────────────────────────────────┘
```

> **Nota**: Os artefatos são gerados em formato texto para consumo programático. Relatórios HTML são gerados apenas no GitHub Actions.

## Active Technologies
- C# / .NET 10.0 + Bedrock BuildingBlocks (Core, Domain.Entities, Data, Persistence.PostgreSql, Observability, Testing) (137-auth-scaffolding)
- N/A (scaffolding apenas — sem entidades nem persistência nesta issue) (137-auth-scaffolding)
- C# / .NET 10.0 + Bedrock.BuildingBlocks.Core, Bedrock.BuildingBlocks.Domain, Bedrock.BuildingBlocks.Domain.Entities, Bedrock.BuildingBlocks.Testing, Konscious.Security.Cryptography (Argon2id) (001-auth-domain-model)
- N/A (domain model apenas — persistência é escopo de outra issue) (001-auth-domain-model)

## Recent Changes
- 137-auth-scaffolding: Added C# / .NET 10.0 + Bedrock BuildingBlocks (Core, Domain.Entities, Data, Persistence.PostgreSql, Observability, Testing)
