<!--
  ============================================================================
  Sync Impact Report
  ============================================================================
  Version change: 1.10.0 → 1.10.1
  Bump rationale: PATCH — correções e clarificações em BB-VII
    e BB-IX decorrentes do PR #173 (fix: merge architecture
    report across test collections).
  Modified principles:
    - BB-VII. Arquitetura Verificada por Código:
      - Atualizado DE001–DE058 → DE001–DE059 no diagrama.
      - Adicionada nota sobre ViolationManager com estado
        estático compartilhado e thread-safety via Lock para
        consolidar resultados de múltiplas fixtures/collections.
    - BB-IX. Disciplina de Testes Unitários:
      - Adicionada regra: classes de teste que compartilham
        estado estático DEVEM usar [Collection] para evitar
        race conditions na execução paralela do xUnit.
  Added sections: Nenhuma
  Removed sections: Nenhuma
  Templates requiring updates:
    - .specify/templates/plan-template.md       ✅ compatível
    - .specify/templates/spec-template.md        ✅ compatível
    - .specify/templates/tasks-template.md       ✅ compatível
    - .specify/templates/checklist-template.md   ✅ compatível
    - .specify/templates/agent-file-template.md  ✅ compatível
  Follow-up TODOs: Nenhum
  ============================================================================
-->

# Bedrock Constitution

## Core Principles

### I. Qualidade Inegociável

Todo código entregue DEVE atingir 100% de cobertura de testes e 100%
de score de mutação (Stryker.NET). Nenhum mutante sobrevivente é
aceitável em código desenvolvido com assistência de IA.

**Cobertura e mutação:**

- 100% de cobertura de linhas (Coverlet).
- 100% de score de mutação (Stryker.NET, threshold 100/100/100).
- Exclusões (`[ExcludeFromCodeCoverage]` + Stryker comments) são
  permitidas SOMENTE para código genuinamente impossível de testar,
  com justificativa documentada em pt-BR.

**Bibliotecas obrigatórias:**

| Biblioteca | Propósito |
|------------|-----------|
| xUnit | Framework de testes |
| Shouldly | Assertions fluentes |
| Moq | Mocking |
| Bogus | Geração de dados fake |
| Humanizer | Formatação humanizada em logs de teste |
| Coverlet | Cobertura de código |
| Stryker.NET | Testes de mutação |

**Cobertura obrigatória de cenários:**

Todo teste unitário DEVE cobrir, quando aplicável:
1. Happy path — inputs válidos e comportamento esperado.
2. Null checks — validação de parâmetros nulos.
3. Inputs inválidos — lógica de validação.
4. Edge cases — condições de fronteira, strings vazias, defaults.
5. Exceções — verificação de exceções lançadas corretamente.
6. Operadores de igualdade — `==`, `!=`, `<`, `>`, `<=`, `>=`.
7. Hash code — consistência de `GetHashCode()`.
8. Cloning/cópia — comportamento de deep copy.
9. Thread safety — acesso concorrente quando aplicável.
10. Mutation killers — testes explícitos para matar mutantes
    específicos, documentados com comentário descrevendo o
    mutante alvo.

**Pipeline (fluxo escalonado):**

- Durante o desenvolvimento, o code agent DEVE usar comandos
  leves para feedback rápido:
  - `dotnet build` — após qualquer alteração de código.
  - `dotnet test <projeto>` — após escrever ou alterar testes.
- Commits intermediários na branch de trabalho NÃO exigem
  pipeline completa — basta build e testes focados.
- A pipeline local completa (`./scripts/pipeline.sh`) DEVE
  passar antes de abrir a PR. Isso inclui cobertura, mutação
  e análise do SonarCloud.

**Autonomia do code agent para exclusões:**

O code agent PODE decidir autonomamente ignorar ou excluir itens
de cobertura, mutação ou análise estática quando julgar que a
pendência não é aplicável, desde que documente a justificativa.

- **Cobertura de testes**: O code agent PODE aplicar
  `[ExcludeFromCodeCoverage]` em código genuinamente impossível
  de testar, incluindo justificativa no atributo em pt-BR.
- **Mutantes sobreviventes**: O code agent PODE aplicar Stryker
  comments (`// Stryker disable ...`) em mutações que são
  impossíveis ou impraticáveis de matar, incluindo a razão
  no comentário.
- **Issues do SonarCloud**: O code agent PODE decidir não
  resolver issues que sejam falsos positivos, que contradigam
  decisões arquiteturais documentadas, ou que não se apliquem
  ao contexto do projeto. Neste caso DEVE:
  1. Documentar no PR qual issue foi ignorada e o motivo.
  2. Marcar como "Won't Fix" no SonarCloud quando tiver acesso.
- **Requisitos para toda exclusão**:
  - Justificativa DEVE ser documentada em pt-BR.
  - A justificativa DEVE explicar *por que* o item não é
    aplicável, não apenas *que* foi ignorado.
  - Exclusões DEVEM ser granulares — preferir `disable once`
    sobre `disable all`, e métodos isolados com
    `[ExcludeFromCodeCoverage]` sobre classes inteiras.
  - O code agent DEVE informar o usuário sobre cada exclusão
    feita, listando o item e a justificativa.

**Razão**: Código assistido por IA tem risco elevado de testes
superficiais. O threshold absoluto elimina falsa confiança e garante
que cada branch lógica é exercitada. A autonomia do code agent para
exclusões justificadas evita loops infinitos em pendências que não
agregam valor real à qualidade do código.

### II. Simplicidade Deliberada

Toda decisão de design DEVE favorecer a solução mais simples que
atende ao requisito atual. Abstrações, indireções e generalizações
DEVEM ser justificadas por necessidade concreta e imediata.

