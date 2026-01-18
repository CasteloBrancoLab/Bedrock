# DE-054: Herança vs Composição em Entidades de Domínio

## Status
Aceita

## Contexto

### O Problema (Analogia Detalhada)

Imagine que você está modelando **animais** em um sistema.

**Cenário 1: Cachorro e Gato**

Um Cachorro **É UM** Animal. Um Gato **É UM** Animal.

Ambos compartilham características fundamentais de Animal: nascem, respiram, morrem, têm idade. As regras de "ter idade" são as mesmas para cachorros e gatos - a idade é calculada da mesma forma, validada da mesma forma.

```
Animal (classe abstrata)
├── Cachorro (classe filha) - É UM Animal
└── Gato (classe filha) - É UM Animal
```

Isso é **HERANÇA** - relacionamento "É UM" (is-a).

**Cenário 2: Carro e Motor**

Um Carro **TEM UM** Motor. O Carro NÃO É UM Motor.

O Motor tem suas próprias regras (cilindrada, potência, consumo). O Carro usa o Motor, mas não herda dele. O Carro pode trocar de Motor. Diferentes carros podem ter motores com regras completamente diferentes.

```
Carro
└── Motor (propriedade) - TEM UM Motor

Moto
└── Motor (propriedade) - TEM UM Motor (regras diferentes!)
```

Isso é **COMPOSIÇÃO** - relacionamento "TEM UM" (has-a).

### O Problema Técnico

O erro mais comum é usar herança quando deveria usar composição:

```csharp
// ❌ ERRADO - Usando herança para "TEM UM"
public abstract class Person
{
    public string FirstName { get; private set; }  // Max 100 chars
    public string LastName { get; private set; }   // Max 100 chars
}

public sealed class Employee : Person
{
    // Employee quer FirstName com Max 50 chars (regra diferente)
    // MAS não pode! Herda as regras de Person.
}
```

Se Employee precisa de regras DIFERENTES para FirstName, então Employee NÃO É UMA Person no sentido de herança. Employee TEM informações de pessoa, mas com regras próprias.

## O Teste "É UM" (Liskov Substitution)

### A Pergunta Fundamental

Antes de usar herança, faça a pergunta:

> "Uma instância da classe filha pode ser usada em QUALQUER lugar onde a classe pai é esperada, SEM quebrar comportamento?"

Se a resposta for **SIM** → Use Herança
Se a resposta for **NÃO** → Use Composição

### Exemplos Práticos

**Exemplo 1: Employee É UMA Person? (Herança)**

```csharp
void ProcessPerson(Person person)
{
    // Assume que FirstName tem no máximo 100 caracteres
    var displayName = person.FirstName.Substring(0, Math.Min(person.FirstName.Length, 100));
    // ...
}

// Se Employee É UMA Person, isso deve funcionar perfeitamente:
var employee = new Employee(...);
ProcessPerson(employee);  // ✅ Funciona - Employee segue as mesmas regras de Person
```

Se Employee segue TODAS as regras de Person (incluindo FirstName max 100), então Employee É UMA Person. Use herança.

**Exemplo 2: Documento Fiscal Brasileiro vs Internacional**

Imagine um sistema que lida com documentos fiscais. Um documento brasileiro tem CPF/CNPJ,
um documento internacional tem Tax ID com formato completamente diferente.

```csharp
// ❌ TENTATIVA COM HERANÇA - NÃO FUNCIONA
public abstract class FiscalDocument
{
    public string TaxIdentifier { get; private set; }  // CPF: 11 dígitos, CNPJ: 14 dígitos

    public static class FiscalDocumentMetadata
    {
        public static int TaxIdentifierMinLength { get; private set; } = 11;
        public static int TaxIdentifierMaxLength { get; private set; } = 14;
        public static string TaxIdentifierPattern { get; private set; } = @"^\d{11,14}$";
    }
}

public sealed class InternationalFiscalDocument : FiscalDocument
{
    // Tax ID internacional: "US-123-456-789" ou "DE-ABC-123"
    // Formato COMPLETAMENTE diferente - letras, hífens, tamanhos variados
    //
    // ❌ NÃO PODE mudar TaxIdentifierPattern da classe pai
    // ❌ NÃO PODE mudar TaxIdentifierMinLength/MaxLength
    // ❌ Herança FORÇA regras brasileiras em documento internacional!
}
```

**Por que método abstrato NÃO resolve?**

