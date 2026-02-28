# Instrucoes do Projeto Bedrock

## Convencoes

- Nome do framework: **Bedrock**
- Namespace base: `Bedrock`
- Target Framework: .NET 10.0
- Diagramas em **Mermaid**

## Gestao de Tarefas

- Issues no GitHub via CLI `gh`
- Toda issue deve ter o campo **Type** (Bug, Feature, Task) via GraphQL:

```bash
# Obter IDs
gh api graphql -f query='{ repository(owner: "CasteloBrancoLab", name: "Bedrock") { issue(number: <N>) { id } issueTypes(first: 10) { nodes { id name } } } }'

# Definir Type
gh api graphql -f query='mutation { updateIssue(input: { id: "<ISSUE_ID>" issueTypeId: "<TYPE_ID>" }) { issue { issueType { name } } } }'
```

## Fluxo de Trabalho

### Branches

`<type>/<issue-number>-<descricao>` (ex: `feature/42-add-value-objects`)

### Ciclo de Vida

```
Issue criada → status:backlog → status:ready → status:in-progress → status:review → merged
```

PR deve conter `Closes #<issue>` para auto-close.

## Fases de Desenvolvimento (Skills)

O desenvolvimento segue **8 fases sequenciais**, cada uma com sua skill:

```
/plan → /code → /arch → /test → /mutate → /integration → /pipeline → /pr
```

| Fase | Skill | Script | O que faz |
|------|-------|--------|-----------|
| 0. Planejamento | `/plan` | `gh` CLI | Analisar issue, plano de implementacao, branch |
| 1. Implementacao | `/code` | `build-check.sh` | Codigo-fonte + build |
| 2. Arquitetura | `/arch` | `arch-check.sh` | Validacao de regras arquiteturais |
| 3. Testes | `/test` | `test-check.sh` | Testes unitarios + cobertura |
| 4. Mutacao | `/mutate` | `mutate-check.sh` | Testes de mutacao (100%) |
| 5. Integracao | `/integration` | `integration-check.sh` | Testes de integracao (Docker) |
| 6. Pipeline | `/pipeline` | `pipeline-check.sh` | Validacao final completa |
| 7. Pull Request | `/pr` | `gh` CLI | Criar PR, acompanhar CI, merge |

### Principio: Scripts fazem o trabalho pesado

- Cada script gera artefatos em `artifacts/pending/`
- Claude le **apenas** `artifacts/pending/SUMMARY.txt` e os pending files
- **NAO** interprete output bruto de dotnet/stryker — leia somente os pending files
- Maximo **5 iteracoes** por fase; se atingir, pare e informe o usuario

### Desenvolvimento rapido (sem skills)

Para iteracoes intermediarias sem skills:

| Fase | Comando |
|------|---------|
| Build rapido | `dotnet build` |
| Testes focados | `dotnet test <projeto>` |
| Pipeline completa | `./scripts/pipeline.sh` |

## Prompts Reutilizaveis

Prompts padronizados em `.claude/prompts/`:

| Prompt | Descricao |
|--------|-----------|
| [review-zero-allocation.md](.claude/prompts/review-zero-allocation.md) | Revisao de performance focada em eliminar alocacoes |
| [implement-domain-entity-rule.md](.claude/prompts/implement-domain-entity-rule.md) | Implementar regra de arquitetura para Domain Entities |

**Uso:** Substituir variaveis `{{variavel}}` pelos valores reais e colar no chat.

## Regras Contextuais (.claude/rules/)

Regras carregadas automaticamente quando Claude toca arquivos nos paths especificados:

| Regra | Paths | Conteudo |
|-------|-------|----------|
| `testing.md` | `tests/UnitTests/**` | AAA, TestBase, libs, nomenclatura |
| `mutation.md` | `tests/MutationTests/**` | Stryker, threshold 100%, exclusoes |
| `integration.md` | `tests/IntegrationTests/**` | Docker, Testcontainers, WSL2 |

## Artefatos Gerados

```
artifacts/
├── pending/                  # Pendencias para o code agent
│   ├── SUMMARY.txt           # Resumo consolidado
│   ├── build_errors.txt      # Erros de build
│   ├── architecture_*.txt    # Violacoes de arquitetura
│   ├── test_*.txt            # Testes falhando
│   ├── mutant_*.txt          # Mutantes sobreviventes
│   ├── integration_*.txt     # Testes de integracao falhando
│   ├── coverage_*.txt        # Cobertura incompleta
│   └── sonar_*.txt           # Issues do SonarCloud
├── coverage/                 # Relatorios Cobertura XML
├── mutation/                 # Relatorios Stryker JSON
└── summary.json              # Resumo da pipeline
```

## Notas

- Cobertura e delegada ao SonarCloud (local Coverlet tem falsos positivos)
- SonarCloud analisa apenas `main` (plano Community); `sonar-check.sh` e pontual
- ADRs em `docs/adrs/` definem decisoes arquiteturais — respeite-os
