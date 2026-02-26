---
paths:
  - "tests/MutationTests/**"
---

# Convencoes de Testes de Mutacao (Stryker.NET)

## Threshold

- Minimo: **100%** (zero mutantes sobreviventes)
- Codigo desenvolvido com IA nao aceita mutantes sobreviventes

## Estrutura

Cada projeto de UnitTests tem um correspondente em MutationTests:

```
tests/MutationTests/<componente>/
└── stryker-config.json
```

## Configuracao (stryker-config.json)

```json
{
  "$schema": "https://raw.githubusercontent.com/stryker-mutator/stryker-net/master/src/Stryker.Core/Stryker.Core/stryker-config.schema.json",
  "stryker-config": {
    "project": "<projeto-src>.csproj",
    "test-projects": [
      "../../../UnitTests/<componente>/<projeto-test>.csproj"
    ],
    "reporters": ["html", "progress"],
    "thresholds": { "high": 100, "low": 100, "break": 100 }
  }
}
```

## Exclusoes de Codigo Nao-Testavel

Para codigo **genuinamente impossivel de testar**:

```csharp
// Stryker disable all : Razao em pt-BR
[ExcludeFromCodeCoverage(Justification = "Razao em pt-BR")]
private static void MetodoImpossivelDeTestar() { }
// Stryker restore all
```

### Regras para exclusao

- Usar **apenas** quando genuinamente impossivel testar
- **Sempre** incluir justificativa em pt-BR
- Preferir `disable once` (granular) sobre `disable all` (bloco)
- Requer `using System.Diagnostics.CodeAnalysis;`

### Stryker comments disponiveis

| Comentario | Uso |
|------------|-----|
| `// Stryker disable all : reason` | Desabilita todas as mutacoes ate `restore` |
| `// Stryker restore all` | Restaura mutacoes |
| `// Stryker disable once all : reason` | Desabilita apenas na proxima linha |
| `// Stryker disable once Statement : reason` | Desabilita remocao de statement |
| `// Stryker disable Equality,Arithmetic : reason` | Desabilita mutadores especificos |
