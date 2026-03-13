# CS-004: Factory SRP — Uma Factory por Tipo de Retorno e Organização por Namespace

## Status
Aceita

## Contexto

### O Problema (Analogia)

Imagine uma padaria onde um único padeiro faz pães, bolos, croissants e biscoitos. Funciona quando há poucos produtos. Mas quando a padaria cresce, esse padeiro vira um gargalo: cada novo produto é mais uma receita no seu caderno, cada mudança de receita exige que ele pare tudo. A solução é especializar: um padeiro para pães, outro para bolos. Cada um domina sua receita, evolui independentemente e não bloqueia o outro.

### O Problema Técnico

A abordagem de uma factory "guarda-chuva" por bounded context (ex: `AuthEventFactory` com métodos `CreateUserRegistered`, `CreateUserAuthenticated`, `CreateUserDeactivated`, etc.) gera problemas à medida que o sistema cresce:

- **God Class latente**: Cada novo evento adiciona um método. Com 15+ eventos no Auth BC, a factory vira um arquivo de 300+ linhas
- **Conflitos no Git**: Desenvolvedores trabalhando em eventos diferentes tocam o mesmo arquivo, gerando merge conflicts frequentes
- **Violação de SRP**: A factory tem N razões para mudar (uma por evento) — qualquer alteração em um evento pode causar conflito com outro
- **Acoplamento temporal**: PRs que tocam eventos diferentes ficam bloqueados entre si por causa do arquivo compartilhado

## Como Normalmente É Feito

### Abordagem Tradicional

Uma factory centralizada por módulo ou bounded context:

```csharp
// God Factory — todos os eventos em um lugar
public static class AuthEventFactory
{
    public static UserRegisteredEvent CreateUserRegistered(...) { ... }
    public static UserAuthenticatedEvent CreateUserAuthenticated(...) { ... }
    public static UserDeactivatedEvent CreateUserDeactivated(...) { ... }
    public static PasswordChangedEvent CreatePasswordChanged(...) { ... }
    public static SessionCreatedEvent CreateSessionCreated(...) { ... }
    // ... 10+ métodos crescendo indefinidamente
}
```

### Por Que Não Funciona Bem

- **SRP violado**: O arquivo muda sempre que qualquer evento muda
- **Git hostil**: Merge conflicts em PRs paralelos que adicionam ou alteram eventos diferentes
- **Difícil de navegar**: 300+ linhas em um arquivo para encontrar o método certo
- **Sem coesão interna**: Os métodos não se relacionam entre si — apenas compartilham o prefixo "Auth"

## A Decisão

### Nossa Abordagem

Cada factory produz **um único tipo de retorno**. A organização de namespace espelha a categoria do artefato produzido:

```
Factories/
└── Messages/                          ← raiz: tudo produz artefatos de Messages
    ├── AuthMessageMetadataFactory.cs   ← produz MessageMetadata
    ├── Events/                         ← produz Events (subtipo de Message)
    │   ├── UserRegisteredEventFactory.cs
    │   └── UserAuthenticatedEventFactory.cs
    └── Models/                         ← produz Models (usados por Messages)
        └── UserModelFactory.cs
```

Regras:

1. **Um tipo de retorno por factory**: Todos os métodos públicos de uma factory retornam o mesmo tipo. Sobrecargas de input são permitidas, output diferente não
2. **Naming convention**: `{ReturnType}Factory` (ex: `UserRegisteredEventFactory` → retorna `UserRegisteredEvent`)
3. **Namespace por categoria de output**: O namespace reflete o que a factory produz, não quem a consome
4. **Static by design**: Factories são funções puras, sem estado, sem DI

```csharp
// UMA factory por tipo de evento
public static class UserRegisteredEventFactory
{
    // Método principal
    public static UserRegisteredEvent Create(
        ExecutionContext executionContext,
        TimeProvider timeProvider,
        string email,
        User user)
    {
        var metadata = AuthMessageMetadataFactory.Create(executionContext, timeProvider);
        var userModel = UserModelFactory.FromEntity(user, executionContext);

        return new UserRegisteredEvent(
            metadata,
            new RegisterUserInputModel(email),
            userModel);
    }

    // Sobrecarga com input diferente é OK (mesmo tipo de retorno)
    // public static UserRegisteredEvent Create(
    //     ExecutionContext executionContext,
    //     TimeProvider timeProvider,
    //     RegisterUserInputModel input,
    //     User user) { ... }
}
```

