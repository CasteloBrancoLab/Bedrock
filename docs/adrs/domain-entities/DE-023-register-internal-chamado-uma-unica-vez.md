# DE-023: Register*Internal Chamado Uma Única Vez

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma **fábrica de carros** com uma linha de montagem:

**Linha de montagem correta**:
- Chassis passa UMA VEZ pela linha
- Motor é instalado
- Rodas são montadas
- Pintura é aplicada
- Carro completo sai no final com número de série único

**Linha de montagem errada**:
- Chassis passa pela linha → recebe número de série 001
- Mesma peça volta para linha → recebe número de série 002
- Mesma peça volta novamente → recebe número de série 003
- Resultado: produto com 3 números de série diferentes, histórico confuso, impossível rastrear

Em entidades de domínio, `RegisterNewInternal` e `RegisterChangeInternal` são como a linha de montagem - devem ser chamados **UMA ÚNICA VEZ** por operação pública. Múltiplas chamadas criam múltiplos clones, múltiplas versões, estado inconsistente.

---

### O Problema Técnico

`RegisterNewInternal` e `RegisterChangeInternal` gerenciam automaticamente:
- Clone da instância (para imutabilidade)
- Incremento de versão (para concurrency control)
- Atualização de auditoria (CreatedAt/By, ModifiedAt/By)
- Registro de eventos de domínio

Chamar esses métodos múltiplas vezes causa:

```csharp
// ❌ ERRADO - Múltiplas chamadas a RegisterChangeInternal
public SimpleAggregateRoot? UpdateProfile(
    ExecutionContext ctx,
    UpdateProfileInput input
)
{
    // Problema 1: Cria clone 1, versão incrementada para 2
    var result1 = RegisterChangeInternal(ctx, this, input, handler: (ctx, inp, n) =>
        n.ChangeNameInternal(ctx, inp.FirstName, inp.LastName));

    if (result1 == null) return null;

    // Problema 2: Cria clone 2, versão incrementada para 3
    var result2 = result1.RegisterChangeInternal(ctx, result1, input, handler: (ctx, inp, n) =>
        n.ChangeBirthDateInternal(ctx, inp.BirthDate));

    if (result2 == null) return null;

    // Problema 3: Cria clone 3, versão incrementada para 4
    var result3 = result2.RegisterChangeInternal(ctx, result2, input, handler: (ctx, inp, n) =>
        n.ChangeAddressInternal(ctx, inp.Address));

    return result3; // Versão pulou de 1 para 4, 2 clones intermediários descartados
}

// Problemas:
// 1. Três clones criados desnecessariamente (overhead de memória)
// 2. Versão incrementada 3x quando deveria ser apenas 1x
// 3. Auditoria inconsistente (ModifiedAt atualizado múltiplas vezes)
// 4. Eventos duplicados ou incorretos
// 5. Performance ruim (clone é operação cara)
```

## Como Normalmente é Feito

### Abordagem Tradicional

Muitos frameworks permitem chamadas encadeadas que parecem elegantes, mas criam problemas:

```csharp
// ⚠️ Abordagem "fluent" com múltiplas chamadas
public class Person
{
    public Person UpdateName(string firstName, string lastName)
    {
        var clone = this.Clone();
        clone.IncrementVersion();          // Versão +1
        clone.FirstName = firstName;
        clone.LastName = lastName;
        return clone;
    }

    public Person UpdateBirthDate(DateTime birthDate)
    {
        var clone = this.Clone();
        clone.IncrementVersion();          // Versão +1
        clone.BirthDate = birthDate;
        return clone;
    }

    public Person UpdateProfile(string firstName, string lastName, DateTime birthDate)
    {
        return this
            .UpdateName(firstName, lastName)      // Clone 1, versão incrementada
            .UpdateBirthDate(birthDate);         // Clone 2, versão incrementada
        // Resultado: 2 clones, versão incrementada 2x
    }
}
```

### Por Que Não Funciona Bem

