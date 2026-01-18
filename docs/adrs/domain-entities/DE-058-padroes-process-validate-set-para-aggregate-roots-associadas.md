# DE-058: Padrões Process, Validate e Set para Aggregate Roots Associadas

## Status
Aceita


## Contexto

### O Problema

Ao implementar associações entre Aggregate Roots (composição 1:1), surge a dúvida: os padrões de métodos `Process*Internal`, `Validate*For*Internal` e `Set*` são os mesmos usados para entidades filhas (composição 1:N)?

A resposta é **sim**, com pequenas diferenças contextuais.

### Relação com ADRs Existentes

Esta ADR estabelece que os padrões de Aggregate Roots Compostas (DE-036 a DE-045) se aplicam a Aggregate Roots Associadas, com as adaptações descritas abaixo.

## A Decisão

### Padrões Compartilhados

Os seguintes padrões são **idênticos** entre composição (1:N) e associação (1:1):

| Padrão | ADR Base | Aplica-se a Associações? |
|--------|----------|--------------------------|
| Método `Process*For*Internal` | DE-040 | ✅ Sim |
| Validação específica por operação | DE-041 | ✅ Sim |
| Nomenclatura `Validate*For*Internal` | DE-041 | ✅ Sim |
| Método `Set*` privado | DE-022 | ✅ Sim (diferente de coleções) |

### Diferenças Contextuais

#### 1. Entidade Recebida vs Criada Internamente

**Composição (1:N) - Entidade Filha:**
```csharp
// A Aggregate Root CRIA a entidade filha internamente
private bool ProcessCompositeChildEntityForRegisterNewInternal(
    ExecutionContext executionContext,
    ChildRegisterNewInput childRegisterNewInput  // Recebe INPUT
)
{
    // Cria a entidade filha via RegisterNew
    var registeredChild = CompositeChildEntity.RegisterNew(
        executionContext,
        childRegisterNewInput
    );

    if (registeredChild is null)
        return false;

    // Valida e adiciona à coleção
    // ...
}
```

**Associação (1:1) - Aggregate Root Associada:**
```csharp
// A Aggregate Root RECEBE a entidade já criada
private bool ProcessReferencedAggregateRootForRegisterNewInternal(
    ExecutionContext executionContext,
    ReferencedAggregateRoot? referencedAggregateRoot  // Recebe INSTÂNCIA
)
{
    // NÃO cria - apenas valida e atribui
    bool isValid = ValidateReferencedAggregateRootForRegisterNewInternal(
        executionContext,
        referencedAggregateRoot
    );

    if (!isValid)
        return false;

    return SetReferencedAggregateRoot(executionContext, referencedAggregateRoot);
}
```

#### 2. Método Set* Existe para Associações Singulares

**Composição (1:N) - Coleção:**
```csharp
// ❌ Coleções NÃO têm Set* (DE-044)
// Gerenciadas via Add/Remove na coleção interna
_compositeChildEntityCollection.Add(registeredChild);
```

**Associação (1:1) - Singular:**
```csharp
// ✅ Associações singulares TÊM Set*
private bool SetReferencedAggregateRoot(
    ExecutionContext executionContext,
    ReferencedAggregateRoot? referencedAggregateRoot
)
{
    bool isValid = ValidateReferencedAggregateRootIsRequired(
        executionContext,
        referencedAggregateRoot
    );

    if (!isValid)
        return false;

    ReferencedAggregateRoot = referencedAggregateRoot;

    return true;
}
```

#### 3. Validação de Null no Validate*For*Internal

**Composição (1:N):**
```csharp
// Entidade filha já foi criada via RegisterNew
// Se chegou aqui, não é null
public bool ValidateCompositeChildEntityForRegisterNewInternal(
    ExecutionContext executionContext,
    CompositeChildEntity compositeChildEntity  // Non-nullable
)
{
    // Valida duplicidade, regras de negócio, etc.
    return compositeChildEntity.IsValid(executionContext);
}
```

