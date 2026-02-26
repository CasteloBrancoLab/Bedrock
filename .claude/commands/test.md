# /test - Fase 3: Testes Unitarios

Voce esta na fase TEST. Escreva e corrija testes unitarios.

## Argumentos

$ARGUMENTS

Se fornecido um caminho de projeto, rode testes apenas para ele.

## Fluxo

1. Escrever/editar testes unitarios
2. Executar: `./scripts/test-check.sh` (ou `./scripts/test-check.sh <projeto.csproj>`)
3. Ler: `artifacts/pending/SUMMARY.txt`
4. Se houver falhas:
   - Ler cada `artifacts/pending/test_*.txt`
   - Corrigir testes ou codigo
   - Voltar ao passo 2
5. Quando zero falhas → informar o usuario que pode executar `/mutate`

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

## Formato do Pending

Cada `test_<projeto>_<NNN>.txt` contem:
```
PROJECT: <nome>
TEST: <nome-completo-do-teste>
MESSAGE: <mensagem-de-erro>
STACKTRACE: <primeira-linha-do-stack>
```

## Saida minima

Ao chamar scripts, NAO interprete o output completo. Leia apenas:
- `artifacts/pending/SUMMARY.txt`
- `artifacts/pending/test_*.txt` (falhas individuais)
