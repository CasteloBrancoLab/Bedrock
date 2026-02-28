---
name: plan
description: "Fase 0: Analisar issue e gerar plano de implementacao antes de /code."
argument-hint: "[issue-number ou descricao]"
disable-model-invocation: true
allowed-tools: Read, Glob, Grep, Bash(gh *), Bash(git *), Task
---

# /plan - Fase 0: Planejamento

Voce esta na fase PLAN. Analise a issue e gere um plano de implementacao.

## Argumentos

$ARGUMENTS

Aceita:
- Numero de issue: `/plan 42` → le a issue existente
- Descricao: `/plan Adicionar cache de tokens` → cria nova issue

## Delegacao de Modelo

Use a Task tool com `model: "haiku"` para tarefas mecanicas:
- Buscar arquivos e padroes no codebase (Explore agent)
- Ler ADRs relevantes em `docs/adrs/`
- Listar estrutura de projetos afetados

Opus (voce) foca em: **analisar escopo e gerar o plano**.

## Fluxo

### 1. Obter ou Criar a Issue

**Se argumento e numero:**
```bash
gh issue view <number> --json title,body,labels,assignees,state
```

**Se argumento e descricao:**
- Determinar o tipo (Feature, Bug, Task) pelo contexto
- Criar a issue:
```bash
gh issue create --title "<titulo>" --body "<descricao>" --label "<tipo>"
```
- Definir o campo Type via GraphQL (conforme CLAUDE.md)

### 2. Analisar o Codebase

Delegar a haiku (Explore agent) para:
- Identificar **arquivos/projetos afetados** pela mudanca
- Encontrar **padroes existentes** similares ao que sera implementado
- Listar **ADRs relevantes** em `docs/adrs/`
- Verificar **dependencias** entre projetos

### 3. Gerar o Plano de Implementacao

Produzir um plano estruturado com:

```markdown
## Plano de Implementacao - Issue #<N>

### Contexto
<Resumo da issue e motivacao>

### Arquivos a Criar
| Arquivo | Projeto | Descricao |
|---------|---------|-----------|

### Arquivos a Modificar
| Arquivo | Projeto | Mudanca |
|---------|---------|---------|

### Testes Necessarios
| Projeto de Teste | Arquivos | Cenarios |
|------------------|----------|----------|

### ADRs Aplicaveis
- ADR-XXX: <titulo> — <como se aplica>

### Fases de Execucao
1. `/code` — <o que implementar>
2. `/arch` — <regras a validar>
3. `/test` — <testes a escrever>
4. `/mutate` — <projetos a mutar>

### Riscos e Consideracoes
- <riscos identificados>
```

### 4. Criar Branch

Seguir a convencao do CLAUDE.md:
```bash
git checkout -b <type>/<issue-number>-<descricao>
```

Tipos: `feature/`, `fix/`, `refactor/`, `test/`, `docs/`

### 5. Atualizar Status da Issue

```bash
gh issue edit <number> --add-label "status:in-progress"
```

### 6. Apresentar o Plano

Exibir o plano ao usuario e aguardar confirmacao antes de sugerir `/code`.

## Regras

- NAO implemente codigo nesta fase — apenas planeje
- NAO crie testes — apenas liste o que sera necessario
- Respeite ADRs em `docs/adrs/` e convencoes do projeto
- O plano deve ser **conciso** e **acionavel** (sem texto generico)
- Se a issue for ambigua, pergunte ao usuario antes de gerar o plano
- Maximo **3 iteracoes** de refinamento do plano

## Saida

Ao final, informar:
- Numero da issue (criada ou existente)
- Branch criada
- Resumo do plano
- Proximo passo: `/code <issue-number>`
