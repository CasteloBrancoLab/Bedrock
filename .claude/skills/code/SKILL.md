---
name: code
description: "Fase 1: Implementacao de codigo-fonte. Roda build-check e corrige erros."
argument-hint: "[issue-ou-requisito]"
disable-model-invocation: true
allowed-tools: Read, Edit, Write, Glob, Grep, Bash, Task
---

# /code - Fase 1: Implementacao

Voce esta na fase CODE. Foco exclusivo em implementar codigo-fonte.

## Argumentos

$ARGUMENTS

## Delegacao de Modelo

Use a Task tool com `model: "haiku"` para tarefas mecanicas:
- Rodar `./scripts/build-check.sh`
- Ler e parsear `artifacts/pending/SUMMARY.txt` e `artifacts/pending/build_errors.txt`
- Reportar de volta o resumo das pendencias

Opus (voce) foca em: **implementar e corrigir codigo-fonte**.

## Fluxo

1. Ler a issue/requisito (se fornecido nos argumentos)
2. Implementar codigo-fonte (sem testes)
3. Delegar a haiku: rodar `./scripts/build-check.sh` e ler pendings
4. Se houver erros de build:
   - Corrigir o codigo
   - Voltar ao passo 3
5. Quando build limpo → informar o usuario que pode executar `/arch`

## Regras

- **Apenas** codigo-fonte (src/). NAO escreva testes nesta fase.
- Respeite ADRs em `docs/adrs/` e convencoes do projeto.
- NAO execute testes, mutacao, ou pipeline completa.
- Maximo **5 iteracoes** de build. Se atingir, pare e informe o usuario.

## Saida minima

Ao chamar scripts, NAO interprete o output completo. Leia apenas:
- `artifacts/pending/SUMMARY.txt` (visao geral)
- `artifacts/pending/build_errors.txt` (se houver erros)
