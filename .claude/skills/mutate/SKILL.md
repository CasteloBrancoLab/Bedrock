---
name: mutate
description: "Fase 4: Testes de mutacao. Roda mutate-check e mata mutantes sobreviventes."
argument-hint: "[mutation-test-dir]"
disable-model-invocation: true
allowed-tools: Read, Edit, Write, Glob, Grep, Bash, Task
---

# /mutate - Fase 4: Testes de Mutacao

Voce esta na fase MUTATE. Corrija testes para matar todos os mutantes.

## Argumentos

$ARGUMENTS

Se fornecido um diretorio de MutationTests, rode apenas para ele.

## Delegacao de Modelo

Use a Task tool com `model: "haiku"` para tarefas mecanicas:
- Rodar `./scripts/mutate-check.sh` (ou com argumento de diretorio)
- Ler e parsear `artifacts/pending/SUMMARY.txt` e `artifacts/pending/mutant_*.txt`
- Reportar de volta o resumo das pendencias

Opus (voce) foca em: **escrever e melhorar testes para matar mutantes**.

## Fluxo

1. Delegar a haiku: rodar `./scripts/mutate-check.sh` e ler pendings
2. Se houver mutantes sobreviventes:
   - Analisar o mutador e a linha afetada
   - Adicionar/melhorar testes para matar o mutante
   - Voltar ao passo 1
3. Quando zero mutantes → informar o usuario que pode executar `/integration`

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
