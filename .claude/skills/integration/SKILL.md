---
name: integration
description: "Fase 5: Testes de integracao com Docker/Testcontainers. Roda integration-check e corrige falhas."
argument-hint: "[projeto.csproj]"
disable-model-invocation: true
allowed-tools: Read, Edit, Write, Glob, Grep, Bash, Task
---

# /integration - Fase 5: Testes de Integracao

Voce esta na fase INTEGRATION. Execute e corrija testes de integracao.

## Argumentos

$ARGUMENTS

Se fornecido um caminho de projeto, rode testes apenas para ele.

## Delegacao de Modelo

Use a Task tool com `model: "haiku"` para tarefas mecanicas:
- Rodar `./scripts/integration-check.sh` (ou com argumento de projeto)
- Ler e parsear `artifacts/pending/SUMMARY.txt` e `artifacts/pending/integration_*.txt`
- Reportar de volta o resumo das pendencias

Opus (voce) foca em: **corrigir testes e codigo de integracao**.

## Fluxo

1. Delegar a haiku: rodar `./scripts/integration-check.sh` e ler pendings
2. Se houver falhas:
   - Corrigir testes ou codigo de integracao
   - Voltar ao passo 1
3. Quando zero falhas → informar o usuario que pode executar `/pipeline`

## Pre-requisitos

- **Docker** deve estar disponivel (Testcontainers)
- Se WSL2: `DOCKER_HOST=tcp://127.0.0.1:2375` (usar 127.0.0.1, NAO localhost)
- O `DockerHostSetup` em `src/BuildingBlocks/Testing/Integration/DockerHostSetup.cs` corrige IPv6/Ryuk automaticamente

## Regras

- Testes de integracao usam containers reais (PostgreSQL, Redis, etc.)
- NAO altere testes unitarios ou de mutacao nesta fase
- Maximo **5 iteracoes**. Se atingir, pare e informe o usuario.

## Formato do Pending

Cada `integration_<projeto>_<NNN>.txt` contem:
```
PROJECT: <nome>
TEST: <nome-completo-do-teste>
MESSAGE: <mensagem-de-erro>
STACKTRACE: <primeira-linha-do-stack>
```

## Saida minima

Ao chamar scripts, NAO interprete o output completo. Leia apenas:
- `artifacts/pending/SUMMARY.txt`
- `artifacts/pending/integration_*.txt` (falhas individuais)
