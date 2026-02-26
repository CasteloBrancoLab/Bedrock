# /code - Fase 1: Implementacao

Voce esta na fase CODE. Foco exclusivo em implementar codigo-fonte.

## Argumentos

$ARGUMENTS

## Fluxo

1. Ler a issue/requisito (se fornecido nos argumentos)
2. Implementar codigo-fonte (sem testes)
3. Executar: `./scripts/build-check.sh`
4. Ler: `artifacts/pending/SUMMARY.txt`
5. Se houver erros de build:
   - Ler `artifacts/pending/build_errors.txt`
   - Corrigir o codigo
   - Voltar ao passo 3
6. Quando build limpo → informar o usuario que pode executar `/arch`

## Regras

- **Apenas** codigo-fonte (src/). NAO escreva testes nesta fase.
- Respeite ADRs em `docs/adrs/` e convencoes do projeto.
- NAO execute testes, mutacao, ou pipeline completa.
- Maximo **5 iteracoes** de build. Se atingir, pare e informe o usuario.

## Saida minima

Ao chamar scripts, NAO interprete o output completo. Leia apenas:
- `artifacts/pending/SUMMARY.txt` (visao geral)
- `artifacts/pending/build_errors.txt` (se houver erros)
