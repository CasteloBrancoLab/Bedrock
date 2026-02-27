---
name: pipeline
description: "Fase 6: Pipeline completa. Validacao final antes de abrir PR."
disable-model-invocation: true
allowed-tools: Read, Edit, Write, Glob, Grep, Bash, Task
---

# /pipeline - Fase 6: Pipeline Completa

Voce esta na fase PIPELINE. Validacao final antes de abrir PR.

## Delegacao de Modelo

Use a Task tool com `model: "haiku"` para tarefas mecanicas:
- Rodar `./scripts/pipeline-check.sh`
- Ler e parsear `artifacts/pending/SUMMARY.txt` e pending files de todas as categorias
- Reportar de volta o resumo das pendencias

Opus (voce) foca em: **corrigir codigo com base nas pendencias reportadas**.

## Fluxo

1. Delegar a haiku: rodar `./scripts/pipeline-check.sh` e ler pendings
2. Se houver pendencias:
   - Identificar a categoria (build, arquitetura, teste, mutacao, integracao, cobertura)
   - Corrigir
   - Voltar ao passo 1
3. Quando zero pendencias → informar o usuario que pode executar `/pr`

## O que a pipeline executa

1. Clean (bin/obj + artifacts)
2. Build
3. Architecture Tests
4. Unit Tests + Coverage
5. Mutation Tests
6. Integration Tests
7. Report Generation
8. Pending Extraction + SUMMARY.txt

## Regras

- Esta fase roda **tudo do zero** (clean build)
- Todas as categorias devem estar zeradas para passar
- Maximo **5 iteracoes**. Se atingir, pare e informe o usuario.
- NAO abra PR ate a pipeline passar completamente

## Saida minima

Ao chamar scripts, NAO interprete o output completo. Leia apenas:
- `artifacts/pending/SUMMARY.txt`
- Arquivos especificos de pendencia conforme a categoria