- YAGNI (You Aren't Gonna Need It) é a regra padrão.
- Três linhas similares são preferíveis a uma abstração prematura.
- Novos projetos, camadas ou pacotes DEVEM ter justificativa
  explícita documentada no PR ou na issue.
- Feature flags e shims de compatibilidade retroativa DEVEM ser
  evitados quando é possível simplesmente alterar o código.

**Razão**: Complexidade acidental é o maior risco em projetos de
framework. Cada abstração tem custo de manutenção que se acumula
ao longo do tempo.

### III. Observabilidade Nativa

Todo componente do Bedrock DEVE ser inspecionável em tempo de
execução. Logging estruturado, métricas e rastreamento DEVEM ser
considerados desde o design inicial, não adicionados depois.

- Logs DEVEM usar formato estruturado (chave-valor).
- Erros DEVEM incluir contexto suficiente para diagnóstico sem
  acesso ao código-fonte.
- Componentes que processam dados DEVEM expor métricas de
  throughput e latência quando aplicável.

**Razão**: Um framework é usado por terceiros que não têm acesso ao
código interno. Sem observabilidade, problemas em produção se tornam
caixas-pretas impossíveis de diagnosticar.

### IV. Modularidade por Contrato

Cada BuildingBlock DEVE ser independente, auto-contido e comunicar-se
com outros blocos exclusivamente por contratos explícitos (interfaces,
abstrações). Dependências entre blocos DEVEM seguir a direção
Abstractions -> Implementação.

- Cada BuildingBlock DEVE ter seu próprio projeto .csproj.
- Relação 1:1 obrigatória entre projeto src e projeto de testes.
- Dependências circulares são proibidas.
- Contratos públicos (interfaces, tipos) DEVEM ser estáveis;
  breaking changes DEVEM ser documentados e justificados.
- Toda interface (`I*.cs`) DEVE ser colocada em uma subpasta
  `Interfaces/` dentro do diretório funcional correspondente,
  refletindo no namespace. Exemplos:
  - `Passwords/Interfaces/IPasswordHasher.cs`
    → namespace `...Passwords.Interfaces`
  - `Repositories/Interfaces/IPostgreSqlRepository.cs`
    → namespace `...Repositories.Interfaces`
  - `Users/Interfaces/IUser.cs`
    → namespace `...Users.Interfaces`
- A pasta `Interfaces/` DEVE conter SOMENTE interfaces.
  Implementações concretas ficam no diretório pai.

**Razão**: Modularidade permite que consumidores adotem apenas os
blocos necessários. Contratos estáveis protegem contra regressões
em cascata. A convenção `Interfaces/` como subpasta padronizada
garante separação visual e de namespace entre contratos e
implementações, facilitando navegação e evitando mistura de
responsabilidades no mesmo diretório.

### V. Automação como Garantia

Todo gate de qualidade DEVE ser automatizado e reproduzível. A
pipeline local DEVE ser equivalente funcional da pipeline de CI.
Verificações manuais não substituem automação.

- Pipeline local: `./scripts/pipeline.sh` (build, test, cobertura,
  mutação, SonarCloud).
- Pipeline CI: GitHub Actions com os mesmos gates.
- Artefatos de pendência (`artifacts/pending/`) DEVEM ser
  consumíveis programaticamente.
- Limite de 5 tentativas por ciclo de correção (local e CI) para
  evitar loops infinitos.

**Razão**: Automação elimina variabilidade humana e garante que o
nível de qualidade definido no Princípio I seja consistentemente
aplicado.

## BuildingBlocks Principles

### BB-I. Performance como Requisito

Toda estrutura de dados nos BuildingBlocks DEVE ser projetada para
zero-allocation no hot path. Performance não é otimização prematura;
é um requisito funcional de um framework.

**Tipos e estruturas:**

- Value objects DEVEM ser `readonly struct` (Id, RegistryVersion,
  EmailAddress, BirthDate, PhoneNumber, PaginationInfo, etc.).
- Geração de IDs (UUIDv7) DEVE usar `[ThreadStatic]` para evitar
  locks em cenários de alta frequência.

**Anti-padrões de alocação heap (proibidos em hot paths):**

- `new` de objetos/arrays DEVE ser evitado quando alternativa
  sem alocação existir.
- Boxing de value types é proibido — cast para `object` ou uso
  de interface em struct DEVE ser evitado.
- Closures que capturam variáveis (lambdas com estado) DEVEM ser
  substituídas por métodos estáticos ou delegates sem captura.
- Concatenação de strings com `+` ou interpolação sem handler é
  proibida — usar `string.Create()` com `SpanAction` ou
  interpolated string handler.
- `params T[]` DEVE ser evitado em APIs de alta frequência —
  preferir overloads explícitos ou `params ReadOnlySpan<T>`.
- LINQ que aloca (`.ToList()`, `.ToArray()`, `.Select()`,
  iteradores) é proibido em hot paths — usar loops explícitos.
- `ToString()` sem necessidade comprovada é proibido.

**Oportunidades Span/Memory (obrigatórias quando aplicável):**

- `Span<char>` / `ReadOnlySpan<char>` DEVEM ser usados em vez
  de `string.Substring()` para parsing sem alocação.
- `stackalloc` DEVE ser usado para buffers temporários de até
  256 bytes; acima deste limite usar `ArrayPool<T>.Shared`.
- `MemoryExtensions` (`.AsSpan()`, `.Slice()`) DEVEM ser
  preferidos sobre operações que alocam novas strings.
- APIs DEVEM oferecer variantes baseadas em `Span<T>` e
  `TryFormat` quando aplicável.

**Pooling (obrigatório para buffers reutilizáveis):**

- `ArrayPool<T>.Shared` DEVE ser usado para buffers temporários
  acima de 256 bytes.
- `RecyclableMemoryStream` DEVE ser usado para pooling de
  buffers em serialização.
- Object pooling DEVE ser considerado para objetos
  frequentemente criados/descartados em cenários de alto
  throughput.

**Priorização de correções:**

1. Hot paths — código executado frequentemente.
2. Tamanho da alocação — buffers grandes primeiro.
3. Facilidade de correção — quick wins.

**Métricas esperadas após revisão:**

- Zero alocações em hot paths.
- `stackalloc` para buffers ≤ 256 bytes.
- `ArrayPool` para buffers > 256 bytes.
- `Span<T>` para todas as operações de slice/substring.

**Razão**: Um framework é infraestrutura. Alocações desnecessárias
no framework se multiplicam por cada operação do consumidor,
degradando toda a aplicação.

### BB-II. Imutabilidade por Padrão

Toda estrutura de dados DEVE ser imutável por padrão. Mutação de
estado é permitida SOMENTE através do padrão Clone-Modify-Return,
onde cada operação retorna uma nova instância.

- Value objects (Core): `readonly struct` sem setters públicos.
- Entidades de domínio: métodos públicos de alteração DEVEM
  retornar `T?` (nova instância ou null se validação falhar).
- Coleções internas: campo privado `List<T>`, propriedade
  pública `IReadOnlyList<T>`.
- Construtores de reconstitução DEVEM criar cópias defensivas
  de coleções recebidas.
- Métodos `void` de mutação são proibidos em entidades
  (atualmente verificado pela regra DE034).

**Razão**: Imutabilidade elimina bugs de estado compartilhado,
facilita raciocínio sobre concorrência e torna cada transição de
estado explícita e rastreável.

### BB-III. Estado Inválido Nunca Existe

Nenhuma instância de entidade DEVE existir em estado inválido.
Criação e alteração de entidades DEVEM passar por validação
obrigatória via factory methods.

- Criação: `RegisterNew(ExecutionContext, ...)` retorna `T?`.
  Null indica falha de validação com mensagens no
  ExecutionContext.
- Reconstitução: `CreateFromExistingInfo(...)` reconstrói a
  partir de dados persistidos sem revalidar.
- Construtores DEVEM ser privados. Dois construtores:
  parameterless (desserialização) e full (reconstitução).
- Classes concretas sem herdeiros DEVEM ser `sealed`.
- Validação DEVE usar operador `&` (bitwise AND) para executar
  todas as checagens, nunca `&&` (short-circuit).
- Resultados de validação DEVEM ser armazenados em variáveis
  intermediárias nomeadas para clareza.
- Regras de validação DEVEM ser definidas em propriedades
  estáticas de metadata, não em data annotations.

> *Nota*: Estas convenções são atualmente verificadas por regras
> Roslyn (ex: DE001, DE002, DE006, DE025). O conjunto de regras
> está em evolução e será expandido conforme a arquitetura amadurece.

**Razão**: Factory methods com validação obrigatória garantem que
o sistema nunca processa dados inconsistentes. O padrão nullable
torna falhas de validação explícitas sem uso de exceptions para
controle de fluxo.

### BB-IV. Explícito sobre Implícito

Toda dependência de contexto DEVE ser passada explicitamente.
Nenhum componente DEVE depender de estado global, ambient context
ou service locator.

- `ExecutionContext` DEVE ser o primeiro parâmetro de todo método
  de domínio e infraestrutura.
- `TimeProvider` DEVE ser obtido exclusivamente via
  `ExecutionContext.TimeProvider`, nunca via `DateTime.UtcNow`
  ou `DateTimeOffset.UtcNow`.
- `TenantInfo` DEVE fluir através do `ExecutionContext` para
  suporte nativo a multi-tenancy.
- `CancellationToken` DEVE ser o último parâmetro de todo
  método assíncrono.
- `PaginationInfo` DEVE ser sempre explícito em consultas;
  usar `PaginationInfo.All` quando unbounded for intencional.

**Razão**: Dependências explícitas tornam o código testável,
rastreável e livre de surpresas em produção. Ambient context
esconde acoplamento e dificulta diagnóstico.

### BB-V. Aggregate Root como Fronteira

O padrão Aggregate Root DEVE ser a única fronteira de
persistência. Entidades filhas são gerenciadas exclusivamente
através do seu aggregate root.

- Repositórios (`IRepository<TAggregateRoot>`) DEVEM existir
  SOMENTE para aggregate roots, nunca para entidades filhas.
- Operações em coleções filhas DEVEM processar itens
  individualmente com validação.
- Lookup de entidades filhas DEVE ser por `Id`, nunca por
  referência de objeto.
- Métodos de repositório DEVEM usar o Handler Pattern
  (`ItemHandler<T>`) para enumeração, evitando que exceções
  de infraestrutura vazem para a camada de domínio.
- Nomenclatura de repositório DEVE refletir intenção de
  negócio (behavior-driven), não operações CRUD.

**Razão**: Aggregate roots definem limites transacionais claros.
O Handler Pattern centraliza tratamento de erros e previne
abstrações vazadas (leaky abstractions).

### BB-VI. Separação Domain.Entities / Domain

O código de domínio DEVE ser dividido em dois blocos com
responsabilidades distintas e regras de dependência estritas.

- **Domain.Entities**: Contém entidades, value objects do
  domínio e regras de negócio puras. DEVE depender SOMENTE
  de Core. Zero dependências de infraestrutura.
- **Domain**: Contém abstrações de integração (IRepository,
  domain services). DEVE depender de Core e Domain.Entities.
- Domain.Entities DEVE ser portável para qualquer runtime
  (Blazor WASM, Unity, mobile) sem adaptação.
- Métodos públicos de entidade DEVEM delegar para métodos
  `protected internal` com sufixo `Internal`.

**Razão**: A separação permite que entidades de domínio sejam
usadas em clientes (validação client-side, UI binding) sem
arrastar dependências de infraestrutura. Domain contém as
abstrações que conectam o domínio ao mundo externo.

### BB-VII. Arquitetura Verificada por Código

Regras arquiteturais DEVEM ser verificadas automaticamente por
analisadores Roslyn. Convenções documentadas que não são
verificadas por código são recomendações, não regras.

- Regras arquiteturais Roslyn DEVEM ser executadas como testes
  unitários no pipeline. Categorias atuais:
  - **DE** (Domain Entities): DE001+ — regras para entidades
    de domínio (sealed, factory methods, imutabilidade, etc.).
  - **CS** (Code Style): CS001+ — regras de estilo de código
    aplicáveis a qualquer tipo em qualquer projeto (interfaces
    em Interfaces/, etc.).
  O conjunto de regras está em evolução e será expandido
  conforme novos padrões forem estabelecidos.
- Cada regra DEVE ter: severity, caminho de ADR associado e
  LLM hints para correção assistida.
- Violações são reportadas com arquivo, linha e mensagem
  clara.
- Novas regras arquiteturais DEVEM ser acompanhadas de
  testes unitários que validem detecção e não-detecção.

**Organização dos testes arquiteturais:**

Testes arquiteturais residem em `tests/ArchitectureTests/`
e validam que o código dos projetos cumpre as regras Roslyn.

- Cada categoria de regra DEVE ter uma única classe de teste
  consolidada, NÃO um arquivo por regra:
  - `DomainEntitiesRuleTests` — todas as regras DE001–DE058.
  - `CodeStyleRuleTests` — todas as regras CS001+.
- Cada `[Fact]` DEVE ser prefixado com o código da regra
  (ex: `DE001_`, `CS001_`) e ordenado numericamente para
  facilitar localização.
- Métodos DEVEM ser organizados com `#region` em blocos de
  10 regras (ex: `#region DE001–DE010`).
- Cada categoria DEVE ter sua própria fixture (herdando
  `RuleFixture`) que define os projetos a escanear via
  `GetProjectPaths()`.
- Testes usam `AssertNoViolations(new RuleClass())` —
  a infraestrutura base (`RuleTestBase<TFixture>`) gera
  relatórios e pendências em `artifacts/` automaticamente.
- Novas categorias de regra exigem: fixture +
  `[CollectionDefinition]` + classe de teste, tudo no
  mesmo projeto de testes arquiteturais.

**ViolationManager — estado estático compartilhado:**

- `ViolationManager` usa estado estático (`_violations`,
  `_ruleResults`) compartilhado entre todas as instâncias,
  protegido por `Lock` para thread-safety.
- Cada fixture (`DomainEntitiesArchFixture`,
  `CodeStyleArchFixture`) cria sua própria instância de
  `ViolationManager`, mas todas acumulam resultados no
  mesmo store estático.
- Isso garante que o relatório JSON consolidado
  (`architecture-report.json`) inclui resultados de TODAS
  as categorias de regra, independente da ordem de execução
  das collections do xUnit.
- `ResetSharedState()` DEVE ser chamado nos testes unitários
  do próprio `ViolationManager` para isolamento.

```
tests/ArchitectureTests/Templates/Domain.Entities/
├── Fixtures/
│   ├── DomainEntitiesArchFixture.cs   # Escaneia templates e samples
│   └── CodeStyleArchFixture.cs        # Escaneia BBs e samples
├── DomainEntitiesRuleTests.cs         # 59 [Fact] DE001–DE059
└── CodeStyleRuleTests.cs              # [Fact] CS001+
```

**Razão**: Documentação desatualiza; código não. Regras
verificadas por analisadores garantem compliance contínuo sem
depender de revisão manual. A consolidação em uma classe por
categoria elimina proliferação de arquivos idênticos e facilita
navegação — cada regra é apenas um `[Fact]` prefixado.
O estado estático compartilhado no `ViolationManager` garante
que múltiplas collections do xUnit contribuem para um único
relatório consolidado, evitando que a última collection
sobrescreva os resultados das anteriores.

### BB-VIII. Camadas de Infraestrutura por Template Method

Implementações de infraestrutura (Data, Persistence) DEVEM usar
o padrão Template Method para centralizar cross-cutting concerns.

- `RepositoryBase<T>` DEVE envolver métodos abstratos internos
  com tratamento de erros, logging e distributed tracing.
- Métodos abstratos internos (`*InternalAsync`) DEVEM conter
  apenas a lógica específica da implementação.
- Exceções de infraestrutura (SqlException, DbException) DEVEM
  ser capturadas e logadas na camada base; a camada de domínio
  recebe `null` ou `false` como indicação de falha.
- Mapeamento entre entidades de domínio e data models DEVE ser
  feito por `DataModelMapperBase`, isolando a transformação.
- Concorrência otimista DEVE usar `RegistryVersion` com
  parâmetro `expectedVersion` em operações de atualização.

**Razão**: Template Method evita duplicação de cross-cutting
concerns em cada implementação concreta. Exceções de
infraestrutura não devem vazar para camadas superiores.

### BB-IX. Disciplina de Testes Unitários

Todo teste unitário DEVE seguir uma estrutura padronizada que
garante rastreabilidade, geração automática de relatórios e
consistência entre todos os BuildingBlocks.

**Herança obrigatória:**

- Toda classe de teste DEVE herdar de `TestBase`.
- O construtor DEVE receber `ITestOutputHelper` e repassar
  para `base(outputHelper)`.
- Para testes com injeção de dependência, usar
  `ServiceCollectionFixture` com `[Collection]` e
  `[CollectionDefinition]`.

**Padrão AAA com logging estruturado:**

- Todo teste DEVE chamar `LogArrange()`, `LogAct()` e
  `LogAssert()` na sequência correta.
- As descrições DEVEM ser em português (pt-BR) e descrever
  a intenção do passo, não a implementação.
- `LogArrange` mapeia para BDD "Dado" (Given).
- `LogAct` mapeia para BDD "Quando" (When).
- `LogAssert` mapeia para BDD "Então" (Then).
- `LogInfo()` DEVE ser usado para registrar valores
  intermediários relevantes ou detalhes do resultado.
- `LogSection()` DEVE ser usado para criar separadores
  visuais em testes complexos.
- `LogElapsed()` DEVE ser usado para registrar durações
  com formatação humanizada (Humanizer).

**Marcadores estruturados para relatórios:**

- Os métodos `LogArrange`, `LogAct` e `LogAssert` emitem
  marcadores `##STEP##` com JSON estruturado:
  `##STEP##{"type":"Dado|Quando|Então","description":"...","timestamp":"..."}`.
- O script `scripts/generate-unittest-report.sh` parseia
  estes marcadores para gerar relatórios HTML consolidados
  em `artifacts/unittest-report/index.html`.
- Timestamps usam formato `HH:mm:ss.fff` para logs e
  ISO 8601 (`"O"`) para step markers.
- Descrições são escapadas para JSON válido.

**Nomenclatura:**

- Classe de teste: `{ClasseTestada}Tests`
  (ex: `IdTests`, `ExecutionContextTests`).
- Método de teste: `{Metodo}_{Cenario}_{ResultadoEsperado}`
  com underscores (ex:
  `Create_WithValidParameters_ShouldCreateContext`,
  `GenerateNewId_ShouldReturnValidId`).
- Arquivo: um arquivo por classe de teste, nome igual à
  classe (`{ClassName}.cs`).
- Namespace: `Bedrock.UnitTests.BuildingBlocks.{Componente}`
  espelhando a estrutura de diretórios.

**Assertions — Shouldly obrigatório:**

- Todas as assertions DEVEM usar Shouldly.
- `Assert.*` do xUnit é proibido.
- Padrões:
  - Igualdade: `result.ShouldBe(expected)`
  - Nulidade: `value.ShouldBeNull()` / `.ShouldNotBeNull()`
  - Booleano: `condition.ShouldBeTrue()` / `.ShouldBeFalse()`
  - Coleções: `.ShouldBeEmpty()`, `.ShouldContain(item)`
  - Exceções: `Should.Throw<TException>(() => action())`
  - Comparações: `.ShouldBeGreaterThan()`, `.ShouldBeLessThan()`
  - Tipos: `.ShouldBeAssignableTo<T>()`

**xUnit — atributos:**

- `[Fact]` para testes com caso único.
- `[Theory]` + `[InlineData]` para testes parametrizados.
- `[Collection]` + `[CollectionDefinition]` para fixtures
  compartilhadas.

**Isolamento de estado estático entre classes de teste:**

- Classes de teste que compartilham estado estático (ex:
  `PasswordPolicyMetadata`, `ViolationManager`) DEVEM usar
  `[Collection("NomeDaCollection")]` para impedir que o
  xUnit as execute em paralelo.
- Sem `[Collection]`, o xUnit executa classes diferentes
  em paralelo por padrão, causando race conditions quando
  uma classe modifica estado estático que outra lê.
- O padrão save/restore em `try/finally` NÃO é suficiente
  para isolamento — ele protege contra falhas no teste, mas
  NÃO contra execução paralela de outra classe.
- Testes dentro da mesma classe já são serializados pelo
  xUnit; a proteção é necessária entre classes diferentes.

**Organização interna do teste:**

- Testes DEVEM ser organizados com `#region`:
  `Constructor Tests`, `Create Tests`, `Validation Tests`,
  `Edge Cases`, `Mutation Killing Tests`, `Helper Methods`.
- Helpers e classes internas de teste DEVEM ficar em
  `#region Helper Methods` no final do arquivo.
- Classes nested para testar comportamento interno
  (ex: `private class TestEntity : EntityBase<TestEntity>`)
  são permitidas dentro de `#region Helper Methods`.

**Setup no construtor:**

- Dados de teste comuns (TimeProvider, TenantInfo,
  CorrelationId) DEVEM ser inicializados no construtor
  da classe de teste, não em cada método.
- Mocks DEVEM ser inicializados no construtor com setup
  padrão e armazenados em campos `private readonly`.

**Mutation killers:**

- Testes que existem especificamente para matar mutantes
  DEVEM ter comentário descrevendo o mutante alvo:
  `// Mata mutante: CompareTo < 0 -> CompareTo <= 0`.
- Testes de boundary conditions DEVEM testar exatamente
  o limite (valor mínimo, valor máximo, valor limite ± 1).
- Testes de cache/referência DEVEM usar
  `ReferenceEquals()` quando verificar instância única.

**Razão**: A padronização garante que qualquer desenvolvedor
(humano ou IA) produz testes com a mesma estrutura, permitindo
geração automática de relatórios BDD e rastreabilidade completa
de cada passo de cada teste.

### BB-X. Disciplina de Testes de Integração

Todo teste de integração DEVE seguir uma estrutura padronizada
que garante isolamento, rastreabilidade e construção explícita
do cenário de teste. A fixture provê fábricas; o teste orquestra.

**Herança obrigatória:**

- Toda classe de teste de integração DEVE herdar de
  `IntegrationTestBase` (que estende `TestBase`).
- O construtor DEVE receber a fixture e `ITestOutputHelper`,
  repassando o output para `base(output)`.
- A fixture DEVE ser armazenada em campo
  `private readonly`.

```csharp
[Collection("NomeDaCollection")]
[Feature("Nome do Feature", "Descrição em pt-BR")]
public class MeuTesteIntegrationTests : IntegrationTestBase
{
    private readonly MinhaFixture _fixture;

    public MeuTesteIntegrationTests(
        MinhaFixture fixture,
        ITestOutputHelper output)
        : base(output)
    {
        _fixture = fixture;
    }
}
```

**UseEnvironment no Arrange:**

- Todo teste de integração DEVE chamar
  `UseEnvironment(_fixture.Environments["chave"])` como
  primeira instrução do Arrange.
- `UseEnvironment` registra o environment ativo no output
  do teste, emitindo marcador `##ENV##` com informações
  de containers para o relatório HTML.
- O environment DEVE ser registrado na fixture via
  `ConfigureEnvironments(IEnvironmentRegistry)` usando a
  API fluent (`WithPostgres`, `WithDatabase`, `WithUser`,
  `WithSeedSql`, `WithResourceLimits`).

**Padrão AAA com logging estruturado:**

- Todo teste DEVE chamar `LogArrange()`, `LogAct()` e
  `LogAssert()` com descrições em pt-BR, seguindo o mesmo
  padrão de BB-IX.
- Logs especializados de integração DEVEM ser usados quando
  aplicável:
  - `LogEnvironmentSetup(string)` — registro do environment.
  - `LogDatabaseConnection(string, string)` — conexão ao banco.
  - `LogSeed(string, string)` — carga de dados de teste.
  - `LogSql(string)` — operações SQL executadas.
- `LogInfo()` DEVE registrar resultados intermediários e
  confirmações de sucesso ao final do Assert.

**Fixture como fábrica, teste como orquestrador:**

- A fixture DEVE fornecer apenas métodos de fábrica e helpers
  de infraestrutura. Exemplos:
  - `CreateExecutionContext(Guid? tenantCode)` — cria contexto.
  - `CreateAppUserConnection()` — cria conexão.
  - `CreateAppUserUnitOfWork()` — cria unit of work.
  - `CreateRepository(unitOfWork)` — cria repositório.
  - `CreateTestEntity(...)` — cria dados de teste.
  - `InsertTestEntityDirectlyAsync(entity)` — SQL direto.
  - `GetTestEntityDirectlyAsync(id, tenantCode)` — SQL direto.
  - `CleanupTestDataAsync(tenantCode)` — limpeza por tenant.
- A fixture NÃO DEVE conter métodos de negócio orquestradores
  (ex: `CriarEInserirEntidade()`, `ExecutarFluxoCompleto()`).
  Toda orquestração DEVE acontecer explicitamente no Arrange
  do teste, tornando o cenário visível e rastreável.
- O teste DEVE construir seu cenário completo no Arrange:
  criar entidades, inserir via SQL direto, criar conexões,
  criar repositórios — tudo explícito e legível.

**Acesso SQL direto para setup/verificação:**

- Dados de teste DEVEM ser inseridos via SQL direto
  (bypass do repositório) para isolar o que está sendo
  testado da infraestrutura de setup.
- Verificações pós-teste PODEM usar SQL direto para
  confirmar persistência independente do repositório.
- Helpers de SQL direto DEVEM ficar na fixture
  (`InsertTestEntityDirectlyAsync`,
  `GetTestEntityDirectlyAsync`,
  `UpdateEntityVersionDirectlyAsync`).

**Isolamento por tenant:**

- Cada teste DEVE usar um `Guid.NewGuid()` como
  `tenantCode` para garantir isolamento completo entre
  testes, mesmo quando executados em paralelo.
- Testes de multi-tenancy DEVEM criar múltiplos tenant
  codes e verificar que um tenant não acessa dados de
  outro.

**Limpeza automática via `await using`:**

- Conexões e UnitOfWork DEVEM ser criados com
  `await using` para garantir dispose automático.
- Transações não commitadas são automaticamente
  revertidas no dispose.
- Limpeza explícita (`CleanupTestDataAsync`) DEVE ser
  usada quando necessário para cenários específicos.

**Atributos de documentação:**

- `[Collection("NomeDaCollection")]` — obrigatório, agrupa
  testes que compartilham a mesma fixture.
- `[Feature("Nome", "Descrição em pt-BR")]` — obrigatório
  na classe, identifica o feature para relatórios.
- `[Scenario("Descrição em pt-BR")]` — opcional no método,
  documenta o cenário BDD para relatórios.
- `[Fact]` para testes com caso único; `[Theory]` +
  `[InlineData]` para parametrizados.

**Nomenclatura:**

- Classe: `{FeatureTestado}IntegrationTests`
  (ex: `ConnectionLifecycleIntegrationTests`,
  `DataModelRepositoryIntegrationTests`).
- Método: `{Metodo}_{Cenario}_{ResultadoEsperado}` ou
  `{Metodo}_Should_{Comportamento}_{Condicao}`
  (ex: `GetByIdAsync_Should_ReturnEntity_WhenExists`,
  `ExecuteAsync_Should_CommitTransaction_OnSuccess`).
- Namespace:
  `Bedrock.IntegrationTests.BuildingBlocks.{Componente}`
  espelhando a estrutura de diretórios.

**Fixture — configuração de environment:**

- Environments DEVEM ser configurados em
  `ConfigureEnvironments(IEnvironmentRegistry)` usando
  a API fluent:

```csharp
protected override void ConfigureEnvironments(
    IEnvironmentRegistry environments)
{
    environments.Register("repository", env => env
        .WithPostgres("main", pg => pg
            .WithImage("postgres:17")
            .WithDatabase("testdb", db => db
                .WithSeedSql("CREATE TABLE ..."))
            .WithUser("app_user", "password", user => user
                .WithSchemaPermission("public",
                    PostgresSchemaPermission.Usage)
                .OnDatabase("testdb", db => db
                    .OnAllTables(
                        PostgresTablePermission.ReadWrite)
                    .OnAllSequences(
                        PostgresSequencePermission.All)))
            .WithResourceLimits(
                memory: "256m", cpu: 0.5)));
}
```

- Services DEVEM ser registrados em
  `ConfigureServices(IServiceCollection)` para DI.
- A fixture gerencia o ciclo de vida dos containers
  Docker via `IAsyncLifetime` (start em `InitializeAsync`,
  stop em `DisposeAsync`).

**ExecutionContext e CancellationToken:**

- Todo método de repositório e infraestrutura DEVE
  receber `ExecutionContext` como primeiro parâmetro e
  `CancellationToken` como último.
- `ExecutionContext` DEVE ser criado via
  `_fixture.CreateExecutionContext(tenantCode)` para
  garantir tenant, user e correlation ID consistentes.

**Cobertura de cenários obrigatória:**

Todo teste de integração de repositório DEVE cobrir,
quando aplicável:
1. CRUD — Get, Insert, Update, Delete por ID.
2. Existência — `ExistsAsync` retorna true/false.
3. Transações — commit e rollback via UnitOfWork.
4. Concorrência otimista — first-wins com
   `expectedVersion`.
5. Multi-tenancy — isolamento entre tenants.
6. Handler Pattern — enumeração com paginação e early
   exit.
7. Ciclo de vida de conexão — open, close, dispose,
   idempotência.
8. Permissões — diferentes usuários (admin, app,
   readonly) com diferentes níveis de acesso.

**Razão**: Testes de integração com orquestração explícita
no Arrange tornam cada cenário auto-documentado e
independente. Fixtures que orquestram negócio escondem
comportamento e dificultam diagnóstico de falhas. O padrão
"fixture fornece fábricas, teste constrói cenário" garante
que o leitor do teste compreende exatamente o que está
sendo verificado sem precisar navegar para a fixture.

### BB-XI. Templates como Lei de Implementação

O diretório `src/templates/` contém templates de implementação
que são o guia normativo para toda nova entidade, repositório e
camada de infraestrutura. Templates DEVEM ser consultados ANTES
de qualquer implementação nova e seguidos como referência
obrigatória.

**Estrutura dos templates:**

> *Estado atual — em evolução*. Novos templates e arquétipos
> podem ser adicionados conforme padrões são estabelecidos.

```
src/templates/
├── Domain.Entities/
│   ├── SimpleAggregateRoots/      # AR sealed simples
│   ├── AbstractAggregateRoots/    # AR com herança
│   │   ├── Base/                  # Classe abstrata
│   │   ├── LeafAggregateRoot*/    # Classes concretas
│   │   └── Enums/                 # Enums do domínio
│   ├── CompositeAggregateRoots/   # AR com coleção filha
│   └── AssociatedAggregateRoots/  # AR com referência
├── Domain/
│   └── Repositories/              # Interfaces IRepository
├── Infra.Data/
│   └── Repositories/              # Repositório abstração
├── Infra.Data.PostgreSql/
│   ├── DataModels/                # Modelos de persistência
│   ├── Factories/                 # Entity↔DataModel
│   ├── Repositories/              # Repositório PostgreSQL
│   ├── Mappers/                   # DataModelMapper
│   ├── Adapters/                  # Adapters
│   ├── Connections/               # Conexões tipadas
│   └── UnitOfWork/                # UoW tipado
└── Infra.CrossCutting.Bootstrapper/
    └── Startup.cs                 # Configuração DI
```

**Quatro arquétipos de entidade:**

Toda nova entidade DEVE seguir um dos quatro arquétipos
definidos nos templates:

1. **SimpleAggregateRoot** — AR `sealed` sem herança nem
   coleções filhas. Cenário mais comum, referência principal.
2. **AbstractAggregateRoot** — AR abstrata com classes
   concretas `sealed` (leaf types). Para famílias de
   entidades com propriedades e validações compartilhadas.
3. **CompositeAggregateRoot** — AR `sealed` que gerencia
   coleção de entidades filhas (`CompositeChildEntity`).
   Para agregados com composição.
4. **AssociatedAggregateRoots** — AR com referência a
   outro AR independente (associação, não composição).

**Marcadores LLM nos templates:**

Os templates contêm marcadores estruturados que codificam
regras e orientações para code agents:

| Marcador | Propósito |
|----------|-----------|
| `LLM_RULE` | Regra obrigatória que DEVE ser seguida |
| `LLM_GUIDANCE` | Orientação com explicação detalhada |
| `LLM_NOTE` | Nota contextual ou observação |
| `LLM_ANTIPATTERN` | Padrão proibido com justificativa |

Estes marcadores são a documentação viva das decisões
arquiteturais. Code agents DEVEM ler e seguir cada marcador
ao implementar novas entidades.

**Princípios fundamentais codificados nos templates:**

Os templates codificam os seguintes princípios, que DEVEM
ser respeitados em toda implementação:

*Construtores privados (sealed) ou protegidos (abstract):*

- Classes `sealed`: construtores `private`.
- Classes abstratas: construtores `protected` (filhas
  precisam chamar `base()`).
- Dois construtores obrigatórios: default (vazio, para
  `RegisterNew`) e full (completo, para reconstitução
  e `Clone`).
- Construtores públicos são PROIBIDOS — criam bypass de
  toda validação.

*RegisterNew vs CreateFromExistingInfo:*

- `RegisterNew`: Valida com regras ATUAIS, gera Id e
  versão, retorna `T?` (null se validação falhar).
  DEVE chamar `RegisterNewInternal` (ou
  `RegisterNewBase` em abstratas).
- `CreateFromExistingInfo`: NÃO valida (reconstitução de
  dados persistidos), retorna `T` não-nullable.
  DEVE usar construtor full direto.
- Razão: Regras evoluem (MaxLength 100 → 20). Dados
  antigos válidos na época DEVEM ser reconstituídos
  sem falha. Event Sourcing exige replay sem validação.

*Clone-Modify-Return para alterações:*

- Todo método público de alteração (`Change*`) DEVE
  retornar `T?` (nova instância ou null).
- DEVE chamar `RegisterChangeInternal` que gerencia
  clone, incremento de versão e auditoria.
- O handler DEVE ser `static` para evitar captura de
  closures (performance + segurança).

*Operador `&` (bitwise AND) em validações:*

- Validações DEVEM usar `&` (não `&&`) para executar
  TODAS as checagens e coletar TODAS as mensagens.
- Short-circuit (`&&`) mostra 1 erro por vez (UX ruim).
- Non-short-circuit (`&`) dá feedback completo.

*Metadata como fonte única de verdade:*

- Cada entidade DEVE ter classe estática
  `{EntityName}Metadata` com propriedades de regras:
  `{Property}IsRequired`, `{Property}MinLength`,
  `{Property}MaxLength`, etc.
- Nomenclatura obrigatória:
  `<PropertyName><ConstraintType>` sem underscores.
- Camadas externas (API, UI) DEVEM ler regras de
  validação diretamente do Metadata da entidade.
- `Change*Metadata()` para customização no startup
  (multi-tenancy). Thread-unsafe em runtime — usar
  Strategy Pattern para variações por request.

*Input objects como `readonly record struct`:*

- Todo parâmetro de factory method DEVE ser um
  `readonly record struct` dedicado.
- Benefícios: zero alocação (stack), imutabilidade,
  igualdade por valor, evoluível sem breaking changes.
- Factories customizáveis por tenant via IoC (ex:
  `BrazilFactory` vs `SpainFactory`).

*Métodos *Internal (orquestração interna):*

- `private` em classes `sealed`, `protected` em
  abstratas.
- Recebem parâmetros diretos (não Input objects).
- Orquestram múltiplas chamadas `Set*` com operador `&`.
- Operam no clone — seguro falhar parcialmente.

*Métodos Set* (validação + atribuição atômica):*

- Sempre `private`.
- Validam propriedade única via `Validate*`.
- Atribuem SOMENTE após validação bem-sucedida.
- Nullability do parâmetro segue a da propriedade
  (após `Validate*` com `IsRequired=true`, valor
  garantido não-null).

*Validação estática (Validate*):*

- Métodos `public static`, parâmetros sempre nullable.
- Usam `ValidationUtils` (`ValidateIsRequired`,
  `ValidateMinLength`, `ValidateMaxLength`).
- `CreateMessageCode<T>` para códigos padronizados:
  `{EntityName}.{PropertyName}`.
- Resultados em variáveis intermediárias nomeadas.
- Usam `executionContext.TimeProvider` para datas
  (nunca `DateTime.Now`).

*Coleções filhas (CompositeAggregateRoot):*

- Campo `private readonly List<T>` inicializado
  com `= []`.
- Propriedade `IReadOnlyList<T>` via `.AsReadOnly()`.
- Construtor faz cópia defensiva:
  `_collection = [.. children]`.
- Processamento item a item:
  `Process{Entity}For{Operation}Internal`.
- Validação específica por operação:
  `Validate{Entity}For{Operation}Internal`.
- Coleções filhas NÃO têm `Set*` — gerenciadas
  exclusivamente pelo AR.

*Aggregate roots associados:*

- Propriedade nullable (`ReferencedAggregateRoot?`).
- Metadata apenas `IsRequired` (AR tem validação
  própria independente).
- TEM `Set*` privado (diferente de coleções).

*Herança (AbstractAggregateRoot):*

- Classe abstrata encapsula seu próprio registro via
  `RegisterNewBase<TConcreteType, TInput>(...)`.
- Classe pai controla validação — filho não pode
  bypassar.
- `CreateFromExistingInfo` é responsabilidade EXCLUSIVA
  da classe concreta (abstrata NUNCA tem).
- `IsValidConcreteInternal` abstract — filha DEVE
  implementar validação das próprias propriedades.
- Composição de validação estática:
  `Child.IsValid = Parent.IsValid & Child.Validate*`.
- LSP garantido por design: `*Internal` protegido
  controla operações completas; `Set*` privado
  impede filha de criar estado inconsistente.

**Camada de infraestrutura (templates):**

- **DataModel**: Herda `DataModelBase`, propriedades
  com `{ get; set; }`, sem validação nem lógica.
- **Factory Entity→DataModel**: Converte entidade de
  domínio para modelo de persistência. Base factory
  mapeia `EntityInfo`.
- **Factory DataModel→Entity**: Reconstrói entidade via
  `CreateFromExistingInfo` — sem validação.
- **Repository**: Converte entre domínio e DataModel,
  delega para DataModel repository.
- **Bootstrapper**: Registra `ExecutionContext` como
  `Scoped` (por request HTTP), extraindo headers
  (`X-Correlation-Id`, `X-Tenant-Code`,
  `X-Execution-User`, `X-Execution-Origin`,
  `X-Business-Operation-Code`).

**Razão**: Templates são a codificação executável das
decisões arquiteturais do Bedrock. Documentação textual
desatualiza; templates compiláveis não. Cada marcador
`LLM_RULE` é uma decisão que foi validada, testada e
documentada com exemplos concretos. Seguir os templates
garante consistência entre todas as implementações e
elimina ambiguidade sobre como aplicar os princípios
abstratos desta constituição.

### Grafo de Dependências

> *Estado atual — em evolução*. Novos BuildingBlocks podem ser
> adicionados e dependências ajustadas conforme a arquitetura
> amadurece. O princípio imutável é: **dependências DEVEM fluir
> de implementação para abstração, nunca o inverso, e
> dependências circulares são sempre proibidas.**

```
Core (zero dependências externas)
  ↑
Domain.Entities (← Core)
  ↑
Domain (← Core, Domain.Entities)
  ↑
Observability (← Core)
  ↑
Data (← Core, Domain, Domain.Entities, Observability)
  ↑
Persistence.Abstractions (← Core)
  ↑
Persistence.PostgreSql (← Core, Domain.Entities, Domain,
                          Observability, Persistence.Abstractions)

Serialization.Abstractions (← Core)
  ↑
Serialization.* (← Core, Serialization.Abstractions)

Testing (todas as dependências — infraestrutura de teste)
```

Este grafo reflete o estado atual dos BuildingBlocks. Ao
adicionar novos blocos, atualizar este grafo e garantir que
as invariantes de direção de dependência sejam respeitadas.

## Restrições Técnicas

- **Framework**: .NET 10.0
- **Namespace base**: `Bedrock`
- **Linguagem**: C#
- **Análise estática**: SonarCloud (issues DEVEM ser resolvidas ou
  justificadas como falso positivo)
- **Análise arquitetural**: Roslyn analyzers (conjunto de regras
  em evolução: DE001+ para Domain Entities, CS001+ para Code Style)
- **Diagramas**: Mermaid (obrigatório em issues e documentação
  técnica)
- **Gestão de tarefas**: GitHub Issues (ver seção
  *Gestão de Issues e Rastreabilidade*)
- **Branching e merge**: ver seção
  *Gestão de Issues e Rastreabilidade*

## Fluxo de Desenvolvimento

O ciclo de vida de uma mudança DEVE seguir o pipeline:

```
backlog → ready → in-progress → review → approved → merged
```

1. Implementar código e testes (usar `dotnet build` e
   `dotnet test <projeto>` para feedback rápido).
2. Commits intermediários na branch conforme necessário.
3. Pipeline local DEVE passar antes de abrir PR (100%
   cobertura, 100% mutação, zero issues SonarCloud aplicáveis).
4. PR criado com `gh pr create` (body contém `Closes #<issue>`).
5. Pipeline CI DEVE passar.
6. Squash merge com `gh pr merge --squash --delete-branch`.
7. Atualização da branch local (`git checkout main && git pull`).

## Gestão de Issues e Rastreabilidade

Toda unidade de trabalho no projeto Bedrock DEVE ser rastreada por
uma issue no GitHub. Issues são a fonte única de verdade para gestão
de tarefas, priorização e histórico de decisões.

### Ferramenta única: GitHub CLI (`gh`)

- Todas as operações de issue, PR e pipeline DEVEM usar a CLI `gh`.
- Comandos padrão:
  - `gh issue list` — listar issues.
  - `gh issue view <number>` — ver detalhes.
  - `gh issue create` — criar nova issue.
  - `gh issue close <number>` — fechar issue.
  - `gh label list` — listar labels disponíveis.

### Campo Type (obrigatório)

Toda issue DEVE ter o campo **Type** definido. Os tipos disponíveis
são:

| Type | Descrição |
|------|-----------|
| `Bug` | Problema inesperado ou comportamento incorreto |
| `Feature` | Nova funcionalidade ou melhoria |
| `Task` | Tarefa específica de trabalho |

> *Nota*: O campo Type NÃO pode ser definido via `gh issue create`
> ou `gh issue edit`. DEVE ser definido via API GraphQL após a
> criação da issue.

### Taxonomia de Labels

Labels DEVEM seguir a taxonomia padronizada do projeto, organizada
em quatro categorias com prefixos:

**Tipo (`type:`)** — classifica a natureza do trabalho:

| Label | Descrição |
|-------|-----------|
| `type:feature` | Nova funcionalidade |
| `type:refactor` | Refatoração de código |
| `type:migration` | Migração de código legado |
| `type:test` | Testes |
| `type:chore` | Manutenção e tarefas auxiliares |

**Componente (`component:`)** — identifica o BuildingBlock afetado:

> *Estado atual — em evolução*. Novos componentes são adicionados
> conforme BuildingBlocks são criados.

| Label | BuildingBlock |
|-------|---------------|
| `component:core` | BuildingBlocks.Core |
| `component:domain` | BuildingBlocks.Domain |
| `component:data` | BuildingBlocks.Data |
| `component:persistence` | BuildingBlocks.Persistence |
| `component:serialization` | BuildingBlocks.Serialization |
| `component:observability` | BuildingBlocks.Observability |

**Prioridade (`priority:`)** — define urgência:

| Label | Descrição |
|-------|-----------|
| `priority:high` | Alta prioridade |
| `priority:medium` | Média prioridade |
| `priority:low` | Baixa prioridade |

**Status (`status:`)** — rastreia o estágio no pipeline:

| Label | Estágio | Descrição |
|-------|---------|-----------|
| `status:backlog` | Upstream | Aguardando priorização |
| `status:ready` | Upstream | Pronto para iniciar (refinado) |
| `status:in-progress` | Middlestream | Em desenvolvimento |
| `status:review` | Middlestream | PR aberto, aguardando review |
| `status:approved` | Downstream | Aprovado, pronto para merge |
| `status:blocked` | — | Bloqueado por dependência |

Labels padrão do GitHub (`bug`, `documentation`, `enhancement`,
`question`, etc.) também estão disponíveis como complemento.

### Branch por Issue

Toda issue DEVE ter sua própria branch, nomeada no formato:

```
<type>/<issue-number>-<descricao>
```

Exemplos: `feature/42-add-value-objects`,
`migration/15-core-execution-context`.

### Ciclo de Vida da Issue

O ciclo de vida de uma issue segue o pipeline:

```
Upstream          Middlestream              Downstream
─────────────────────────────────────────────────────────
backlog → ready → in-progress → review → approved → merged
```

1. **Issue criada** → `status:backlog`.
2. **Refinada/priorizada** → `status:ready`.
3. **Branch criada, desenvolvimento iniciado** → `status:in-progress`.
4. **PR aberto** → `status:review` (PR DEVE conter
   `Closes #<issue>` no body para auto-close).
5. **PR aprovado** → `status:approved`.
6. **Merge** → Issue fechada automaticamente.

### Convenções de PR

- PR DEVE conter `Closes #<issue>` no body para fechamento
  automático da issue no merge.
- Merge DEVE ser squash merge:
  `gh pr merge <number> --squash --delete-branch`.
- Após merge, atualizar branch local:
  `git checkout main && git pull`.
- Branches locais obsoletas DEVEM ser removidas:
  `git branch -d <branch-name>`.

**Razão**: Rastreabilidade completa garante que nenhuma mudança
existe sem motivação documentada. A taxonomia de labels e o ciclo
de vida padronizado permitem visibilidade do estado de todo o
projeto a qualquer momento. O uso exclusivo de `gh` CLI elimina
fricção e garante consistência.

## Governance

Esta constituição é o documento normativo supremo do projeto Bedrock.
Em caso de conflito entre esta constituição e qualquer outro documento
(CLAUDE.md, READMEs, ADRs), esta constituição prevalece.

- **Emendas** DEVEM ser documentadas com justificativa, aprovadas
  pelo mantenedor principal, e acompanhadas de plano de migração
  quando afetam código existente.
- **Versionamento** segue Semantic Versioning:
  - MAJOR: Remoção ou redefinição incompatível de princípios.
  - MINOR: Adição de princípio ou expansão material de orientação.
  - PATCH: Correções de redação, clarificações, ajustes não-semânticos.
- **Compliance**: Todo PR DEVE ser verificável contra os princípios
  desta constituição. Violações DEVEM ser justificadas na
  Complexity Tracking do plano de implementação.
- **Orientação operacional**: O arquivo `CLAUDE.md` na raiz do
  projeto contém orientações operacionais detalhadas para o code
  agent, derivadas desta constituição.

**Version**: 1.10.1 | **Ratified**: 2026-02-08 | **Last Amended**: 2026-02-09