1. **Versões inconsistentes**: Cada operação incrementa versão, criando "buracos" na sequência
   ```
   Versão original: 1
   Após UpdateName: 2
   Após UpdateBirthDate: 3
   Mas UpdateProfile deveria resultar em versão 2, não 3!
   ```

2. **Performance ruim**: Clone é operação cara (copia toda estrutura), fazer múltiplas vezes é desperdício

3. **Auditoria confusa**: `ModifiedAt` atualizado múltiplas vezes dentro da mesma operação lógica

4. **Eventos duplicados**: Cada chamada pode registrar eventos, criando histórico incorreto

5. **Concurrency control quebrado**: Versões puladas impedem detecção correta de conflitos
   ```csharp
   // Thread 1: lê versão 1, tenta salvar versão 3 (pulou 2)
   // Thread 2: lê versão 1, tenta salvar versão 2
   // Ambos passam no optimistic lock check - CONFLITO NÃO DETECTADO!
   ```

6. **Difícil de testar**: Versões imprevisíveis complicam asserções em testes

## A Decisão

### Nossa Abordagem

**Regra fundamental**: Cada método público DEVE chamar `Register*Internal` **UMA ÚNICA VEZ**, combinando múltiplos métodos `*Internal` dentro de uma única chamada.

**Regra adicional**: O handler DEVE ser `static` para evitar closures acidentais (ver seção "Handler Deve Ser Static" abaixo).

```csharp
public sealed class SimpleAggregateRoot : EntityBase<SimpleAggregateRoot>
{
    // -----------------------------------------------------------------------
    // ✅ CORRETO - Uma chamada, múltiplos métodos internos, handler static
    // -----------------------------------------------------------------------

    public static SimpleAggregateRoot? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: (ctx, inp) => new SimpleAggregateRoot(),
            handler: static (ctx, inp, instance) =>
            {
                // UMA chamada a RegisterNewInternal
                // MÚLTIPLOS métodos *Internal dentro
                return
                    instance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
                    & instance.ChangeBirthDateInternal(ctx, inp.BirthDate);
            }
        );
    }

    public SimpleAggregateRoot? UpdateProfile(
        ExecutionContext executionContext,
        UpdateProfileInput input
    )
    {
        return RegisterChangeInternal(
            executionContext,
            instance: this,
            input,
            handler: static (ctx, inp, newInstance) =>
            {
                // UMA chamada a RegisterChangeInternal
                // MÚLTIPLOS métodos *Internal dentro
                return
                    newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
                    & newInstance.ChangeBirthDateInternal(ctx, inp.BirthDate)
                    & newInstance.ChangeAddressInternal(ctx, inp.Address);
            }
        );
    }

    public SimpleAggregateRoot? ChangeName(
        ExecutionContext executionContext,
        ChangeNameInput input
    )
    {
        return RegisterChangeInternal(
            executionContext,
            instance: this,
            input,
            handler: static (ctx, inp, newInstance) =>
            {
                // UMA chamada a RegisterChangeInternal
                // UM método *Internal (operação específica)
                return newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName);
            }
        );
    }
}
```

### Handler Deve Ser Static

O parâmetro handler DEVE usar a keyword `static` para prevenir captura acidental de variáveis do escopo externo (closures):

```csharp
// ✅ CORRETO - static lambda
handler: static (ctx, inp, newInstance) =>
{
    return newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName);
}

// ❌ ERRADO - lambda não-static pode capturar variáveis acidentalmente
handler: (ctx, inp, newInstance) =>
{
    return newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName);
}
```

**Razões para usar `static`**:

1. **Segurança**: `static` causa erro de compilação se tentar capturar variável externa
2. **Performance**: static lambdas não alocam objetos de closure no heap
3. **Clareza**: Torna explícito que o handler depende APENAS dos parâmetros recebidos
4. **Prevenção de bugs**: Evita captura acidental de `this` ou outras variáveis

**Exemplo de bug prevenido**:

```csharp
var valorExterno = 42;

// ❌ Sem static - compila mas cria closure:
handler: (ctx, inp, inst) => {
    Console.WriteLine(valorExterno); // Captura acidental!
    return inst.ChangeNameInternal(...);
}

// ✅ Com static - NÃO compila, erro de compilação:
handler: static (ctx, inp, inst) => {
    Console.WriteLine(valorExterno); // CS8820: Cannot use 'valorExterno'
    return inst.ChangeNameInternal(...);
}
```

### Anatomia da Chamada Única

```csharp
// O que acontece DENTRO de RegisterChangeInternal:
return RegisterChangeInternal(ctx, this, input, handler: static (ctx, inp, newInstance) =>
{
    // 1. ✅ Clone JÁ foi criado (newInstance)
    // 2. ✅ Versão e auditoria JÁ foram atualizadas em newInstance
    // 3. ✅ newInstance já possui EntityInfo atualizado ANTES do handler

    // Você só foca na LÓGICA de negócio:
    return
        newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
        & newInstance.ChangeBirthDateInternal(ctx, inp.BirthDate);
});

// Fluxo interno:
// RegisterChangeInternal()
//   → Clone()                          [1 clone criado]
//   → SetEntityInfo() em newInstance   [versão+1, auditoria atualizada]
//   → handler() executa                [validações e modificações]
//   → Se sucesso: Retorna newInstance
//   → Se falha: Retorna null (clone descartado)
```

**Importante**: A versão e auditoria são atualizadas **ANTES** do handler, não depois. Isso significa que dentro do handler, `newInstance.EntityInfo` já contém a versão incrementada e timestamps atualizados. Se o handler falhar, o clone (com versão já incrementada) é simplesmente descartado - a instância original permanece inalterada.

**Comparando versão anterior com nova**: Se você precisar comparar a versão anterior com a nova dentro do handler, passe `this` como parte do Input:

```csharp
public readonly record struct UpdateWithComparisonInput(
    SimpleAggregateRoot Original,  // ✅ Referência à instância original
    string FirstName,
    string LastName
);

handler: static (ctx, inp, newInstance) =>
{
    // inp.Original.EntityInfo.EntityVersion = versão antes da mudança
    // newInstance.EntityInfo.EntityVersion = versão após a mudança
    var previousVersion = inp.Original.EntityInfo.EntityVersion;
    var newVersion = newInstance.EntityInfo.EntityVersion;

    return newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName);
}
```

### Por Que Usar Operador & (Bitwise AND)

```csharp
// & garante que TODAS as validações executam
bool isSuccess =
    newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
    & newInstance.ChangeBirthDateInternal(ctx, inp.BirthDate)
    & newInstance.ChangeAddressInternal(ctx, inp.Address);

// BENEFÍCIOS:
// ✅ Usuário recebe TODOS os erros de validação de uma vez
// ✅ Todas as propriedades são validadas, mesmo se alguma falhar
// ✅ UX superior: corrigir tudo de uma vez vs. um erro por vez

// Comparação com &&:
bool isSuccessWrong =
    newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
    && newInstance.ChangeBirthDateInternal(ctx, inp.BirthDate)
    && newInstance.ChangeAddressInternal(ctx, inp.Address);
// ❌ Se ChangeNameInternal falhar, as outras NUNCA executam
// ❌ Usuário vê apenas 1 erro por vez (UX ruim)
```

### O Que Register*Internal Gerencia Automaticamente

| Aspecto | Gerenciado Por | Quando |
|---------|----------------|--------|
| **Clone** | Register*Internal | Antes do handler |
| **Versão** | Register*Internal | Antes do handler (já no clone) |
| **Auditoria** | Register*Internal | Antes do handler (já no clone) |
| **Validação** | Métodos *Internal | Dentro do handler |
| **Lógica de negócio** | Métodos *Internal | Dentro do handler |
| **Descarte se falha** | Register*Internal | Após handler (retorna null) |

## Benefícios