```csharp
public abstract class FiscalDocument
{
    public string TaxIdentifier { get; private set; }

    // Mesmo com método abstrato para validação...
    protected abstract bool ValidateTaxIdentifierFormat(string taxId);

    // ...os METADADOS ainda são da classe pai!
    // UI, API, banco de dados usam os metadados para:
    // - MaxLength de campos de input
    // - Tamanho de colunas no banco
    // - Validação em camadas externas ANTES de chegar na entidade

    public static class FiscalDocumentMetadata
    {
        // 💥 Estes valores são usados por TODA a aplicação
        // Se InternationalFiscalDocument precisa de valores diferentes,
        // herança não funciona!
        public static int TaxIdentifierMaxLength { get; private set; } = 14;
    }
}
```

O problema não é apenas a validação (que poderia ser abstrata), mas os **METADADOS** que são
usados por toda a aplicação: UI, API, banco de dados, relatórios.

Se a classe filha precisa de metadados diferentes, herança quebra porque:
- Metadados são estáticos e compartilhados
- Alterar metadados na filha afeta TODAS as instâncias
- Não existe "metadado por instância" - é da CLASSE

## Como Normalmente É Feito (Errado)

### Abordagem Tradicional

Muitos programadores forçam herança onde não deveria:

**1. Override de Validação (Violação de LSP)**

```csharp
public abstract class FiscalDocument
{
    public virtual string TaxIdentifierPattern => @"^\d{11,14}$";  // Apenas dígitos

    public virtual bool ValidateTaxIdentifier(string taxId)
    {
        return Regex.IsMatch(taxId, TaxIdentifierPattern);
    }
}

public sealed class InternationalFiscalDocument : FiscalDocument
{
    // "Sobrescreve" para aceitar letras e hífens
    public override string TaxIdentifierPattern => @"^[A-Z]{2}-[\w-]+$";

    public override bool ValidateTaxIdentifier(string taxId)
    {
        return Regex.IsMatch(taxId, TaxIdentifierPattern);  // Regra diferente!
    }
}
```

**Por que é errado?**

```csharp
void ProcessDocument(FiscalDocument doc)
{
    // Código assume que TaxIdentifier são apenas dígitos
    var numericId = long.Parse(doc.TaxIdentifier);  // 💥 EXPLODE com "US-123-ABC"!
}
```

Código que espera FiscalDocument assume formato brasileiro. InternationalFiscalDocument quebra essa expectativa.

**2. Flags Para Diferenciar Comportamento**

```csharp
public class FiscalDocument
{
    public bool IsInternational { get; set; }

    public bool ValidateTaxIdentifier(string taxId)
    {
        if (IsInternational)
            return Regex.IsMatch(taxId, @"^[A-Z]{2}-[\w-]+$");
        else
            return Regex.IsMatch(taxId, @"^\d{11,14}$");
    }

    public int GetMaxTaxIdLength()
    {
        return IsInternational ? 50 : 14;  // 💥 Lógica condicional baseada em tipo
    }
}
```

**Por que é errado?** Viola Open/Closed Principle. Cada novo país adiciona mais ifs. Metadados ficam espalhados em lógica condicional.

**3. Herança Profunda**

```csharp
public abstract class Document { }
public abstract class FiscalDocument : Document { }
public abstract class BrazilianFiscalDocument : FiscalDocument { }
public abstract class BrazilianInvoice : BrazilianFiscalDocument { }
public sealed class BrazilianServiceInvoice : BrazilianInvoice { }  // 💥 5 níveis!
```

**Por que é errado?** Hierarquias profundas são frágeis. Mudança em Document afeta tudo. Impossível testar isoladamente.

## A Decisão

### Nossa Abordagem

**Use HERANÇA quando:**
- A classe filha É UMA versão especializada da pai
- Todas as regras da pai se aplicam à filha SEM MODIFICAÇÃO
- A filha pode ser substituída pela pai em qualquer contexto (LSP)
- A filha ADICIONA comportamento, não MODIFICA comportamento da pai

**Use COMPOSIÇÃO quando:**
- A classe TEM características de outra, mas com regras diferentes
- Você quer MODIFICAR regras da classe "pai"
- Os objetos têm ciclos de vida independentes
- Você precisa de flexibilidade para trocar implementações

### Exemplo Correto: Herança

