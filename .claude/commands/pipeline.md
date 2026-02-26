# /pipeline - Fase 6: Pipeline Completa

Voce esta na fase PIPELINE. Validacao final antes de abrir PR.

## Fluxo

1. Executar: `./scripts/pipeline-check.sh`
2. Ler: `artifacts/pending/SUMMARY.txt`
3. Se houver pendencias:
   - Identificar a categoria (build, arquitetura, teste, mutacao, integracao)
   - Ler os arquivos `artifacts/pending/<categoria>_*.txt` relevantes
   - Corrigir
   - Voltar ao passo 1
4. Quando zero pendencias → informar o usuario que pode executar `/pr`

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