1. **Versões consistentes e previsíveis**
   ```csharp
   var person = SimpleAggregateRoot.RegisterNew(...); // Versão 1
   var updated = person.UpdateProfile(...);           // Versão 2 (sempre!)

   // Não importa quantos *Internal foram chamados,
   // versão incrementa EXATAMENTE 1 vez por operação pública
   ```

2. **Performance otimizada**
   ```csharp
   // ✅ Uma chamada: 1 clone
   var result = RegisterChangeInternal(..., handler: (ctx, inp, n) =>
       n.ChangeNameInternal(...) & n.ChangeBirthDateInternal(...));
   // Clone: ~1µs

   // ❌ Múltiplas chamadas: 2 clones
   var r1 = RegisterChangeInternal(..., handler: n => n.ChangeNameInternal(...));
   var r2 = r1.RegisterChangeInternal(..., handler: n => n.ChangeBirthDateInternal(...));
   // Clone: ~2µs (2x mais lento)
   ```

3. **Auditoria precisa**
   ```csharp
   // ✅ Uma chamada:
   ModifiedAt: 2025-01-15T10:30:00Z  // Timestamp único e correto

   // ❌ Múltiplas chamadas:
   ModifiedAt: 2025-01-15T10:30:00.123Z  // Primeira chamada
   ModifiedAt: 2025-01-15T10:30:00.456Z  // Segunda chamada (incorreto!)
   ```

4. **Concurrency control correto**
   ```csharp
   // Optimistic locking funciona corretamente:
   // UPDATE Entities
   // SET ..., EntityVersion = 2
   // WHERE Id = @Id AND EntityVersion = 1

   // Versão sempre incrementa 1 por 1, sem pulos
   ```

5. **Eventos consistentes**
   ```csharp
   // ✅ Uma chamada: 1 evento ProfileUpdated
   // ❌ Múltiplas chamadas: NameChanged, BirthDateChanged, AddressChanged
   //    (3 eventos quando deveria ser 1 operação lógica)
   ```

6. **Código previsível e testável**
   ```csharp
   [Fact]
   public void UpdateProfile_Should_IncrementVersionByOne()
   {
       var person = CreatePerson(); // Versão 1
       var updated = person.UpdateProfile(...);

       updated.EntityInfo.Version.Should().Be(2); // Sempre previsível!
   }
   ```

## Trade-offs (Com Perspectiva)

- **Handler aninhado pode parecer verboso**
  ```csharp
  return RegisterChangeInternal(..., handler: static (ctx, inp, newInstance) =>
  {
      return newInstance.ChangeNameInternal(...) & newInstance.ChangeBirthDateInternal(...);
  });
  ```
  - **Mitigação**: Verbosidade explícita é melhor que bugs sutis. A estrutura é consistente e previsível.

- **Todos os *Internal devem estar em uma única expressão**
  ```csharp
  // Pode exigir variáveis intermediárias para lógica complexa
  handler: static (ctx, inp, newInstance) =>
  {
      string fullName = $"{inp.FirstName} {inp.LastName}"; // Cálculo intermediário OK
      return newInstance.ChangeNameInternal(...) & newInstance.ChangeAddressInternal(...);
  }
  ```
  - **Mitigação**: Lógica complexa indica que talvez precise de um método *Internal adicional

### Trade-offs Frequentemente Superestimados

**"Handler aninhado é difícil de entender"**

Na prática, o padrão é consistente e auto-explicativo:

```csharp
// Todo método público segue EXATAMENTE o mesmo padrão:
return Register*Internal(ctx, ..., handler: static (ctx, inp, newInstance) =>
{
    return newInstance.*Internal(...) & newInstance.*Internal(...);
});

// Após ver 2-3 exemplos, o padrão se torna natural
```

**"Preciso de múltiplas chamadas para operações condicionais"**

Condicionalidade pode ser dentro do handler:

```csharp
handler: static (ctx, inp, newInstance) =>
{
    bool success = newInstance.ChangeNameInternal(...);

    // Condicional: só muda endereço se nome válido
    if (success && inp.Address != null)
        success &= newInstance.ChangeAddressInternal(ctx, inp.Address);

    return success;
}
```

**"Performance do operador & é ruim"**

O custo de avaliar validações extras (~50-100ns cada) é desprezível comparado ao custo de clone (~1µs):

```csharp
// Custo de 3 validações com &:
bool success = Validate1() & Validate2() & Validate3();
// ~150ns total

// Custo de 1 clone extra por múltiplas chamadas:
// ~1000ns (7x mais caro que 3 validações!)
```

## Fundamentação Teórica

### O Que o DDD Diz

Eric Evans em "Domain-Driven Design" (2003) sobre invariantes de agregados:

> "The AGGREGATE root is responsible for checking the fulfillment of all invariants, but it delegates the actual execution to the appropriate ENTITIES within the AGGREGATE."
>
> *A raiz do AGGREGATE é responsável por verificar o cumprimento de todos os invariantes, mas delega a execução real para as ENTITIES apropriadas dentro do AGGREGATE.*

`RegisterChangeInternal` é a raiz que verifica invariantes (versão, auditoria, eventos). Métodos `*Internal` são as entidades que executam lógica específica. Uma chamada = uma verificação atômica de invariantes.

Vaughn Vernon em "Implementing Domain-Driven Design" (2013) sobre atomicidade:

> "An Aggregate is a consistency boundary. [...] All invariant rules of the Aggregate must be satisfied before the transaction is committed."
>
> *Um Aggregate é um limite de consistência. [...] Todas as regras invariantes do Aggregate devem ser satisfeitas antes da transação ser confirmada.*

Múltiplas chamadas a `Register*Internal` = múltiplos limites de consistência = quebra da atomicidade. Uma operação lógica DEVE ter um único limite de consistência.

### O Que o Clean Code Diz

Robert C. Martin em "Clean Code" (2008) sobre níveis de abstração:

> "Functions should do one thing. They should do it well. They should do it only."
>
> *Funções devem fazer uma coisa. Devem fazer bem. Devem fazer apenas isso.*

Método público faz **UMA coisa**: executar operação de negócio completa. Isso significa **UMA chamada** a `Register*Internal` para gerenciar ciclo de vida completo.

O princípio **"Don't Repeat Yourself" (DRY)**:

> "Every piece of knowledge must have a single, unambiguous, authoritative representation within a system."
>
> *Cada pedaço de conhecimento deve ter uma representação única, inequívoca e autoritativa dentro de um sistema.*

Lógica de incremento de versão, auditoria e eventos está em `Register*Internal`. Chamar múltiplas vezes = repetir essa lógica = violação do DRY.

### O Que o Clean Architecture Diz

Robert C. Martin em "Clean Architecture" (2017) sobre limites de transação:

> "Use cases should be transaction boundaries. All the work of a use case should be done in a single transaction."
>
> *Casos de uso devem ser limites de transação. Todo o trabalho de um caso de uso deve ser feito em uma única transação.*

Método público = use case = limite de transação = **UMA** chamada a `Register*Internal`.

### Command Pattern

Gang of Four em "Design Patterns" (1994):

> "Encapsulate a request as an object, thereby letting you parameterize clients with different requests, queue or log requests, and support undoable operations."
>
> *Encapsule uma requisição como um objeto, permitindo parametrizar clientes com diferentes requisições, enfileirar ou logar requisições, e suportar operações reversíveis.*

Cada método público é um Command. Commands são **atômicos** - executam completamente ou falham completamente. Múltiplas chamadas a `Register*Internal` quebram a atomicidade.

### Transaction Script Pattern

Martin Fowler em "Patterns of Enterprise Application Architecture" (2002):

> "A Transaction Script organizes all business logic for a single transaction in a single procedure."
>
> *Um Transaction Script organiza toda a lógica de negócio para uma única transação em um único procedimento.*

