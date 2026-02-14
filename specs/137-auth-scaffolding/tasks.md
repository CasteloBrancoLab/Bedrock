# Tasks: Auth - Estrutura dos Projetos (Scaffolding)

**Input**: Design documents from `/specs/137-auth-scaffolding/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md, quickstart.md

**Tests**: Não solicitados na especificação. Nenhuma tarefa de teste incluída.

**Organization**: Tasks organizadas por user story para implementação e validação independentes.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Pode executar em paralelo (arquivos diferentes, sem dependências)
- **[Story]**: User story a que pertence (US1, US2, US3, US4)
- Inclui caminhos exatos de arquivo nas descrições

---

## Phase 1: User Story 1 - Criar Projetos src do Auth (Priority: P1)

**Goal**: Criar os 5 projetos src em `src/ShopDemo/Auth/` com referências corretas entre camadas e aos BuildingBlocks

**Independent Test**: `dotnet build Bedrock.sln` compila sem erros com todos os projetos src Auth

- [ ] T001 [P] [US1] Criar `src/ShopDemo/Auth/Domain.Entities/ShopDemo.Auth.Domain.Entities.csproj` com referências a `ShopDemo.Core.Entities` e `Bedrock.BuildingBlocks.Domain.Entities`
- [ ] T002 [P] [US1] Criar `src/ShopDemo/Auth/Domain.Entities/GlobalUsings.cs`
- [ ] T003 [P] [US1] Criar `src/ShopDemo/Auth/Application/ShopDemo.Auth.Application.csproj` com referência a `ShopDemo.Auth.Domain.Entities`
- [ ] T004 [P] [US1] Criar `src/ShopDemo/Auth/Application/GlobalUsings.cs`
- [ ] T005 [P] [US1] Criar `src/ShopDemo/Auth/Infra.Data/ShopDemo.Auth.Infra.Data.csproj` com referências a `ShopDemo.Auth.Domain.Entities` e `Bedrock.BuildingBlocks.Data`
- [ ] T006 [P] [US1] Criar `src/ShopDemo/Auth/Infra.Data/GlobalUsings.cs`
- [ ] T007 [P] [US1] Criar `src/ShopDemo/Auth/Infra.Data.PostgreSql/ShopDemo.Auth.Infra.Data.PostgreSql.csproj` com referências a `ShopDemo.Auth.Infra.Data`, `ShopDemo.Auth.Domain.Entities` e `Bedrock.BuildingBlocks.Persistence.PostgreSql`
- [ ] T008 [P] [US1] Criar `src/ShopDemo/Auth/Infra.Data.PostgreSql/GlobalUsings.cs`
- [ ] T009 [P] [US1] Criar `src/ShopDemo/Auth/Api/ShopDemo.Auth.Api.csproj` com referências a `ShopDemo.Auth.Application` e `Bedrock.BuildingBlocks.Observability`
- [ ] T010 [P] [US1] Criar `src/ShopDemo/Auth/Api/GlobalUsings.cs`
- [ ] T011 [US1] Adicionar os 5 projetos src à solution `Bedrock.sln` via `dotnet sln add`
- [ ] T012 [US1] Verificar compilação: executar `dotnet build Bedrock.sln` e confirmar sucesso

**Checkpoint**: Os 5 projetos src Auth compilam e estão na solution.

---

## Phase 2: User Story 2 - Criar Projetos de Testes Unitários (Priority: P2)

**Goal**: Criar 5 projetos de testes unitários em relação 1:1 com os projetos src

**Independent Test**: `dotnet test Bedrock.sln` executa sem erros

- [ ] T013 [P] [US2] Criar `tests/UnitTests/ShopDemo/Auth/Domain.Entities/ShopDemo.UnitTests.Auth.Domain.Entities.csproj` com referências a `ShopDemo.Auth.Domain.Entities` e `Bedrock.BuildingBlocks.Testing`
- [ ] T014 [P] [US2] Criar `tests/UnitTests/ShopDemo/Auth/Application/ShopDemo.UnitTests.Auth.Application.csproj` com referências a `ShopDemo.Auth.Application` e `Bedrock.BuildingBlocks.Testing`
- [ ] T015 [P] [US2] Criar `tests/UnitTests/ShopDemo/Auth/Infra.Data/ShopDemo.UnitTests.Auth.Infra.Data.csproj` com referências a `ShopDemo.Auth.Infra.Data` e `Bedrock.BuildingBlocks.Testing`
- [ ] T016 [P] [US2] Criar `tests/UnitTests/ShopDemo/Auth/Infra.Data.PostgreSql/ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.csproj` com referências a `ShopDemo.Auth.Infra.Data.PostgreSql` e `Bedrock.BuildingBlocks.Testing`
- [ ] T017 [P] [US2] Criar `tests/UnitTests/ShopDemo/Auth/Api/ShopDemo.UnitTests.Auth.Api.csproj` com referências a `ShopDemo.Auth.Api` e `Bedrock.BuildingBlocks.Testing`
- [ ] T018 [US2] Adicionar os 5 projetos de teste à solution `Bedrock.sln` via `dotnet sln add`
- [ ] T019 [US2] Verificar testes: executar `dotnet test Bedrock.sln` e confirmar sucesso

**Checkpoint**: Os 5 projetos de teste compilam, estão na solution e `dotnet test` passa.

---

## Phase 3: User Story 3 - Criar Projetos de Testes de Mutação (Priority: P3)

**Goal**: Criar 5 configurações `stryker-config.json` com thresholds de 100%

**Independent Test**: Cada `stryker-config.json` é JSON válido e referencia corretamente src e teste

- [ ] T020 [P] [US3] Criar `tests/MutationTests/ShopDemo/Auth/Domain.Entities/stryker-config.json` referenciando `ShopDemo.Auth.Domain.Entities.csproj` e `ShopDemo.UnitTests.Auth.Domain.Entities.csproj`
- [ ] T021 [P] [US3] Criar `tests/MutationTests/ShopDemo/Auth/Application/stryker-config.json` referenciando `ShopDemo.Auth.Application.csproj` e `ShopDemo.UnitTests.Auth.Application.csproj`
- [ ] T022 [P] [US3] Criar `tests/MutationTests/ShopDemo/Auth/Infra.Data/stryker-config.json` referenciando `ShopDemo.Auth.Infra.Data.csproj` e `ShopDemo.UnitTests.Auth.Infra.Data.csproj`
- [ ] T023 [P] [US3] Criar `tests/MutationTests/ShopDemo/Auth/Infra.Data.PostgreSql/stryker-config.json` referenciando `ShopDemo.Auth.Infra.Data.PostgreSql.csproj` e `ShopDemo.UnitTests.Auth.Infra.Data.PostgreSql.csproj`
- [ ] T024 [P] [US3] Criar `tests/MutationTests/ShopDemo/Auth/Api/stryker-config.json` referenciando `ShopDemo.Auth.Api.csproj` e `ShopDemo.UnitTests.Auth.Api.csproj`

**Checkpoint**: Todos os `stryker-config.json` são válidos com thresholds 100/100/100.

---

## Phase 4: User Story 4 - Integrar na Solution e Pipeline (Priority: P4)

**Goal**: Registrar Auth no teste de arquitetura e validar pipeline completa

**Independent Test**: `./scripts/pipeline.sh` passa completamente

- [ ] T025 [US4] Adicionar path `ShopDemo.Auth.Domain.Entities` ao `DomainEntitiesArchFixture.GetProjectPaths()` em `tests/ArchitectureTests/Templates/Domain.Entities/Fixtures/DomainEntitiesArchFixture.cs`
- [ ] T026 [US4] Executar `./scripts/pipeline.sh` e confirmar que a pipeline passa completamente com a estrutura vazia
- [ ] T027 [US4] Executar validação quickstart: `dotnet sln list | grep -i Auth` confirma 10 projetos, `find src/ShopDemo/Auth -name "*.csproj"` confirma 5, `find tests -path "*/ShopDemo/Auth/*" -name "*.csproj"` confirma 5, `find tests -path "*/ShopDemo/Auth/*" -name "stryker-config.json"` confirma 5

**Checkpoint**: Pipeline completa passa. Estrutura pronta para issue #138.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (US1)**: Sem dependências — iniciar imediatamente
- **Phase 2 (US2)**: Depende de Phase 1 (projetos src são referenciados pelos testes)
- **Phase 3 (US3)**: Depende de Phase 1 e Phase 2 (stryker referencia ambos)
- **Phase 4 (US4)**: Depende de Phase 1, 2 e 3 (validação completa)

### Within Each Phase

- Tarefas marcadas [P] podem executar em paralelo
- Tarefas de verificação (T012, T019, T026, T027) devem executar por último na fase
- `dotnet sln add` (T011, T018) deve executar após todos os .csproj da fase

### Parallel Opportunities

```text
# Phase 1 — Todos os .csproj e GlobalUsings em paralelo:
T001, T002, T003, T004, T005, T006, T007, T008, T009, T010 (10 tarefas em paralelo)
→ T011 (add to solution)
→ T012 (verify build)

