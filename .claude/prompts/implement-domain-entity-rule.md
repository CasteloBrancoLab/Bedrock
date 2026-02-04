# Implementar Validação de ADR: Domain Entities Rule

## Variáveis

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `{{adr_id}}` | Identificador do ADR (DE-XXX) | `DE-003` |

---

## Prompt

Implemente uma nova regra de validação arquitetural para o ADR `{{adr_id}}` no diretório `src/BuildingBlocks/Testing/Architecture/Rules/DomainEntitiesRules/` seguindo os detalhes do prompt em .claude\prompts\implement-domain-entity-rule.md

### Passo 1: Ler o ADR

Ler o arquivo do ADR correspondente em `docs/adrs/domain-entities/` para entender completamente a regra a ser validada. Identificar:
- Qual comportamento é obrigatório
- Quais são as exceções permitidas
- Exemplos de código correto e incorreto

### Passo 2: Consultar Regras Existentes como Referência

Ler as regras existentes para seguir o padrão exato:
- `src/BuildingBlocks/Testing/Architecture/Rules/DomainEntitiesRules/DE001_SealedClassRule.cs`
- `src/BuildingBlocks/Testing/Architecture/Rules/DomainEntitiesRules/DE002_PrivateConstructorRule.cs`
- `src/BuildingBlocks/Testing/Architecture/Rule.cs` (classe base)
- `src/BuildingBlocks/Testing/Architecture/TypeContext.cs` (contexto disponível)
- `src/BuildingBlocks/Testing/Architecture/Violation.cs` (modelo de violação)

### Passo 3: Implementar a Regra

Criar o arquivo `DE{NNN}_{NomeDescritivo}Rule.cs` seguindo obrigatoriamente:

**Estrutura da classe:**
- Namespace: `Bedrock.BuildingBlocks.Testing.Architecture.Rules.DomainEntitiesRules`
- Classe `sealed` herdando de `Rule`
- XML doc comment descrevendo a regra e exceções
- Properties: `Name`, `Description`, `DefaultSeverity`, `AdrPath`
- Override de `AnalyzeType(TypeContext context)` retornando `Violation?`

**Convenções de nomenclatura:**
- Arquivo: `DE{NNN}_{NomeDescritivo}Rule.cs` (ex: `DE003_ImmutableCloneModifyReturnRule.cs`)
- Name: `"DE{NNN}_{NomeDescritivo}"` (ex: `"DE003_ImmutableCloneModifyReturn"`)
- Description: frase descritiva terminando com `(DE-{NNN})`

**Padrão de análise:**
- Filtrar tipos não aplicáveis no início (return null)
- Usar a API Roslyn (`INamedTypeSymbol`) para inspeção semântica
- Utilizar `context.GlobalInheritedTypes` quando necessário verificar herança cross-project
- Retornar `null` quando não há violação
- Retornar `Violation` com todos os campos preenchidos quando houver violação
- `LlmHint` deve ser uma instrução clara e direta de como corrigir

**Propriedades da Violation:**
```
Rule        = Name
Severity    = DefaultSeverity
Adr         = AdrPath
Project     = context.ProjectName
File        = context.RelativeFilePath
Line        = context.LineNumber
Message     = mensagem descritiva com o nome do tipo
LlmHint     = instrução de correção para code agent
```


### Passo 5: Executar O script de validação

Executar `scripts\architecture.sh` e resolver pendências geradas
