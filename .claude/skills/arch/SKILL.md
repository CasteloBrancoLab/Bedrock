---
name: arch
description: "Fase 2: Validacao de regras arquiteturais. Roda arch-check e corrige violacoes."
disable-model-invocation: true
allowed-tools: Read, Edit, Write, Glob, Grep, Bash, Task
---

# /arch - Fase 2: Testes de Arquitetura

Voce esta na fase ARCH. Valide que o codigo segue as regras arquiteturais.

## Delegacao de Modelo

Use a Task tool com `model: "haiku"` para tarefas mecanicas:
- Rodar `./scripts/arch-check.sh`
- Ler e parsear `artifacts/pending/SUMMARY.txt` e `artifacts/pending/architecture_*.txt`
- Reportar de volta o resumo das pendencias

Opus (voce) foca em: **corrigir codigo-fonte para cumprir regras arquiteturais**.

## Fluxo

1. Delegar a haiku: rodar `./scripts/arch-check.sh` e ler pendings
2. Se houver violacoes:
   - Corrigir o codigo-fonte para cumprir a regra
   - Voltar ao passo 1
3. Quando zero violacoes → informar o usuario que pode executar `/test`

## Regras

- Corrija **apenas** violacoes de arquitetura. NAO altere testes.
- Cada arquivo de violacao contem: RULE, SEVERITY, FILE, LINE, MESSAGE, LLM_HINT.
- O campo LLM_HINT contem instrucoes diretas de como corrigir.
- Maximo **5 iteracoes**. Se atingir, pare e informe o usuario.

## Saida minima

Ao chamar scripts, NAO interprete o output completo. Leia apenas:
- `artifacts/pending/SUMMARY.txt`
- `artifacts/pending/architecture_*.txt` (violacoes individuais)
