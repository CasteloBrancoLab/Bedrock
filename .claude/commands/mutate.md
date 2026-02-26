# /mutate - Fase 4: Testes de Mutacao

Voce esta na fase MUTATE. Corrija testes para matar todos os mutantes.

## Argumentos

$ARGUMENTS

Se fornecido um diretorio de MutationTests, rode apenas para ele.

## Fluxo

1. Executar: `./scripts/mutate-check.sh` (ou `./scripts/mutate-check.sh <mutation-test-dir>`)
2. Ler: `artifacts/pending/SUMMARY.txt`
3. Se houver mutantes sobreviventes:
   - Ler cada `artifacts/pending/mutant_*.txt`
   - Analisar o mutador e a linha afetada
   - Adicionar/melhorar testes para matar o mutante
   - Voltar ao passo 1
4. Quando zero mutantes → informar o usuario que pode executar `/integration`

## Estrategias por Mutador

| Mutador | Como matar |
|---------|-----------|
| `ConditionalExpression` | Teste com valor que inverta a condicao |
| `EqualityOperator` | Teste boundary (==, !=, <, >, <=, >=) |
| `StringMutation` | Teste que valide o valor exato da string |
| `BooleanLiteral` | Teste que force ambos true e false |
| `ArithmeticOperator` | Teste que valide resultado aritmetico exato |
| `Statement` | Teste que dependa do efeito colateral do statement |

## Regras

- Threshold: **100%** (zero mutantes sobreviventes)
- NAO altere codigo-fonte para evitar mutantes — melhore os TESTES
- Se genuinamente impossivel testar, use `// Stryker disable once all : razao`
- Maximo **5 iteracoes**. Se atingir, pare e informe o usuario.

## Formato do Pending

Cada `mutant_<projeto>_<NNN>.txt` contem:
```
PROJECT: <nome>
FILE: <caminho>
LINE: <linha>
STATUS: Survived|NoCoverage
MUTATOR: <tipo>
DESCRIPTION: <descricao>
```

## Saida minima

Ao chamar scripts, NAO interprete o output completo. Leia apenas:
- `artifacts/pending/SUMMARY.txt`
- `artifacts/pending/mutant_*.txt` (mutantes individuais)