# Phase 2 — Todos os .csproj de teste em paralelo:
T013, T014, T015, T016, T017 (5 tarefas em paralelo)
→ T018 (add to solution)
→ T019 (verify test)

# Phase 3 — Todos os stryker-config em paralelo:
T020, T021, T022, T023, T024 (5 tarefas em paralelo)

# Phase 4 — Sequencial:
T025 → T026 → T027
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Completar Phase 1: Criar 5 projetos src
2. **PARAR e VALIDAR**: `dotnet build Bedrock.sln` passa
3. Continuar para Phase 2 (testes), Phase 3 (mutação), Phase 4 (pipeline)

### Incremental Delivery

1. Phase 1 → Build passa → 5 projetos src prontos
2. Phase 2 → Test passa → 5 projetos teste prontos
3. Phase 3 → Stryker configs prontos → 5 configs criadas
4. Phase 4 → Pipeline passa → Estrutura completa

### Referência de Caminhos Relativos

Todos os `.csproj` DEVEM usar caminhos relativos conforme documentado no plan.md:

| De (src) | Para | Caminho Relativo |
|----------|------|-----------------|
| `Domain.Entities` | `ShopDemo.Core.Entities` | `..\..\Core\Entities\ShopDemo.Core.Entities.csproj` |
| `Domain.Entities` | `BB.Domain.Entities` | `..\..\..\..\src\BuildingBlocks\Domain.Entities\Bedrock.BuildingBlocks.Domain.Entities.csproj` |
| `Application` | `Domain.Entities` | `..\Domain.Entities\ShopDemo.Auth.Domain.Entities.csproj` |
| `Infra.Data` | `Domain.Entities` | `..\Domain.Entities\ShopDemo.Auth.Domain.Entities.csproj` |
| `Infra.Data` | `BB.Data` | `..\..\..\..\src\BuildingBlocks\Data\Bedrock.BuildingBlocks.Data.csproj` |
| `Infra.Data.PostgreSql` | `Infra.Data` | `..\Infra.Data\ShopDemo.Auth.Infra.Data.csproj` |
| `Infra.Data.PostgreSql` | `Domain.Entities` | `..\Domain.Entities\ShopDemo.Auth.Domain.Entities.csproj` |
| `Infra.Data.PostgreSql` | `BB.Persistence.PostgreSql` | `..\..\..\..\src\BuildingBlocks\Persistence.PostgreSql\Bedrock.BuildingBlocks.Persistence.PostgreSql.csproj` |
| `Api` | `Application` | `..\Application\ShopDemo.Auth.Application.csproj` |
| `Api` | `BB.Observability` | `..\..\..\..\src\BuildingBlocks\Observability\Bedrock.BuildingBlocks.Observability.csproj` |

