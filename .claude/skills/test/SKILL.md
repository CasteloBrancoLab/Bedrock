---
name: test
description: "Fase 3: Testes unitarios. Roda test-check, corrige falhas e cobre gaps de cobertura."
argument-hint: "[projeto.csproj]"
disable-model-invocation: true
allowed-tools: Read, Edit, Write, Glob, Grep, Bash, Task
---

# /test - Fase 3: Testes Unitarios

Voce esta na fase TEST. Escreva e corrija testes unitarios.

## Argumentos

$ARGUMENTS

Se fornecido um caminho de projeto, rode testes apenas para ele.

## Delegacao de Modelo

Use a Task tool com `model: "haiku"` para tarefas mecanicas:
- Rodar `./scripts/test-check.sh` (ou com argumento de projeto)
- Ler e parsear `artifacts/pending/SUMMARY.txt` e pending files (`test_*.txt`, `coverage_*.txt`)
- Reportar de volta o resumo das pendencias

Opus (voce) foca em: **escrever e corrigir testes**.

## Fluxo

1. Escrever/editar testes unitarios
2. Delegar a haiku: rodar `./scripts/test-check.sh` e ler pendings
3. Se houver falhas de teste (`test_*.txt`):
   - Corrigir testes ou codigo
   - Voltar ao passo 2
4. Se houver gaps de cobertura (`coverage_*.txt`):
   - Escrever testes para cobrir as linhas faltantes
   - Voltar ao passo 2
5. Quando zero falhas e zero gaps → informar o usuario que pode executar `/mutate`

## Convencoes de Teste

- Padrao **AAA** (Arrange, Act, Assert) obrigatorio
- Herdar de `TestBase` com `ITestOutputHelper`
- Usar: xUnit, Shouldly, Moq, Bogus
- Logging: `LogArrange()`, `LogAct()`, `LogAssert()`
- Relacao **1:1** entre projeto src e projeto de testes
- Nomenclatura: `Bedrock.UnitTests.<namespace-do-src>`

## Regras

- Escreva testes que cubram **todos os cenarios** (happy path, edge cases, erros)
- NAO rode mutacao nesta fase — foco em testes passando
- Maximo **5 iteracoes**. Se atingir, pare e informe o usuario.

## Formato dos Pendings

Cada `test_<projeto>_<NNN>.txt` contem:
```
PROJECT: <nome>
TEST: <nome-completo-do-teste>
MESSAGE: <mensagem-de-erro>
STACKTRACE: <primeira-linha-do-stack>
```

Cada `coverage_<projeto>_<NNN>.txt` contem:
```
PROJECT: <nome>
FILE: <caminho-relativo>
LINE_RATE: <percentual>%
TOTAL_LINES: <total>
COVERED_LINES: <cobertas>
UNCOVERED_LINES: <linhas-nao-cobertas-csv>
```

## Saida minima

Ao chamar scripts, NAO interprete o output completo. Leia apenas:
- `artifacts/pending/SUMMARY.txt`
- `artifacts/pending/test_*.txt` (falhas individuais)
- `artifacts/pending/coverage_*.txt` (gaps de cobertura)