```csharp
// ✅ HERANÇA CORRETA - Employee É UMA Person
public abstract class Person : EntityBase<Person>
{
    public string FirstName { get; private set; }  // Max 100, obrigatório
    public string LastName { get; private set; }   // Max 100, obrigatório

    public static class PersonMetadata
    {
        public static int FirstNameMaxLength { get; private set; } = 100;
        public static int LastNameMaxLength { get; private set; } = 100;
    }

    // Regras de validação de Person
    public static bool ValidateFirstName(ExecutionContext ctx, string? firstName) { ... }
}

public sealed class Employee : Person
{
    public string EmployeeNumber { get; private set; }  // Propriedade ADICIONAL
    public Department Department { get; private set; }   // Propriedade ADICIONAL

    // Employee ADICIONA validações, não MODIFICA as de Person
    public static bool ValidateEmployeeNumber(ExecutionContext ctx, string? empNumber) { ... }

    // Employee pode ser usado onde Person é esperada
    // Todas as regras de Person se aplicam a Employee
}
```

**Por que funciona?**
- Employee não tenta mudar FirstNameMaxLength
- Employee ADICIONA EmployeeNumber, não modifica FirstName
- Qualquer código que espera Person funciona com Employee

### Exemplo Correto: Composição

```csharp
// ✅ COMPOSIÇÃO CORRETA - Diferentes tipos de documentos com regras diferentes
public sealed class BrazilianFiscalDocument : EntityBase<BrazilianFiscalDocument>, IAggregateRoot
{
    public TaxIdentifierBrazil TaxIdentifier { get; private set; }  // TEM identificador brasileiro
    public DocumentInfo DocumentInfo { get; private set; }  // TEM informações comuns

    public static class BrazilianFiscalDocumentMetadata
    {
        // Metadados específicos para documento brasileiro
        public static int TaxIdentifierMinLength { get; private set; } = 11;
        public static int TaxIdentifierMaxLength { get; private set; } = 14;
        public static string TaxIdentifierPattern { get; private set; } = @"^\d{11,14}$";
    }
}

public sealed class InternationalFiscalDocument : EntityBase<InternationalFiscalDocument>, IAggregateRoot
{
    public TaxIdentifierInternational TaxIdentifier { get; private set; }  // TEM identificador internacional
    public DocumentInfo DocumentInfo { get; private set; }  // TEM informações comuns

    public static class InternationalFiscalDocumentMetadata
    {
        // Metadados COMPLETAMENTE diferentes - sem conflito!
        public static int TaxIdentifierMinLength { get; private set; } = 5;
        public static int TaxIdentifierMaxLength { get; private set; } = 50;
        public static string TaxIdentifierPattern { get; private set; } = @"^[A-Z]{2}-[\w-]+$";
    }
}

// Value Objects para identificadores específicos
public readonly record struct TaxIdentifierBrazil(string Value);  // CPF ou CNPJ
public readonly record struct TaxIdentifierInternational(string CountryCode, string Value);

// Value Object para informações comuns (reutilizável via COMPOSIÇÃO)
public readonly record struct DocumentInfo(
    string Description,
    DateOnly IssueDate,
    decimal TotalAmount
);
```

**Por que funciona?**
- BrazilianFiscalDocument e InternationalFiscalDocument são entidades INDEPENDENTES
- Cada uma tem seus PRÓPRIOS metadados (MaxLength, Pattern, etc.)
- Ambas USAM DocumentInfo via composição, mas não herdam dele
- Não há expectativa de substituição (não são polimórficos)
- UI/API/Banco usam os metadados CORRETOS para cada tipo

### Diagrama de Decisão

```
┌─────────────────────────────────────────────────────────────────┐
│                    PERGUNTA INICIAL                             │
│                                                                 │
│  "A classe filha precisa de regras DIFERENTES da classe pai?"  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
                              │
              ┌───────────────┴───────────────┐
              │                               │
              ▼                               ▼
       ┌──────────┐                    ┌──────────┐
       │   NÃO    │                    │   SIM    │
       └──────────┘                    └──────────┘
              │                               │
              ▼                               ▼
┌─────────────────────────┐    ┌─────────────────────────┐
│      Use HERANÇA        │    │    Use COMPOSIÇÃO       │
│                         │    │                         │
│ - Filha É UMA pai       │    │ - Classe TEM outra      │
│ - Regras idênticas      │    │ - Regras diferentes     │
│ - Adiciona, não modifica│    │ - Independentes         │
│ - Pode substituir (LSP) │    │ - Flexibilidade         │
└─────────────────────────┘    └─────────────────────────┘
```

### Por Que a Classe Pai Controla Seu Estado

Quando você usa herança, a classe pai é a AUTORIDADE sobre suas propriedades:

```csharp
public abstract class Person
{
    // Person define AS REGRAS de FirstName
    public static class PersonMetadata
    {
        public static int FirstNameMaxLength { get; private set; } = 100;
    }

    // Person define COMO FirstName é validado
    public static bool ValidateFirstName(ExecutionContext ctx, string? firstName)
    {
        return ValidationUtils.ValidateMaxLength(ctx, ..., PersonMetadata.FirstNameMaxLength, ...);
    }

    // Person define COMO FirstName é atribuído
    private bool SetFirstName(ExecutionContext ctx, string firstName)
    {
        if (!ValidateFirstName(ctx, firstName))
            return false;
        FirstName = firstName;
        return true;
    }
}
```

**Se a filha pudesse alterar FirstNameMaxLength:**

```csharp
public sealed class Employee : Person
{
    static Employee()
    {
        // ❌ ERRADO - Filha tentando alterar regra da pai
        PersonMetadata.ChangeFirstNameMaxLength(50);
    }
}

public sealed class Customer : Person
{
    static Customer()
    {
        // ❌ ERRADO - Outra filha com regra diferente
        PersonMetadata.ChangeFirstNameMaxLength(200);
    }
}

// 💥 PROBLEMA: Qual é o FirstNameMaxLength de Person?
// Depende de qual filha foi carregada primeiro!
// Comportamento imprevisível e bugs impossíveis de rastrear.
```

**Regra:** Se você precisa alterar regras da classe pai na classe filha, você NÃO deveria estar usando herança.

## Consequências

### Benefícios

- **Clareza Conceitual**: Herança para "É UM", Composição para "TEM UM"
- **LSP Garantido**: Classes filhas sempre substituíveis pela pai
- **Estado Consistente**: Classe pai é autoridade absoluta sobre suas propriedades
- **Flexibilidade**: Composição permite regras diferentes sem hierarquia

### Trade-offs

- **Menos Reutilização Aparente**: Composição pode parecer "repetir código"
- **Mais Objetos**: Value Objects e objetos compostos em vez de herança profunda
- **Decisão Antecipada**: Precisa pensar no relacionamento antes de codificar

### Quando Refatorar de Herança Para Composição

Se você encontrar:
- Override de métodos de validação da classe pai
- Flags como `IsPremium`, `IsSpecial` para diferenciar comportamento
- Hierarquias com mais de 2 níveis
- Necessidade de "desligar" comportamento herdado
- Classes filhas que "não deveriam" herdar certos métodos

## Fundamentação Teórica

### Liskov Substitution Principle (LSP)

Barbara Liskov, 1987:

> "Se S é um subtipo de T, então objetos do tipo T podem ser substituídos por objetos do tipo S sem alterar as propriedades desejáveis do programa."

Em termos práticos: se Employee herda de Person, qualquer código que funciona com Person DEVE funcionar com Employee sem surpresas.

### Composition Over Inheritance (GoF, 1994)

O livro "Design Patterns" do Gang of Four recomenda:

> "Favor object composition over class inheritance."

Razões:
- Herança é definida em compile-time, composição em runtime
- Herança expõe detalhes internos da pai (white-box reuse)
- Composição permite trocar comportamento dinamicamente (black-box reuse)

### Tell, Don't Ask

Composição promove encapsulamento:

```csharp
// Composição - Tell
customer.CreditPolicy.ApproveCredit(amount);  // Delega para o objeto composto

// Herança - Ask (tende a violar encapsulamento)
if (customer is PremiumCustomer)
    // Lógica específica aqui
```

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Como identificar se devo usar herança ou composição em um caso específico?"
- "Quais são os code smells que indicam uso errado de herança?"
- "Como refatorar uma hierarquia de herança profunda para composição?"
- "Qual a relação entre Liskov Substitution Principle e herança?"

### Leitura Recomendada

- [Liskov Substitution Principle - Wikipedia](https://en.wikipedia.org/wiki/Liskov_substitution_principle)
- [Composition over Inheritance - Wikipedia](https://en.wikipedia.org/wiki/Composition_over_inheritance)
- Design Patterns: Elements of Reusable Object-Oriented Software (GoF)
- Effective Java, Item 18: Favor composition over inheritance

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| [EntityBase](../../building-blocks/domain-entities/entity-base.md) | Classe base que demonstra herança correta |
| Value Objects | Usados em composição como PersonInfo, CreditPolicy |

## Referências no Código

- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs) - exemplo de herança correta onde filhas não modificam regras da pai
- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs) - exemplo de classe concreta sealed
- ADR DE-047: Métodos Set* Privados - por que filhas não podem alterar estado da pai diretamente
- ADR DE-053: Metadados de Validação - classe pai define seus próprios metadados