| De (teste) | Para | Caminho Relativo |
|------------|------|-----------------|
| `UnitTests/.../Domain.Entities` | `Auth Domain.Entities` | `..\..\..\..\..\..\src\ShopDemo\Auth\Domain.Entities\ShopDemo.Auth.Domain.Entities.csproj` |
| `UnitTests/.../Domain.Entities` | `BB.Testing` | `..\..\..\..\..\..\src\BuildingBlocks\Testing\Bedrock.BuildingBlocks.Testing.csproj` |
| `UnitTests/.../Application` | `Auth Application` | `..\..\..\..\..\..\src\ShopDemo\Auth\Application\ShopDemo.Auth.Application.csproj` |
| `UnitTests/.../Application` | `BB.Testing` | `..\..\..\..\..\..\src\BuildingBlocks\Testing\Bedrock.BuildingBlocks.Testing.csproj` |
| `UnitTests/.../Infra.Data` | `Auth Infra.Data` | `..\..\..\..\..\..\src\ShopDemo\Auth\Infra.Data\ShopDemo.Auth.Infra.Data.csproj` |
| `UnitTests/.../Infra.Data` | `BB.Testing` | `..\..\..\..\..\..\src\BuildingBlocks\Testing\Bedrock.BuildingBlocks.Testing.csproj` |
| `UnitTests/.../Infra.Data.PostgreSql` | `Auth Infra.Data.PostgreSql` | `..\..\..\..\..\..\src\ShopDemo\Auth\Infra.Data.PostgreSql\ShopDemo.Auth.Infra.Data.PostgreSql.csproj` |
| `UnitTests/.../Infra.Data.PostgreSql` | `BB.Testing` | `..\..\..\..\..\..\src\BuildingBlocks\Testing\Bedrock.BuildingBlocks.Testing.csproj` |
| `UnitTests/.../Api` | `Auth Api` | `..\..\..\..\..\..\src\ShopDemo\Auth\Api\ShopDemo.Auth.Api.csproj` |
| `UnitTests/.../Api` | `BB.Testing` | `..\..\..\..\..\..\src\BuildingBlocks\Testing\Bedrock.BuildingBlocks.Testing.csproj` |

### Stryker Config — Caminhos Relativos

De `tests/MutationTests/ShopDemo/Auth/{Layer}/` para `tests/UnitTests/ShopDemo/Auth/{Layer}/`:

```text
../../../../UnitTests/ShopDemo/Auth/{Layer}/ShopDemo.UnitTests.Auth.{Layer}.csproj
```

### Template .csproj (src)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="{relative-path-to-dependency}" />
  </ItemGroup>
</Project>
```

> Nota: `TargetFramework`, `Nullable` e `ImplicitUsings` são herdados de `Directory.Build.props` na raiz.

### Template .csproj (teste)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" />
    <PackageReference Include="xunit" />
    <PackageReference Include="xunit.runner.visualstudio">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
    <PackageReference Include="coverlet.collector">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="{relative-path-to-testing-bb}" />
    <ProjectReference Include="{relative-path-to-src-project}" />
  </ItemGroup>
</Project>
```

---

## Notes

- [P] tasks = arquivos diferentes, sem dependências
- [Story] label mapeia cada tarefa à user story correspondente
- Cada user story é completável e testável independentemente
- Commitar após cada fase ou grupo lógico de tarefas
- Parar em qualquer checkpoint para validar independentemente
- Projetos vazios (sem classes) são válidos — a pipeline DEVE aceitar
