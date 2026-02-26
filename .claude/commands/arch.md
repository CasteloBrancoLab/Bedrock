# /arch - Fase 2: Testes de Arquitetura

Voce esta na fase ARCH. Valide que o codigo segue as regras arquiteturais.

## Fluxo

1. Executar: `./scripts/arch-check.sh`
2. Ler: `artifacts/pending/SUMMARY.txt`
3. Se houver violacoes:
   - Ler cada `artifacts/pending/architecture_*.txt`
   - Corrigir o codigo-fonte para cumprir a regra
   - Voltar ao passo 1
4. Quando zero violacoes → informar o usuario que pode executar `/test`

## Regras

- Corrija **apenas** violacoes de arquitetura. NAO altere testes.
- Cada arquivo de violacao contem: RULE, SEVERITY, FILE, LINE, MESSAGE, LLM_HINT.
- O campo LLM_HINT contem instrucoes diretas de como corrigir.
- Maximo **5 iteracoes**. Se atingir, pare e informe o usuario.

## Saida minima

Ao chamar scripts, NAO interprete o output completo. Leia apenas:
- `artifacts/pending/SUMMARY.txt`
- `artifacts/pending/architecture_*.txt` (violacoes individuais)