Método público = transaction script. Todo ciclo de vida (clone, versão, auditoria) DEVE estar em **UMA** execução do script.

## Antipadrões Documentados

### Antipadrão 1: Múltiplas Chamadas Sequenciais

```csharp
// ❌ ERRADO - Cada chamada cria clone e incrementa versão
public SimpleAggregateRoot? UpdateProfile(ExecutionContext ctx, UpdateProfileInput input)
{
    var step1 = RegisterChangeInternal(ctx, this, input, handler: (c, i, n) =>
        n.ChangeNameInternal(c, i.FirstName, i.LastName));

    if (step1 == null) return null;

    var step2 = step1.RegisterChangeInternal(ctx, step1, input, handler: (c, i, n) =>
        n.ChangeBirthDateInternal(c, i.BirthDate));

    if (step2 == null) return null;

    var step3 = step2.RegisterChangeInternal(ctx, step2, input, handler: (c, i, n) =>
        n.ChangeAddressInternal(c, i.Address));

    return step3;
    // Resultado: 3 clones, versão incrementada 3x, 3 timestamps diferentes
}

// ✅ CORRETO - Uma chamada, todos os métodos *Internal juntos, handler static
public SimpleAggregateRoot? UpdateProfile(ExecutionContext ctx, UpdateProfileInput input)
{
    return RegisterChangeInternal(ctx, this, input, handler: static (c, i, n) =>
        n.ChangeNameInternal(c, i.FirstName, i.LastName)
        & n.ChangeBirthDateInternal(c, i.BirthDate)
        & n.ChangeAddressInternal(c, i.Address));
    // Resultado: 1 clone, versão incrementada 1x, 1 timestamp
}
```

### Antipadrão 2: Método Público Chamando Outro Público

```csharp
// ❌ ERRADO - UpdateProfile chama ChangeName que chama RegisterChangeInternal
public SimpleAggregateRoot? ChangeName(ExecutionContext ctx, ChangeNameInput input)
{
    return RegisterChangeInternal(ctx, this, input, handler: (c, i, n) =>
        n.ChangeNameInternal(c, i.FirstName, i.LastName));
}

public SimpleAggregateRoot? UpdateProfile(ExecutionContext ctx, UpdateProfileInput input)
{
    // Problema: ChangeName já chama RegisterChangeInternal
    var step1 = this.ChangeName(ctx, new ChangeNameInput(input.FirstName, input.LastName));
    if (step1 == null) return null;

    // Problema: ChangeBirthDate também chama RegisterChangeInternal
    var step2 = step1.ChangeBirthDate(ctx, new ChangeBirthDateInput(input.BirthDate));

    return step2; // 2 chamadas a RegisterChangeInternal, versão +2
}

// ✅ CORRETO - UpdateProfile chama Register*Internal diretamente, handler static
public SimpleAggregateRoot? UpdateProfile(ExecutionContext ctx, UpdateProfileInput input)
{
    return RegisterChangeInternal(ctx, this, input, handler: static (c, i, n) =>
        n.ChangeNameInternal(c, i.FirstName, i.LastName)
        & n.ChangeBirthDateInternal(c, i.BirthDate));
    // 1 chamada a RegisterChangeInternal, versão +1
}
```

### Antipadrão 3: Lógica Condicional Incorreta

```csharp
// ❌ ERRADO - Múltiplas chamadas baseadas em condição
public SimpleAggregateRoot? Update(ExecutionContext ctx, UpdateInput input)
{
    var result = RegisterChangeInternal(ctx, this, input, handler: (c, i, n) =>
        n.ChangeNameInternal(c, i.FirstName, i.LastName));

    if (result == null) return null;

    // Problema: Segunda chamada a RegisterChangeInternal
    if (input.ShouldUpdateAddress)
    {
        result = result.RegisterChangeInternal(ctx, result, input, handler: (c, i, n) =>
            n.ChangeAddressInternal(c, i.Address));
    }

    return result;
}

// ✅ CORRETO - Condicional DENTRO do handler, handler static
public SimpleAggregateRoot? Update(ExecutionContext ctx, UpdateInput input)
{
    return RegisterChangeInternal(ctx, this, input, handler: static (c, i, n) =>
    {
        bool success = n.ChangeNameInternal(c, i.FirstName, i.LastName);

        // Condicional dentro da mesma chamada
        if (i.ShouldUpdateAddress)
            success &= n.ChangeAddressInternal(c, i.Address);

        return success;
    });
}
```