O que é proibido:

```csharp
// PROIBIDO: factory que retorna tipos diferentes
public static class AuthEventFactory
{
    public static UserRegisteredEvent CreateRegistered(...) { ... }     // ← tipo A
    public static UserAuthenticatedEvent CreateAuthenticated(...) { ... } // ← tipo B — violação
}
```

### Por Que Funciona Melhor

1. **SRP real**: Cada factory tem exatamente uma razão para mudar — quando o evento que ela produz muda
2. **Git-friendly**: Eventos diferentes = ficheiros diferentes = zero conflitos
3. **Navegação direta**: `UserRegisteredEventFactory` → está claro o que faz sem abrir o ficheiro
4. **Composição lateral mantida**: Factories de Events usam factories de Models e Messages — a composição MS-013 continua

## Consequências

### Benefícios

- **Isolamento total**: Mudança num evento não afeta ficheiros de outros eventos
- **Paralelismo no Git**: PRs tocando eventos diferentes nunca conflitam
- **Discoverability**: Ctrl+P + "UserRegistered" encontra a factory imediatamente
- **Consistência**: A naming convention torna a relação factory↔tipo explícita e verificável por regra de arquitetura

### Trade-offs (Com Perspectiva)

- **Mais ficheiros**: Um ficheiro por tipo de evento em vez de uma God Factory
  - Ficheiros pequenos e coesos são mais fáceis de manter que ficheiros grandes e monolíticos
- **Overhead inicial**: Criar uma nova factory exige criar um novo ficheiro
  - O template é trivial: copiar factory existente, trocar o tipo. E a regra de arquitetura impede que alguém "atalhe" adicionando ao ficheiro errado
- **Namespace mais profundo**: `Factories.Messages.Events` em vez de `Factories`
  - A profundidade reflete a organização real e ajuda na navegação

## Fundamentação Teórica

### Single Responsibility Principle (SRP)

> "A class should have only one reason to change."
>
> *Uma classe deve ter apenas uma razão para mudar.*
>
> — Robert C. Martin

Uma factory com N métodos retornando N tipos diferentes tem N razões para mudar. Dividir em N factories, cada uma com uma razão para mudar, é a aplicação direta do SRP.

### Common Closure Principle (CCP)

> "Classes that change together are packaged together."
>
> *Classes que mudam juntas são empacotadas juntas.*
>
> — Robert C. Martin

A factory de `UserRegisteredEvent` muda quando `UserRegisteredEvent` muda. Agrupá-las no mesmo namespace (`Messages.Events`) reflete essa coesão.

## Aprenda Mais

### Perguntas Para Fazer à LLM

- "Qual a diferença entre SRP aplicado a métodos vs a classes?"
- "Como o Common Closure Principle guia a organização de namespaces?"
- "Por que God Classes são um anti-pattern mesmo quando são static?"

### Leitura Recomendada

- [Single Responsibility Principle - Clean Architecture](https://blog.cleancoder.com/uncle-bob/2014/05/08/SingleReponsibilityPrinciple.html)
- [God Class - Refactoring Guru](https://refactoring.guru/antipatterns/god-class)

## Building Blocks Correlacionados

| Building Block | Relação com a ADR |
|----------------|-------------------|
| MS-013 | Define que eventos devem ser criados via factory; CS-004 define como organizar essas factories |

## Referências no Código

- [UserRegisteredEventFactory.cs](../../../src/ShopDemo/Auth/Application/Factories/Messages/Events/UserRegisteredEventFactory.cs) - exemplo de factory SRP
- [UserAuthenticatedEventFactory.cs](../../../src/ShopDemo/Auth/Application/Factories/Messages/Events/UserAuthenticatedEventFactory.cs) - exemplo de factory SRP
- [UserModelFactory.cs](../../../src/ShopDemo/Auth/Application/Factories/Messages/Models/UserModelFactory.cs) - factory de Model
- [AuthMessageMetadataFactory.cs](../../../src/ShopDemo/Auth/Application/Factories/Messages/AuthMessageMetadataFactory.cs) - factory de Metadata