**Associação (1:1):**
```csharp
// AR associada pode ser null (IsRequired é dinâmico)
private static bool ValidateReferencedAggregateRootForRegisterNewInternal(
    ExecutionContext executionContext,
    ReferencedAggregateRoot? referencedAggregateRoot  // Nullable
)
{
    // Se null, IsRequired será validado no Set*
    if (referencedAggregateRoot is null)
        return true;

    // Validações específicas + IsValid da AR
    return referencedAggregateRoot.IsValid(executionContext);
}
```

### Resumo Visual

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    COMPOSIÇÃO (1:N) - Entidade Filha                        │
├─────────────────────────────────────────────────────────────────────────────┤
│ Process*For*Internal                                                        │
│   ├── Recebe INPUT (ChildRegisterNewInput)                                  │
│   ├── Cria entidade via RegisterNew()                                       │
│   ├── Chama Validate*For*Internal                                           │
│   └── Adiciona à coleção interna                                            │
│                                                                             │
│ Validate*For*Internal                                                       │
│   ├── Recebe entidade NON-NULLABLE (já foi criada)                          │
│   └── Valida regras de negócio (duplicidade, etc.)                          │
│                                                                             │
│ Set* → NÃO EXISTE para coleções (DE-044)                                    │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                    ASSOCIAÇÃO (1:1) - Aggregate Root                        │
├─────────────────────────────────────────────────────────────────────────────┤
│ Process*For*Internal                                                        │
│   ├── Recebe INSTÂNCIA já criada (ReferencedAggregateRoot?)                 │
│   ├── NÃO cria (ciclo de vida independente)                                 │
│   ├── Chama Validate*For*Internal                                           │
│   └── Chama Set* para validar IsRequired e atribuir                         │
│                                                                             │
│ Validate*For*Internal                                                       │
│   ├── Recebe entidade NULLABLE                                              │
│   ├── Se null, retorna true (IsRequired validado no Set*)                   │
│   └── Se não null, valida regras + IsValid()                                │
│                                                                             │
│ Set* → EXISTE para associações singulares                                   │
│   ├── Valida IsRequired via ValidateReferencedAggregateRootIsRequired       │
│   └── Atribui à propriedade                                                 │
└─────────────────────────────────────────────────────────────────────────────┘
```

## Consequências

### Benefícios

- **Consistência**: Mesmos padrões de nomenclatura e estrutura
- **Curva de aprendizado**: Quem conhece composição, entende associação
- **Previsibilidade**: Comportamento esperado em ambos os casos

### Trade-offs

- **Diferenças sutis**: É preciso entender quando a entidade é criada vs recebida
  - *Perspectiva*: A diferença reflete a semântica real (ciclo de vida independente vs dependente)

## ADRs Relacionadas

| ADR | Título | Relação |
|-----|--------|---------|
| [DE-040](./DE-040-processamento-entidades-filhas-uma-a-uma.md) | Processamento de Entidades Filhas Uma a Uma | Padrão base para `Process*For*Internal` |
| [DE-041](./DE-041-validacao-entidade-filha-especifica-operacao.md) | Validação de Entidade Filha Específica por Operação | Padrão base para `Validate*For*Internal` |
| [DE-022](./DE-022-metodos-set-privados.md) | Métodos Set* Privados | Padrão base para `Set*` |
| [DE-044](./DE-044-antipadrao-colecoes-sem-metodo-set.md) | Antipadrão: Coleções Não Têm Método Set | Explica por que coleções não têm Set* |
| [DE-057](./DE-057-metadata-aggregate-roots-associadas-apenas-isrequired.md) | Metadata de ARs Associadas - Apenas IsRequired | Complementar a esta ADR |

## Referências no Código

- [PrimaryAggregateRoot.cs](../../../templates/Domain.Entities/AssociatedAggregateRoots/PrimaryAggregateRoot.cs) - Implementação de referência para associações
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs) - Implementação de referência para composição
- [CompositeChildEntity.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeChildEntity.cs) - Entidade filha de referência
