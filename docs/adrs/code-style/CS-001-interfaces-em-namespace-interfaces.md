# CS-001: Interfaces Devem Residir em Subpasta Interfaces/

## Status

Aceita

## Contexto

### O Problema (Analogia)

Imagine um escritorio onde contratos e documentos de trabalho ficam
misturados na mesma gaveta. Quando voce precisa encontrar um contrato
especifico, precisa vasculhar entre relatorios, planilhas e rascunhos.
Agora imagine que todos os contratos ficam em uma pasta separada
rotulada "Contratos" dentro de cada gaveta. A busca fica instantanea.

### O Problema Tecnico

Em projetos C#, interfaces (`I*.cs`) definem contratos publicos que
consumidores externos dependem. Quando interfaces ficam misturadas
com suas implementacoes no mesmo diretorio e namespace, surgem
problemas:

1. **Descobribilidade**: Dificil identificar rapidamente quais sao
   os contratos publicos de um modulo.
2. **Namespace poluido**: Consumidores que fazem `using Passwords`
   recebem tanto contratos quanto implementacoes no IntelliSense.
3. **Inconsistencia**: Sem regra explicita, cada desenvolvedor
   (humano ou IA) coloca interfaces onde achar melhor.

## Como Normalmente E Feito

### Abordagem Tradicional

A maioria dos projetos C# coloca interfaces no mesmo diretorio das
implementacoes:

```
Passwords/
  IPasswordHasher.cs    # namespace Passwords
  PasswordHasher.cs     # namespace Passwords
  PasswordPolicy.cs     # namespace Passwords
```

### Por Que Nao Funciona Bem

- Ao navegar a pasta, nao ha separacao visual entre contratos e
  implementacoes.
- O namespace nao diferencia o que e contrato do que e implementacao.
- Em projetos grandes, pastas com 20+ arquivos misturam interfaces e
  classes concretas indistintamente.
- Code agents (LLMs) nao tem uma regra clara para seguir e criam
  interfaces em locais inconsistentes.

## A Decisao

### Nossa Abordagem

Toda interface (`I*.cs`) DEVE ser colocada em uma subpasta
`Interfaces/` dentro do diretorio funcional correspondente,
refletindo no namespace:

```
Passwords/
  Interfaces/
    IPasswordHasher.cs  # namespace Passwords.Interfaces
  PasswordHasher.cs     # namespace Passwords
  PasswordPolicy.cs     # namespace Passwords
```

A convencao se aplica a todo o repositorio:

```csharp
// Correto
namespace Bedrock.BuildingBlocks.Security.Passwords.Interfaces;
public interface IPasswordHasher { }

// Incorreto
namespace Bedrock.BuildingBlocks.Security.Passwords;
public interface IPasswordHasher { }
```

### Por Que Funciona Melhor

1. **Separacao visual**: Ao navegar diretórios, `Interfaces/` se
   destaca imediatamente.
2. **Namespace explicito**: `using ...Interfaces` deixa claro no
   codigo que voce esta importando contratos, nao implementacoes.
3. **Consistencia**: Regra unica e verificavel por codigo — zero
   ambiguidade para humanos e code agents.
4. **Padrao ja estabelecido**: O Bedrock ja usava essa convencao em
   12+ locais (Domain.Entities, Persistence.PostgreSql, Serialization,
   etc.). A regra formaliza e automatiza o padrao existente.

## Consequencias

### Beneficios

- Navegacao rapida: toda interface esta em `Interfaces/`.
- Code agents geram interfaces no local correto automaticamente.
- Regra Roslyn (CS001) verifica compliance automaticamente.
- Consumidores podem fazer `using ...Interfaces` para importar
  apenas contratos.

### Trade-offs (Com Perspectiva)

- **Subpasta extra**: Cada modulo com interfaces ganha uma subpasta.
  Na pratica, a maioria dos modulos ja tem subpastas
  (Enums/, Inputs/, Models/), entao `Interfaces/` e apenas mais uma.
- **Namespace adicional no using**: Um `using` extra por arquivo que
  consome interfaces. Custo minimo comparado a clareza ganha.
- **Interfaces existentes fora do padrao**: Algumas interfaces
  legadas (IRepository, IConnection, IExecutionContextAccessor)
  ainda estao fora de `Interfaces/`. Serao migradas incrementalmente
  conforme os modulos forem tocados.

## Fundamentacao Teorica

### Padroes de Design Relacionados

**Interface Segregation Principle (ISP)**: A separacao fisica em
subpasta reforca a separacao conceitual entre contrato e
implementacao.

### O Que o Clean Architecture Diz

> "Depend on abstractions, not concretions."
>
> *Dependa de abstracoes, nao de implementacoes concretas.*

A subpasta `Interfaces/` materializa essa regra no filesystem:
contratos (abstracoes) ficam separados de implementacoes (concretas).

### Outros Fundamentos

**Convenção sobre configuração**: Uma regra simples e universal
elimina decisoes caso-a-caso que geram inconsistencia.

## Aprenda Mais

### Perguntas Para Fazer a LLM

- "Quais sao os trade-offs de separar interfaces em subpastas
  vs. mante-las no mesmo diretorio?"
- "Como o namespace afeta a descobribilidade de contratos em
  projetos C# grandes?"
- "Que outros frameworks C# usam convencao de pasta Interfaces/?"

### Leitura Recomendada

- [Interface Segregation Principle](https://en.wikipedia.org/wiki/Interface_segregation_principle)
- [Clean Architecture — Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## Building Blocks Correlacionados

| Building Block | Relacao com a ADR |
|----------------|-------------------|
| Testing.Architecture | Implementa regra Roslyn CS001 que verifica esta convencao |
| Security | Primeiro BB onde a violacao foi detectada e corrigida |
| Domain.Entities | Exemplo de uso correto: `Users/Interfaces/IUser.cs` |
| Persistence.PostgreSql | Exemplo de uso correto: `Connections/Interfaces/`, `Repositories/Interfaces/` |

## Referencias no Codigo

- `src/BuildingBlocks/Testing/Architecture/Rules/CodeStyleRules/CS001_InterfacesInInterfacesNamespaceRule.cs` — Regra Roslyn
- `src/BuildingBlocks/Security/Passwords/Interfaces/IPasswordHasher.cs` — Exemplo corrigido
- `src/BuildingBlocks/Domain.Entities/Interfaces/IEntity.cs` — Exemplo existente correto