### Antipadrão 4: Usar && Ao Invés de &

```csharp
// ❌ ERRADO - && para short-circuit (usuário vê 1 erro por vez)
handler: static (ctx, inp, newInstance) =>
{
    return
        newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
        && newInstance.ChangeBirthDateInternal(ctx, inp.BirthDate)
        && newInstance.ChangeAddressInternal(ctx, inp.Address);
    // Se ChangeNameInternal falhar, os outros NUNCA executam
}

// ✅ CORRETO - & para validação completa (usuário vê TODOS os erros)
handler: static (ctx, inp, newInstance) =>
{
    return
        newInstance.ChangeNameInternal(ctx, inp.FirstName, inp.LastName)
        & newInstance.ChangeBirthDateInternal(ctx, inp.BirthDate)
        & newInstance.ChangeAddressInternal(ctx, inp.Address);
    // TODAS as validações executam, todas as mensagens coletadas
}
```

### Antipadrão 5: Handler Sem Static

```csharp
// ❌ ERRADO - Handler sem static pode capturar variáveis acidentalmente
public SimpleAggregateRoot? Update(ExecutionContext ctx, UpdateInput input)
{
    var valorExterno = ObterValorExterno();

    return RegisterChangeInternal(ctx, this, input, handler: (c, i, n) =>
    {
        // valorExterno capturado acidentalmente - cria closure!
        Console.WriteLine(valorExterno);
        return n.ChangeNameInternal(c, i.FirstName, i.LastName);
    });
}

// ✅ CORRETO - Handler static previne closures acidentais
public SimpleAggregateRoot? Update(ExecutionContext ctx, UpdateInput input)
{
    return RegisterChangeInternal(ctx, this, input, handler: static (c, i, n) =>
    {
        // Se tentar usar variável externa, NÃO compila
        return n.ChangeNameInternal(c, i.FirstName, i.LastName);
    });
}
```

## Decisões Relacionadas

- [DE-006](./DE-006-operador-bitwise-and-para-validacao-completa.md) - Operador & para validação completa
- [DE-019](./DE-019-input-objects-pattern.md) - Input Objects para métodos públicos
- [DE-020](./DE-020-dois-construtores-privados.md) - Construtores privados
- [DE-021](./DE-021-metodos-publicos-vs-metodos-internos.md) - Métodos públicos vs métodos internos
- [DE-022](./DE-022-metodos-set-privados.md) - Métodos Set* privados

## Leitura Recomendada

- [Domain-Driven Design - Eric Evans](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)
- [Implementing Domain-Driven Design - Vaughn Vernon](https://www.amazon.com/Implementing-Domain-Driven-Design-Vaughn-Vernon/dp/0321834577)
- [Clean Code - Robert C. Martin](https://blog.cleancoder.com/)
- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Command Pattern - GoF](https://refactoring.guru/design-patterns/command)
- [Transaction Script Pattern - Martin Fowler](https://martinfowler.com/eaaCatalog/transactionScript.html)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Fornece RegisterNewInternal e RegisterChangeInternal que devem ser chamados uma única vez por método público |

## Referências no Código

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - RegisterNew - exemplo de uma única chamada a RegisterNewInternal
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - ChangeName - exemplo de uma única chamada a RegisterChangeInternal
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - LLM_RULE: Register*Internal Chamado UMA ÚNICA VEZ
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - ChangeNameInternal - uso do operador & para combinar múltiplos Set*
