# /integration - Fase 5: Testes de Integracao

Voce esta na fase INTEGRATION. Execute e corrija testes de integracao.

## Argumentos

$ARGUMENTS

Se fornecido um caminho de projeto, rode testes apenas para ele.

## Fluxo

1. Executar: `./scripts/integration-check.sh` (ou `./scripts/integration-check.sh <projeto.csproj>`)
2. Ler: `artifacts/pending/SUMMARY.txt`
3. Se houver falhas:
   - Ler cada `artifacts/pending/integration_*.txt`
   - Corrigir testes ou codigo de integracao
   - Voltar ao passo 1
4. Quando zero falhas → informar o usuario que pode executar `/pipeline`

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
