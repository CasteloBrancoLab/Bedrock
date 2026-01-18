# 🎂 BirthDate - Representação Tipada de Data de Nascimento

A estrutura `BirthDate` encapsula uma data de nascimento com cálculo preciso de idade, eliminando erros comuns em operações com datas. Fornece uma abstração imutável e type-safe para representar e manipular datas de nascimento em sistemas de domínio.

> 💡 **Visão Geral:** Estrutura imutável de **16 bytes** para datas de nascimento, com cálculo de idade **preciso** considerando dia e mês — perfeita para sistemas que precisam de validação de idade ou regras de negócio baseadas em data de nascimento.

---

## 📋 Sumário

- [Contexto: Por Que Existe](#-contexto-por-que-existe)
- [Problemas Resolvidos](#-problemas-resolvidos)
  - [Cálculo Incorreto de Idade](#1--cálculo-incorreto-de-idade)
  - [Falta de Semântica no Tipo](#2-️-falta-de-semântica-no-tipo)
- [Funcionalidades](#-funcionalidades)
- [Como Usar](#-como-usar)
- [Trade-offs](#️-tradeoffs)
- [Exemplos Avançados](#-exemplos-avançados)
- [Referências](#-referências)

---

## 🎯 Contexto: Por Que Existe

### O Problema Real

Em muitas aplicações, a data de nascimento é tratada como um simples `DateTime` ou `DateTimeOffset`, o que leva a erros frequentes no cálculo de idade e falta de semântica no código. As abordagens tradicionais apresentam problemas sérios:

**Exemplo de desafios comuns:**

```csharp
❌ Abordagem 1: Usar DateTime/DateTimeOffset diretamente
public class Customer
{
    public DateTimeOffset BirthDate { get; set; }  // ⚠️ Sem semântica específica

    public int GetAge()
    {
        // ⚠️ Cálculo ingênuo - ERRADO!
        return DateTime.Now.Year - BirthDate.Year;
    }
}

// Exemplo do bug:
var customer = new Customer
{
    BirthDate = new DateTimeOffset(2000, 12, 31, 0, 0, 0, TimeSpan.Zero)
};

// Se hoje for 1º de janeiro de 2024:
var age = customer.GetAge();  // Retorna 24, mas a pessoa ainda tem 23!

❌ Problemas:
- Cálculo de idade não considera mês e dia
- Pessoa nascida em 31/dez seria considerada 1 ano mais velha em 1º/jan
- Erros em validações de maioridade
- Uso de DateTime.Now dificulta testes
```

```csharp
❌ Abordagem 2: Cálculo de idade espalhado pelo código
public class AgeValidator
{
    public bool IsAdult(DateTimeOffset birthDate)
    {
        var today = DateTime.UtcNow;
        int age = today.Year - birthDate.Year;

        // ⚠️ Tentativa de correção, mas ainda incorreta
        if (today.Month < birthDate.Month)
            age--;

        return age >= 18;
    }
}

public class InsuranceCalculator
{
    public decimal CalculatePremium(DateTimeOffset birthDate)
    {
        var today = DateTime.UtcNow;
        int age = today.Year - birthDate.Year;

        // ⚠️ Cálculo diferente do AgeValidator!
        if (today < birthDate.AddYears(age))
            age--;

        return age > 60 ? 500m : 200m;
    }
}

❌ Problemas:
- Lógica de cálculo de idade duplicada
- Implementações diferentes em cada lugar
- Inconsistência entre validadores
- Difícil de testar (usa DateTime.UtcNow)
- Sem proteção contra uso incorreto
```

### A Solução

O `BirthDate` implementa uma estrutura **imutável** com cálculo de idade **preciso** e suporte a **TimeProvider** para testabilidade.

```csharp
✅ Abordagem com BirthDate:
public class Customer
{
    public BirthDate BirthDate { get; private set; }  // ✨ Tipo semântico

    public int GetAge(TimeProvider timeProvider)
    {
        // ✨ Cálculo preciso e centralizado
        return BirthDate.CalculateAgeInYears(timeProvider);
    }

    public bool IsAdult(TimeProvider timeProvider)
    {
        return GetAge(timeProvider) >= 18;  // ✅ Usa o mesmo cálculo
    }
}

// Exemplo correto:
var customer = new Customer
{
    BirthDate = BirthDate.CreateNew(new DateTimeOffset(2000, 12, 31, 0, 0, 0, TimeSpan.Zero))
};

// Se hoje for 1º de janeiro de 2024:
var age = customer.GetAge(TimeProvider.System);  // Retorna 23 ✅ CORRETO!

// Se hoje for 31 de dezembro de 2024:
var age2 = customer.GetAge(TimeProvider.System);  // Retorna 24 ✅ CORRETO!

✅ Benefícios:
- Cálculo de idade preciso (considera mês E dia)
- Lógica centralizada em um único lugar
- Testável via TimeProvider
- Tipo semântico expressa intenção
- Imutável e thread-safe
- Conversão implícita de/para DateTimeOffset
```

**Estrutura do BirthDate:**

```
┌──────────────────────────────────────────────────────────────────────────┐
│                     ESTRUTURA DO BIRTHDATE                               │
└──────────────────────────────────────────────────────────────────────────┘
│                                                                           │
│   readonly struct BirthDate : IEquatable<BirthDate>                      │
│   └── Value: DateTimeOffset (16 bytes) → Data de nascimento armazenada  │
│                                                                           │
│   Características:                                                        │
│   ├── Imutável (readonly struct)                                         │
│   ├── Value type (alocado na stack)                                      │
│   ├── Implementa IEquatable<T> para comparação eficiente                 │
│   ├── Factory methods (CreateNew) para criação controlada                │
│   ├── Conversão implícita de/para DateTimeOffset                         │
│   └── Operadores de comparação (<, >, <=, >=, ==, !=)                    │
│                                                                           │
│   Tamanho em memória: 16 bytes (mesmo que DateTimeOffset)                │
│                                                                           │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## 🔧 Problemas Resolvidos

### 1. 🔢 Cálculo Incorreto de Idade

**Problema:** Subtrair anos diretamente não considera se o aniversário já ocorreu no ano atual.

#### 📚 Analogia: O Aniversário de Janeiro

Imagine duas crianças nascidas em anos diferentes:

**❌ Com cálculo simples (ano atual - ano de nascimento):**

```
Data: 15 de Janeiro de 2024

Criança A: Nasceu em 1º de Janeiro de 2020
  → 2024 - 2020 = 4 anos ✅ Correto (já fez aniversário)

Criança B: Nasceu em 31 de Dezembro de 2020
  → 2024 - 2020 = 4 anos ❌ ERRADO! (ainda não fez aniversário)
  → Idade real: 3 anos

⚠️ PROBLEMA: O sistema diz que têm a mesma idade,
   mas a Criança B ainda vai fazer 4 anos em dezembro!
```

**✅ Com BirthDate.CalculateAgeInYears():**

```
Data: 15 de Janeiro de 2024

Criança A: Nasceu em 1º de Janeiro de 2020
  → CalculateAgeInYears() = 4 anos ✅
  → Aniversário em Janeiro? ✓ Dia 1 já passou? ✓

Criança B: Nasceu em 31 de Dezembro de 2020
  → CalculateAgeInYears() = 3 anos ✅
  → Aniversário em Dezembro? Ainda não chegou!

✅ CORRETO: Sistema considera corretamente o mês E o dia
```

#### 💻 Impacto Real no Código

**❌ Código com cálculo manual:**

```csharp
public class AgeVerificationService
{
    public bool CanBuyAlcohol(DateTimeOffset birthDate)
    {
        // ⚠️ Cálculo ingênuo
        int age = DateTime.UtcNow.Year - birthDate.Year;
        return age >= 21;
    }

    public bool CanVote(DateTimeOffset birthDate)
    {
        // ⚠️ Tentativa de correção, mas incompleta
        var today = DateTime.UtcNow;
        int age = today.Year - birthDate.Year;
        if (today.Month < birthDate.Month)
            age--;
        return age >= 16;
    }

    public bool CanDrive(DateTimeOffset birthDate)
    {
        // ⚠️ Outra implementação diferente!
        var today = DateTime.UtcNow;
        int age = today.Year - birthDate.Year;
        if (today.DayOfYear < birthDate.DayOfYear)  // ⚠️ Bug em anos bissextos!
            age--;
        return age >= 18;
    }
}

// Problema real:
var birthDate = new DateTimeOffset(2003, 3, 15, 0, 0, 0, TimeSpan.Zero);
var today = new DateTimeOffset(2024, 3, 10, 0, 0, 0, TimeSpan.Zero);

// Pessoa nasceu em 15/Mar/2003, hoje é 10/Mar/2024
// Idade REAL: 20 anos (ainda não fez 21)

var service = new AgeVerificationService();
service.CanBuyAlcohol(birthDate);  // Retorna TRUE! ❌ BUG!
// Permite compra de álcool para menor de 21

❌ Problemas:
- 3 implementações diferentes de cálculo de idade
- Nenhuma está 100% correta
- Bug de ano bissexto no método CanDrive
- Impossível testar (usa DateTime.UtcNow)
```

**✅ Código com BirthDate:**

```csharp
public class AgeVerificationService
{
    private readonly TimeProvider _timeProvider;

    public AgeVerificationService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public bool CanBuyAlcohol(BirthDate birthDate)
    {
        return birthDate.CalculateAgeInYears(_timeProvider) >= 21;  // ✨ Sempre correto
    }

    public bool CanVote(BirthDate birthDate)
    {
        return birthDate.CalculateAgeInYears(_timeProvider) >= 16;  // ✨ Mesmo cálculo
    }

    public bool CanDrive(BirthDate birthDate)
    {
        return birthDate.CalculateAgeInYears(_timeProvider) >= 18;  // ✨ Consistente
    }
}

// Uso correto:
var birthDate = BirthDate.CreateNew(new DateTimeOffset(2003, 3, 15, 0, 0, 0, TimeSpan.Zero));

var service = new AgeVerificationService(TimeProvider.System);
service.CanBuyAlcohol(birthDate);  // Retorna FALSE ✅ CORRETO!

// Testável:
var fixedTime = new DateTimeOffset(2024, 3, 10, 0, 0, 0, TimeSpan.Zero);
var testTimeProvider = new FakeTimeProvider(fixedTime);
var testService = new AgeVerificationService(testTimeProvider);

testService.CanBuyAlcohol(birthDate);  // FALSE - 20 anos
testService.CanVote(birthDate);        // TRUE - 20 anos >= 16
testService.CanDrive(birthDate);       // TRUE - 20 anos >= 18

✅ Benefícios:
- Cálculo único, centralizado e correto
- Todos os métodos usam a mesma lógica
- Testável via TimeProvider
- Sem bugs de ano bissexto
```

---

### 2. 🏷️ Falta de Semântica no Tipo

**Problema:** Usar `DateTimeOffset` para tudo perde a intenção do dado e permite erros de uso.

#### 📚 Analogia: A Etiqueta do Presente

Imagine uma loja que embala presentes:

**❌ Sem tipo semântico:**

```
Caixa 1: "DateTimeOffset" → O que é? Data de compra? Entrega? Nascimento?
Caixa 2: "DateTimeOffset" → Mesmo problema!
Caixa 3: "DateTimeOffset" → Impossível saber sem abrir!

⚠️ PROBLEMA: Funcionário pode confundir e calcular idade
   usando a data de compra em vez da data de nascimento!
```

**✅ Com tipo semântico:**

```
Caixa 1: "BirthDate" → Claramente uma data de nascimento
Caixa 2: "PurchaseDate" → Data de compra
Caixa 3: "DeliveryDate" → Data de entrega

✅ CORRETO: Impossível confundir, cada tipo tem seu propósito
```

#### 💻 Impacto Real no Código

**❌ Código com DateTimeOffset genérico:**

```csharp
public class CustomerService
{
    public void ProcessCustomer(
        DateTimeOffset birthDate,
        DateTimeOffset registrationDate,
        DateTimeOffset lastPurchaseDate
    )
    {
        // ⚠️ Fácil passar parâmetros na ordem errada!
        var age = CalculateAge(registrationDate);  // BUG: usou data errada!

        if (age >= 18)
        {
            // Lógica de adulto...
        }
    }

    // Qual data esse método espera? Nascimento? Qualquer uma?
    private int CalculateAge(DateTimeOffset date)
    {
        return DateTime.UtcNow.Year - date.Year;
    }
}

// Chamada errada (compila sem erro!):
customerService.ProcessCustomer(
    birthDate: registrationDate,      // ⚠️ Trocou!
    registrationDate: birthDate,      // ⚠️ Trocou!
    lastPurchaseDate: purchaseDate
);

❌ Problemas:
- Parâmetros podem ser trocados sem erro de compilação
- Método CalculateAge não expressa que espera data de nascimento
- Nenhuma proteção contra uso incorreto
```

**✅ Código com BirthDate:**

```csharp
public class CustomerService
{
    public void ProcessCustomer(
        BirthDate birthDate,                    // ✨ Tipo específico
        DateTimeOffset registrationDate,
        DateTimeOffset lastPurchaseDate,
        TimeProvider timeProvider
    )
    {
        // ✨ Impossível confundir - birthDate tem método específico
        var age = birthDate.CalculateAgeInYears(timeProvider);

        if (age >= 18)
        {
            // Lógica de adulto...
        }
    }
}

// Chamada errada NÃO compila!
customerService.ProcessCustomer(
    birthDate: registrationDate,      // ❌ Erro de compilação!
    registrationDate: birthDate,      // ❌ Erro de compilação!
    lastPurchaseDate: purchaseDate,
    timeProvider: TimeProvider.System
);

// Chamada correta:
customerService.ProcessCustomer(
    birthDate: BirthDate.CreateNew(birthDateValue),
    registrationDate: registrationDate,
    lastPurchaseDate: purchaseDate,
    timeProvider: TimeProvider.System
);

✅ Benefícios:
- Compilador impede troca de parâmetros
- Código auto-documentado
- Método CalculateAgeInYears só existe em BirthDate
- Impossível calcular idade de data de registro
```

---

## ✨ Funcionalidades

### 🎯 Cálculo Preciso de Idade

Considera mês E dia para determinar se o aniversário já ocorreu no ano atual.

```csharp
var birthDate = BirthDate.CreateNew(new DateTimeOffset(2000, 6, 15, 0, 0, 0, TimeSpan.Zero));

// Antes do aniversário
var beforeBirthday = new DateTimeOffset(2024, 6, 14, 0, 0, 0, TimeSpan.Zero);
var ageBefore = birthDate.CalculateAgeInYears(beforeBirthday);
Console.WriteLine(ageBefore);  // 23

// No dia do aniversário
var onBirthday = new DateTimeOffset(2024, 6, 15, 0, 0, 0, TimeSpan.Zero);
var ageOn = birthDate.CalculateAgeInYears(onBirthday);
Console.WriteLine(ageOn);  // 24

// Depois do aniversário
var afterBirthday = new DateTimeOffset(2024, 6, 16, 0, 0, 0, TimeSpan.Zero);
var ageAfter = birthDate.CalculateAgeInYears(afterBirthday);
Console.WriteLine(ageAfter);  // 24
```

**Algoritmo:**
```csharp
int age = referenceDate.Year - Value.Year;

// Ajusta se o aniversário ainda não ocorreu neste ano
if (referenceDate.Month < Value.Month ||
    (referenceDate.Month == Value.Month && referenceDate.Day < Value.Day))
{
    age--;
}
```

---

### 🧪 Testabilidade com TimeProvider

Suporte nativo a `TimeProvider` para testes determinísticos.

```csharp
// Produção
var age = birthDate.CalculateAgeInYears(TimeProvider.System);

// Testes com tempo fixo
var fixedTime = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
var testTimeProvider = new CustomTimeProvider(
    utcNowFunc: _ => fixedTime,
    localTimeZone: null
);
var testAge = birthDate.CalculateAgeInYears(testTimeProvider);
```

**Por quê é importante?**
- Testes reproduzíveis
- Sem dependência de relógio do sistema
- Pode simular qualquer data

---

### 🔄 Conversão Implícita

Conversão automática de/para `DateTimeOffset` para facilitar integração.

```csharp
// DateTimeOffset → BirthDate (implícito)
DateTimeOffset dateValue = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
BirthDate birthDate = dateValue;  // ✅ Conversão automática

// BirthDate → DateTimeOffset (implícito)
BirthDate bd = BirthDate.CreateNew(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));
DateTimeOffset dto = bd;  // ✅ Conversão automática

// Útil para persistência
SaveToDatabase(birthDate);  // Aceita DateTimeOffset, converte automaticamente
```

---

### 📅 Suporte a DateOnly

Factory method para criar a partir de `DateOnly` (comum em formulários).

```csharp
// A partir de DateOnly (sem hora)
var dateOnly = new DateOnly(2000, 6, 15);
var birthDate = BirthDate.CreateNew(dateOnly);

// Internamente converte para DateTimeOffset com hora 00:00:00 UTC
Console.WriteLine(birthDate.Value);  // 2000-06-15T00:00:00.0000000+00:00
```

---

### ⚖️ Operadores de Comparação

Comparação completa entre instâncias de `BirthDate`.

```csharp
var older = BirthDate.CreateNew(new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero));
var younger = BirthDate.CreateNew(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));

Console.WriteLine(older < younger);   // True (nasceu antes)
Console.WriteLine(older > younger);   // False
Console.WriteLine(older == older);    // True
Console.WriteLine(older != younger);  // True

// Ordenação funciona automaticamente
var birthDates = new List<BirthDate> { younger, older };
birthDates.Sort();
// birthDates[0] == older (nasceu primeiro)
```

---

## 🚀 Como Usar

### 1️⃣ Uso Básico - Criação e Cálculo de Idade

```csharp
using Bedrock.BuildingBlocks.Core.BirthDates;

// Criar BirthDate a partir de DateTimeOffset
var birthDate = BirthDate.CreateNew(
    new DateTimeOffset(1990, 5, 20, 0, 0, 0, TimeSpan.Zero)
);

// Calcular idade atual
var age = birthDate.CalculateAgeInYears(TimeProvider.System);
Console.WriteLine($"Idade: {age} anos");

// Calcular idade em uma data específica
var referenceDate = new DateTimeOffset(2024, 5, 19, 0, 0, 0, TimeSpan.Zero);
var ageOnDate = birthDate.CalculateAgeInYears(referenceDate);
Console.WriteLine($"Idade em 19/05/2024: {ageOnDate} anos");  // 33 (ainda não fez aniversário)
```

**Quando usar:** Qualquer situação que precise representar e calcular idade a partir de data de nascimento.

---

### 2️⃣ Uso com DateOnly (Formulários)

```csharp
using Bedrock.BuildingBlocks.Core.BirthDates;

// Dados vindos de um formulário web
var formDate = new DateOnly(1995, 12, 25);

// Criar BirthDate
var birthDate = BirthDate.CreateNew(formDate);

// Usar normalmente
var age = birthDate.CalculateAgeInYears(TimeProvider.System);
Console.WriteLine($"Idade: {age} anos");
```

**Quando usar:** Recebimento de datas de formulários que usam `DateOnly`.

---

### 3️⃣ Uso em Entidades de Domínio

```csharp
using Bedrock.BuildingBlocks.Core.BirthDates;

public class Person
{
    public string Name { get; private set; }
    public BirthDate BirthDate { get; private set; }

    private Person(string name, BirthDate birthDate)
    {
        Name = name;
        BirthDate = birthDate;
    }

    public static Person Create(string name, DateTimeOffset birthDate)
    {
        return new Person(name, BirthDate.CreateNew(birthDate));
    }

    public int GetAge(TimeProvider timeProvider)
    {
        return BirthDate.CalculateAgeInYears(timeProvider);
    }

    public bool IsAdult(TimeProvider timeProvider)
    {
        return GetAge(timeProvider) >= 18;
    }

    public bool CanRetire(TimeProvider timeProvider)
    {
        return GetAge(timeProvider) >= 65;
    }
}

// Uso:
var person = Person.Create("João", new DateTimeOffset(1960, 3, 15, 0, 0, 0, TimeSpan.Zero));
Console.WriteLine($"{person.Name} tem {person.GetAge(TimeProvider.System)} anos");
Console.WriteLine($"Pode aposentar: {person.CanRetire(TimeProvider.System)}");
```

**Quando usar:** Entidades de domínio que precisam de lógica baseada em idade.

---

### 4️⃣ Uso em Testes Unitários

```csharp
using Bedrock.BuildingBlocks.Core.BirthDates;
using Bedrock.BuildingBlocks.Core.TimeProviders;

public class PersonTests
{
    [Theory]
    [InlineData("2006-01-15", "2024-01-14", 17, false)]  // Dia antes do aniversário
    [InlineData("2006-01-15", "2024-01-15", 18, true)]   // No dia do aniversário
    [InlineData("2006-01-15", "2024-01-16", 18, true)]   // Dia depois do aniversário
    public void IsAdult_ShouldConsiderExactBirthday(
        string birthDateStr,
        string referenceDateStr,
        int expectedAge,
        bool expectedIsAdult)
    {
        // Arrange
        var birthDate = BirthDate.CreateNew(DateTimeOffset.Parse(birthDateStr));
        var referenceDate = DateTimeOffset.Parse(referenceDateStr);

        var timeProvider = new CustomTimeProvider(
            utcNowFunc: _ => referenceDate,
            localTimeZone: null
        );

        var person = Person.Create("Test", birthDate);

        // Act
        var age = person.GetAge(timeProvider);
        var isAdult = person.IsAdult(timeProvider);

        // Assert
        Assert.Equal(expectedAge, age);
        Assert.Equal(expectedIsAdult, isAdult);
    }

    [Fact]
    public void BirthDate_ShouldBeComparable()
    {
        // Arrange
        var older = BirthDate.CreateNew(new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var younger = BirthDate.CreateNew(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));

        // Act & Assert
        Assert.True(older < younger);
        Assert.True(younger > older);
        Assert.False(older == younger);
    }
}
```

**Quando usar:** Testes que precisam de controle preciso sobre datas e idades.

---

## ⚖️ Trade-offs

### Benefícios

| Benefício | Impacto | Análise |
|-----------|---------|---------|
| **Cálculo preciso de idade** | ✅ Alto | Considera mês e dia, elimina bugs comuns |
| **Type-safety** | ✅ Alto | Compilador impede uso incorreto de datas |
| **Testabilidade** | ✅ Alto | TimeProvider permite testes determinísticos |
| **Imutabilidade** | ✅ Médio | Thread-safe, sem efeitos colaterais |
| **Conversão implícita** | ✅ Médio | Integração fácil com código existente |
| **Value type** | ✅ Médio | Sem alocação no heap, mesmo tamanho que DateTimeOffset |

### Custos

| Custo | Impacto | Mitigação |
|-------|---------|-----------|
| **Novo tipo para aprender** | ⚠️ Baixo | API simples e intuitiva |
| **Conversão explícita ao criar** | ⚠️ Baixo | Usar factory methods `CreateNew()` |

### Quando Usar vs Quando Evitar

#### ✅ Use quando:
1. Precisa calcular idade de pessoas
2. Tem validações baseadas em idade (maioridade, aposentadoria, etc.)
3. Quer garantir que data de nascimento não seja confundida com outras datas
4. Precisa de testes determinísticos com datas
5. Tem múltiplos lugares que calculam idade

#### ❌ Evite quando:
1. A data é apenas para exibição (sem cálculo de idade)
2. Não há lógica de negócio baseada em idade
3. Sistema muito simples sem necessidade de type-safety

---

## 🔬 Exemplos Avançados

### 🏥 Validação de Faixa Etária para Plano de Saúde

```csharp
public enum HealthPlanCategory
{
    Child,      // 0-12 anos
    Teen,       // 13-17 anos
    Adult,      // 18-59 anos
    Senior      // 60+ anos
}

public class HealthPlanService
{
    private readonly TimeProvider _timeProvider;

    public HealthPlanService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public HealthPlanCategory GetCategory(BirthDate birthDate)
    {
        var age = birthDate.CalculateAgeInYears(_timeProvider);

        return age switch
        {
            < 13 => HealthPlanCategory.Child,
            < 18 => HealthPlanCategory.Teen,
            < 60 => HealthPlanCategory.Adult,
            _ => HealthPlanCategory.Senior
        };
    }

    public decimal CalculatePremium(BirthDate birthDate)
    {
        var category = GetCategory(birthDate);

        return category switch
        {
            HealthPlanCategory.Child => 150.00m,
            HealthPlanCategory.Teen => 200.00m,
            HealthPlanCategory.Adult => 350.00m,
            HealthPlanCategory.Senior => 600.00m,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public bool RequiresGuardian(BirthDate birthDate)
    {
        return birthDate.CalculateAgeInYears(_timeProvider) < 18;
    }
}

// Uso:
var birthDate = BirthDate.CreateNew(new DateTimeOffset(2010, 8, 20, 0, 0, 0, TimeSpan.Zero));
var service = new HealthPlanService(TimeProvider.System);

var category = service.GetCategory(birthDate);      // Child ou Teen dependendo da data atual
var premium = service.CalculatePremium(birthDate);  // Valor correspondente
var needsGuardian = service.RequiresGuardian(birthDate);  // True se < 18
```

**Pontos importantes:**
- Toda lógica de idade usa o mesmo cálculo (CalculateAgeInYears)
- TimeProvider injetado permite testes
- Categorias mudam automaticamente conforme aniversário

---

### 📊 Relatório de Distribuição Etária

```csharp
public class AgeDistributionReport
{
    private readonly TimeProvider _timeProvider;

    public AgeDistributionReport(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public Dictionary<string, int> GenerateReport(IEnumerable<BirthDate> birthDates)
    {
        var distribution = new Dictionary<string, int>
        {
            ["0-17"] = 0,
            ["18-29"] = 0,
            ["30-44"] = 0,
            ["45-59"] = 0,
            ["60+"] = 0
        };

        foreach (var birthDate in birthDates)
        {
            var age = birthDate.CalculateAgeInYears(_timeProvider);

            var category = age switch
            {
                < 18 => "0-17",
                < 30 => "18-29",
                < 45 => "30-44",
                < 60 => "45-59",
                _ => "60+"
            };

            distribution[category]++;
        }

        return distribution;
    }
}

// Uso em teste:
var fixedDate = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
var testTimeProvider = new CustomTimeProvider(
    utcNowFunc: _ => fixedDate,
    localTimeZone: null
);

var birthDates = new List<BirthDate>
{
    BirthDate.CreateNew(new DateTimeOffset(2010, 1, 1, 0, 0, 0, TimeSpan.Zero)),  // 14 anos
    BirthDate.CreateNew(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero)),  // 24 anos
    BirthDate.CreateNew(new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero)),  // 44 anos
    BirthDate.CreateNew(new DateTimeOffset(1960, 1, 1, 0, 0, 0, TimeSpan.Zero)),  // 64 anos
};

var report = new AgeDistributionReport(testTimeProvider);
var distribution = report.GenerateReport(birthDates);

// distribution["0-17"] = 1
// distribution["18-29"] = 1
// distribution["30-44"] = 1
// distribution["60+"] = 1
```

**Pontos importantes:**
- Relatório é completamente determinístico em testes
- Mesma lógica funciona em produção e testes
- Fácil adicionar novas faixas etárias

---

## 📚 Referências

- [TimeProvider in .NET 8](https://learn.microsoft.com/en-us/dotnet/api/system.timeprovider) - Documentação oficial do TimeProvider
- [Value Types in C#](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/struct) - Structs e value types
- [IEquatable<T> Interface](https://docs.microsoft.com/en-us/dotnet/api/system.iequatable-1) - Implementação de igualdade
- [Operator Overloading](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/operator-overloading) - Sobrecarga de operadores em C#
