# Revisão de Performance: Zero Allocation

## Variáveis

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `{{file_path}}` | Caminho do arquivo | `src/BuildingBlocks/Core/Extensions/StringExtensions.cs` |

---

## Prompt

Revise os objetos em em `{{file_path}}` para identificar e eliminar alocações desnecessárias conforme instruções @.claude/prompts/review-zero-allocation.md

### Análise Obrigatória

**1. Alocações de Heap**

Identificar e reportar:
- `new` de objetos/arrays (exceto quando inevitável)
- Boxing de value types (cast para object, interface em struct)
- Closures que capturam variáveis (lambdas com estado)
- Concatenação de strings com `+` ou interpolação sem handler
- `params T[]` que aloca array a cada chamada
- LINQ que aloca (`.ToList()`, `.ToArray()`, `.Select()`, iteradores)
- `ToString()` sem necessidade

**2. Oportunidades de Span/Memory**

Verificar onde aplicar:
- `Span<char>` em vez de `string.Substring()`
- `ReadOnlySpan<char>` para parsing sem alocação
- `stackalloc` para buffers pequenos (< 256 bytes)
- `MemoryExtensions` (`.AsSpan()`, `.Slice()`)
- `string.Create()` com `SpanAction` para construção

**3. Pooling**

Identificar candidatos para:
- `ArrayPool<T>.Shared` para buffers temporários
- `StringBuilder` pool via `StringBuilderCache` ou similar
- Object pooling para objetos frequentemente criados/descartados

### Formato de Saída

Para cada problema encontrado:

```
## [Nome do Problema]

**Localização:** linha X
**Alocação:** [tipo de alocação]
**Impacto:** [Alto/Médio/Baixo] - [motivo]

### Código Atual
[código problemático]

### Código Otimizado
[código sem alocação]

### Justificativa
[explicação técnica da mudança]
```

### Priorização

Ordenar problemas por:
1. **Hot path** - código executado frequentemente
2. **Tamanho da alocação** - buffers grandes primeiro
3. **Facilidade de correção** - quick wins

### Métricas Esperadas

Após correções:
- Zero alocações em hot paths
- Uso de `stackalloc` para buffers ≤ 256 bytes
- `ArrayPool` para buffers > 256 bytes
- `Span<T>` para todas operações de slice/substring

### Exemplo de Análise

```csharp
// ANTES: aloca substring + array
public string GetDomain(string email)
{
    var parts = email.Split('@');  // aloca array
    return parts[1];                // retorna string existente (ok)
}

// DEPOIS: zero allocation
public ReadOnlySpan<char> GetDomain(ReadOnlySpan<char> email)
{
    var atIndex = email.IndexOf('@');
    return email.Slice(atIndex + 1);  // sem alocação
}
```
