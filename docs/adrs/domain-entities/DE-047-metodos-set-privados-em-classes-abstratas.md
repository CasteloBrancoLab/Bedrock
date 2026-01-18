# DE-047: Métodos Set* Privados em Classes Abstratas

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma fábrica de automóveis onde a matriz (classe abstrata) define o processo de montagem do motor. A matriz tem procedimentos internos rigorosos: primeiro instala os pistões, depois as válvulas, depois faz a calibração - tudo em sequência específica.

Agora imagine que a filial (classe derivada) recebe acesso direto a cada procedimento individual. Um gerente da filial decide "otimizar" e pula a calibração, ou instala válvulas antes dos pistões. O motor sai da fábrica, mas falha em campo.

O problema: a matriz confiou que a filial seguiria o processo completo, mas deu ferramentas que permitiam fazer diferente.

### O Problema Técnico

Em hierarquias de herança, a abordagem comum é deixar métodos `Set*` como `protected` para que classes derivadas possam "setar" propriedades da classe base:

```csharp
// ❌ Abordagem comum - Set* protegido
public abstract class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string FullName { get; private set; }  // Derivado de FirstName + LastName

    protected bool SetFirstName(ExecutionContext ctx, string value)
    {
        FirstName = value;
        return true;
    }

    protected bool SetLastName(ExecutionContext ctx, string value)
    {
        LastName = value;
        FullName = $"{FirstName} {LastName}";
        return true;
    }
}
```

O raciocínio parece lógico: "a filha precisa setar as propriedades da pai, então vou deixar protected".

Mas isso ignora que a classe abstrata tem **invariantes próprias** que precisam ser protegidas.

## Como Normalmente É Feito

### Abordagem Tradicional

A maioria dos projetos trata a classe abstrata como um "container de propriedades compartilhadas":

```csharp
// Abordagem comum - classe abstrata como container
public abstract class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string FullName { get; private set; }

    // Set* protegidos - filha pode chamar individualmente
    protected bool SetFirstName(ExecutionContext ctx, string value) { ... }
    protected bool SetLastName(ExecutionContext ctx, string value) { ... }
}

public sealed class Employee : Person
{
    public static Employee? RegisterNew(ExecutionContext ctx, string firstName, string lastName)
    {
        var instance = new Employee();

        // Desenvolvedor pode chamar na ordem que quiser
        // Ou esquecer de chamar algum
        instance.SetLastName(ctx, lastName);
        // Esqueceu SetFirstName!

        return instance;
    }
}
```

### Por Que Não Funciona Bem

**1. Estado Inválido Silencioso**

```csharp
public sealed class Employee : Person
{
    public static Employee? RegisterNew(ExecutionContext ctx, string firstName, string lastName)
    {
        var instance = new Employee();

        // Desenvolvedor esquece SetFirstName, chama apenas SetLastName
        instance.SetLastName(ctx, lastName);  // FullName = " Silva" (FirstName vazio!)

        return instance;  // 💥 ESTADO INVÁLIDO: FullName inconsistente
    }
}
```

**2. Violação do Liskov Substitution Principle (LSP)**

LSP diz que subclasses devem ser substituíveis pela classe base sem quebrar comportamento:

```csharp
void ProcessPerson(Person person)
{
    // Código assume que FullName é consistente com FirstName + LastName
    var parts = person.FullName.Split(' ');
    var firstName = parts[0];  // 💥 Se FullName = " Silva", firstName = ""
}

// Employee com FullName inconsistente VIOLA LSP
// Não pode ser usada onde Person é esperada sem quebrar o sistema
```

**3. Confiança Implícita na Filha**

A abordagem assume que o desenvolvedor da classe filha vai:
- Chamar todos os setters necessários
- Chamar na ordem correta
- Nunca esquecer nenhum

Isso é "proteção por documentação", não por design.

**4. Testes Não Pegam**

O código compila, os testes unitários da filha passam (porque testam o caminho feliz), mas a invariante está quebrada. O bug só aparece em produção.

## A Decisão

### Nossa Abordagem

Métodos `Set*` são **PRIVADOS** na classe abstrata. Classes filhas acessam o estado da pai através de métodos `*Internal` **PROTEGIDOS** que representam operações de negócio completas:

```csharp
public abstract class Person
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string FullName { get; private set; }

    // *Internal PROTEGIDO - operação de negócio completa
    protected bool ChangeNameInternal(ExecutionContext ctx, string firstName, string lastName)
    {
        // Classe PAI garante que FirstName, LastName e FullName são SEMPRE atualizados juntos
        return SetFirstName(ctx, firstName)
            & SetLastName(ctx, lastName)
            & SetFullName(ctx, $"{firstName} {lastName}");
    }

    // Set* PRIVADOS - inacessíveis à filha
    private bool SetFirstName(ExecutionContext ctx, string value) { ... }
    private bool SetLastName(ExecutionContext ctx, string value) { ... }
    private bool SetFullName(ExecutionContext ctx, string value) { ... }
}

public sealed class Employee : Person
{
    public static Employee? RegisterNew(ExecutionContext ctx, string firstName, string lastName)
    {
        var instance = new Employee();

        // Única opção: chamar ChangeNameInternal que garante consistência
        instance.ChangeNameInternal(ctx, firstName, lastName);

        return instance;  // ✅ FullName SEMPRE consistente
    }
}
```

### Por Que Funciona Melhor

1. **Invariantes Garantidas**: A classe pai define COMO seu estado pode ser alterado
2. **LSP por Design**: Qualquer instância de Employee respeita as invariantes de Person
3. **Impossível Errar**: Não existe caminho de código que permita estado inconsistente
4. **Compilador como Guardião**: Erros são detectados em compile-time, não em runtime

## Consequências

### Benefícios

- **Encapsulamento Real**: Classe abstrata controla seu próprio estado
- **LSP Garantido**: Subclasses são sempre substituíveis pela classe base
- **Bugs Impossíveis**: Não há como esquecer de atualizar propriedades relacionadas
- **Manutenibilidade**: Mudanças na lógica de atualização são feitas em um único lugar

### Trade-offs

- **Mais Métodos *Internal**: Cada operação de negócio precisa de um método protegido
- **Menos Flexibilidade**: Classe filha não pode "customizar" como propriedades são setadas

### Trade-offs Frequentemente Superestimados

**"Preciso de flexibilidade para setar propriedades individualmente"**

Se você precisa setar propriedades individualmente, provavelmente elas não têm invariantes entre si. Nesse caso, podem ter métodos `*Internal` separados:

```csharp
// Se Email e Phone são independentes
protected bool ChangeEmailInternal(ExecutionContext ctx, string email) { ... }
protected bool ChangePhoneInternal(ExecutionContext ctx, string phone) { ... }

// Se FirstName e LastName são interdependentes (via FullName)
protected bool ChangeNameInternal(ExecutionContext ctx, string first, string last) { ... }
```

A granularidade do `*Internal` deve refletir as **operações de negócio**, não as propriedades individuais.

## Fundamentação Teórica

### SOLID - Liskov Substitution Principle

Barbara Liskov definiu:

> "Objects of a superclass should be replaceable with objects of a subclass without affecting the correctness of the program."
>
> *Objetos de uma superclasse devem ser substituíveis por objetos de uma subclasse sem afetar a corretude do programa.*

Com `Set*` protegido, subclasses podem criar estados que violam invariantes da superclasse, quebrando LSP.

Com `*Internal` protegido, a superclasse mantém controle total sobre suas invariantes, garantindo LSP por construção.

### Encapsulamento em Hierarquias

O princípio de encapsulamento não se aplica apenas a classes externas, mas também a classes derivadas. A classe pai é responsável por manter seu próprio estado consistente - dar acesso granular a `Set*` quebra essa responsabilidade.

### Design by Contract

Bertrand Meyer propôs que classes definem **contratos** (pré-condições, pós-condições, invariantes). Invariantes devem ser verdadeiras antes e depois de qualquer operação pública.

`*Internal` protegido permite que a classe pai mantenha suas invariantes. `Set*` protegido permite que a filha quebre o contrato.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Como o Liskov Substitution Principle se aplica a classes abstratas?"
- "Qual a diferença entre encapsulamento para classes externas vs derivadas?"
- "Como garantir invariantes em hierarquias de herança?"
- "Por que 'protected' não é o mesmo que 'encapsulado'?"

### Leitura Recomendada

- [SOLID Principles - Liskov Substitution](https://en.wikipedia.org/wiki/Liskov_substitution_principle)
- [Effective Java - Item 18: Favor composition over inheritance](https://www.oreilly.com/library/view/effective-java/9780134686097/)
- [Design by Contract - Bertrand Meyer](https://en.wikipedia.org/wiki/Design_by_contract)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Classe base que fornece infraestrutura para o padrão de métodos privados |

## Referências no Código

- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - comentário LLM_GUIDANCE sobre métodos Set* em classes abstratas
- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - comentário LLM_ANTIPATTERN sobre Set* protegido
- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - comentário LLM_RULE sobre garantia do LSP
