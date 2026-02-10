# ADRs - Domain Entities (DE)

Decisões arquiteturais relacionadas a **entidades de domínio**, agregados e value objects.

## Lista de ADRs

### Fundamentos de Entidades

| ADR | Título | Status |
|-----|--------|--------|
| [DE-001](./DE-001-entidades-devem-ser-sealed.md) | Entidades Devem Ser Sealed | Aceita |
| [DE-002](./DE-002-construtores-privados-com-factory-methods.md) | Construtores Privados com Factory Methods | Aceita |
| [DE-003](./DE-003-imutabilidade-controlada-clone-modify-return.md) | Imutabilidade Controlada (Clone-Modify-Return) | Aceita |
| [DE-004](./DE-004-estado-invalido-nunca-existe-na-memoria.md) | Estado Inválido Nunca Existe na Memória | Aceita |
| [DE-005](./DE-005-aggregateroot-deve-implementar-iaggregateroot.md) | AggregateRoot Deve Implementar IAggregateRoot | Aceita |

### Validação

| ADR | Título | Status |
|-----|--------|--------|
| [DE-006](./DE-006-operador-bitwise-and-para-validacao-completa.md) | Operador Bitwise AND para Validação Completa | Aceita |
| [DE-007](./DE-007-retorno-nullable-vs-result-pattern.md) | Retorno Nullable vs Result Pattern | Aceita |
| [DE-008](./DE-008-excecoes-vs-retorno-nullable.md) | Exceções vs Retorno Nullable | Aceita |
| [DE-009](./DE-009-metodos-validate-publicos-e-estaticos.md) | Métodos Validate* Públicos e Estáticos | Aceita |
| [DE-010](./DE-010-validationutils-para-validacoes-padrao.md) | ValidationUtils para Validações Padrão | Aceita |
| [DE-011](./DE-011-parametros-validate-nullable-por-design.md) | Parâmetros Validate* Nullable por Design | Aceita |

### Metadados de Validação

| ADR | Título | Status |
|-----|--------|--------|
| [DE-012](./DE-012-metadados-estaticos-vs-data-annotations.md) | Metadados Estáticos vs Data Annotations | Aceita |
| [DE-013](./DE-013-nomenclatura-de-metadados.md) | Nomenclatura de Metadados (PropertyName + ConstraintType) | Aceita |
| [DE-014](./DE-014-inicializacao-inline-de-metadados.md) | Inicialização Inline de Metadados | Aceita |
| [DE-015](./DE-015-customizacao-de-metadados-apenas-no-startup.md) | Customização de Metadados Apenas no Startup | Aceita |
| [DE-016](./DE-016-single-source-of-truth-para-regras-de-validacao.md) | Single Source of Truth para Regras de Validação | Aceita |

### Criação e Reconstitution

| ADR | Título | Status |
|-----|--------|--------|
| [DE-017](./DE-017-separacao-registernew-vs-createfromexistinginfo.md) | Separação RegisterNew vs CreateFromExistingInfo | Aceita |
| [DE-018](./DE-018-reconstitution-nao-valida-dados.md) | Reconstitution Não Valida Dados | Aceita |
| [DE-019](./DE-019-input-objects-pattern.md) | Input Objects Pattern (readonly record struct) | Aceita |
| [DE-020](./DE-020-dois-construtores-privados.md) | Dois Construtores Privados (Vazio e Completo) | Aceita |

### Estrutura Interna de Métodos

| ADR | Título | Status |
|-----|--------|--------|
| [DE-021](./DE-021-metodos-publicos-vs-metodos-internos.md) | Métodos Públicos vs Métodos Internos (*Internal) | Aceita |
| [DE-022](./DE-022-metodos-set-privados.md) | Métodos Set* Privados | Aceita |
| [DE-023](./DE-023-register-internal-chamado-uma-unica-vez.md) | Register*Internal Chamado Uma Única Vez | Aceita |
| [DE-024](./DE-024-metodo-publico-nunca-chama-outro-publico.md) | Método Público Nunca Chama Outro Método Público | Aceita |
| [DE-025](./DE-025-variaveis-intermediarias-para-legibilidade-e-debug.md) | Variáveis Intermediárias para Legibilidade e Debug | Aceita |

### Propriedades e Estado

| ADR | Título | Status |
|-----|--------|--------|
| [DE-026](./DE-026-propriedades-derivadas-persistidas-vs-calculadas.md) | Propriedades Derivadas: Persistidas vs Calculadas | Aceita |
| [DE-027](./DE-027-entidades-nao-tem-dependencias-externas.md) | Entidades Não Têm Dependências Externas | Aceita |

### Contexto de Execução

| ADR | Título | Status |
|-----|--------|--------|
| [DE-028](./DE-028-executioncontext-explicito.md) | ExecutionContext Explícito (não Implícito) | Aceita |
| [DE-029](./DE-029-timeprovider-encapsulado-no-executioncontext.md) | TimeProvider Encapsulado no ExecutionContext | Aceita |
| [DE-030](./DE-030-message-codes-com-createmessagecode.md) | Message Codes com CreateMessageCode<T> | Aceita |

### EntityInfo e Auditoria

| ADR | Título | Status |
|-----|--------|--------|
| [DE-031](./DE-031-entityinfo-gerenciado-pela-classe-base.md) | EntityInfo Gerenciado pela Classe Base | Aceita |
| [DE-032](./DE-032-optimistic-locking-com-entityversion.md) | Optimistic Locking com EntityVersion | Aceita |

### Antipadrões Documentados

| ADR | Título | Status |
|-----|--------|--------|
| [DE-033](./DE-033-antipadrao-readonly-struct-para-entidades.md) | Antipadrão: Readonly Struct para Entidades | Aceita |
| [DE-034](./DE-034-antipadrao-mutabilidade-direta.md) | Antipadrão: Mutabilidade Direta | Aceita |
| [DE-035](./DE-035-antipadrao-construtor-que-valida.md) | Antipadrão: Construtor que Valida | Aceita |

### Aggregate Roots Compostos (Entidades Pai-Filho)

| ADR | Título | Status |
|-----|--------|--------|
| [DE-036](./DE-036-colecoes-entidades-filhas-field-privado-list.md) | Coleções de Entidades Filhas com Field Privado List<T> | Aceita |
| [DE-037](./DE-037-propriedade-publica-ireadonlylist-asreadonly.md) | Propriedade Pública Retorna IReadOnlyList via AsReadOnly | Aceita |
| [DE-038](./DE-038-field-colecao-sempre-inicializado.md) | Field de Coleção Sempre Inicializado (Não Nullable) | Aceita |
| [DE-039](./DE-039-defensive-copy-colecoes-construtor.md) | Defensive Copy de Coleções no Construtor | Aceita |
| [DE-040](./DE-040-processamento-entidades-filhas-uma-a-uma.md) | Processamento de Entidades Filhas Uma a Uma | Aceita |
| [DE-041](./DE-041-validacao-entidade-filha-especifica-operacao.md) | Validação de Entidade Filha Específica por Operação | Aceita |
| [DE-042](./DE-042-localizacao-entidade-filha-por-id.md) | Localização de Entidade Filha por Id | Aceita |
| [DE-043](./DE-043-modificacao-entidade-filha-via-metodo-negocio.md) | Modificação de Entidade Filha Via Método de Negócio Dela | Aceita |
| [DE-044](./DE-044-antipadrao-colecoes-sem-metodo-set.md) | Antipadrão: Coleções Não Têm Método Set | Aceita |
| [DE-045](./DE-045-validacao-duplicidade-ignora-propria-entidade.md) | Validação de Duplicidade Ignora a Própria Entidade | Aceita |

### Enumerações

| ADR | Título | Status |
|-----|--------|--------|
| [DE-046](./DE-046-convencoes-enumeracoes-dominio.md) | Convenções de Enumerações no Domínio | Aceita |

### Aggregate Roots Abstratos (Hierarquias de Herança)

| ADR | Título | Status |
|-----|--------|--------|
| [DE-047](./DE-047-metodos-set-privados-em-classes-abstratas.md) | Métodos Set* Privados em Classes Abstratas | Aceita |
| [DE-048](./DE-048-metodos-validate-publicos-em-classes-abstratas.md) | Métodos Validate* Públicos em Classes Abstratas | Aceita |
| [DE-049](./DE-049-metodos-internal-protegidos-em-classes-abstratas.md) | Métodos *Internal Protegidos em Classes Abstratas | Aceita |
| [DE-050](./DE-050-classe-abstrata-nao-expoe-metodos-publicos-negocio.md) | Classe Abstrata Não Expõe Métodos Públicos de Negócio | Aceita |
| [DE-051](./DE-051-hierarquia-isvalid-em-classes-abstratas.md) | Hierarquia IsValid em Classes Abstratas | Aceita |
| [DE-052](./DE-052-construtores-protegidos-em-classes-abstratas.md) | Construtores Protegidos em Classes Abstratas | Aceita |
| [DE-053](./DE-053-metadados-validacao-em-classes-abstratas.md) | Metadados de Validação em Classes Abstratas | Aceita |
| [DE-054](./DE-054-heranca-vs-composicao-em-entidades.md) | Herança vs Composição em Entidades de Domínio | Aceita |
| [DE-055](./DE-055-registernewbase-em-classes-abstratas.md) | RegisterNewBase em Classes Abstratas | Aceita |
| [DE-056](./DE-056-classe-abstrata-nao-tem-createfromexistinginfo.md) | Classe Abstrata Não Tem CreateFromExistingInfo | Aceita |

### Aggregate Roots Associadas (Composição 1:1)

| ADR | Título | Status |
|-----|--------|--------|
| [DE-057](./DE-057-metadata-aggregate-roots-associadas-apenas-isrequired.md) | Metadata de Aggregate Roots Associadas - Apenas IsRequired | Aceita |
| [DE-058](./DE-058-padroes-process-validate-set-para-aggregate-roots-associadas.md) | Padrões Process, Validate e Set para Aggregate Roots Associadas | Aceita |

### Estrutura de Metadados

| ADR | Título | Status |
|-----|--------|--------|
| [DE-059](./DE-059-metadata-deve-ser-classe-aninhada.md) | Metadados Devem Ser Classe Aninhada da Entidade | Aceita |

---

## Fonte

Estas ADRs foram derivadas dos comentários `LLM_GUIDANCE`, `LLM_RULE`, `LLM_TEMPLATE` e `LLM_ANTIPATTERN` documentados em:

- [SimpleAggregateRoot.cs](../../../templates/Domain.Entities/SimpleAggregateRoots/SimpleAggregateRoot.cs)
- [CompositeAggregateRoot.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeAggregateRoot.cs)
- [CompositeChildEntity.cs](../../../templates/Domain.Entities/CompositeAggregateRoots/CompositeChildEntity.cs)
- [CategoryType.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Enums/CategoryType.cs)
- [AbstractAggregateRoot.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/Base/AbstractAggregateRoot.cs)
- [LeafAggregateRootTypeA.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/LeafAggregateRootTypeA.cs)
- [LeafAggregateRootTypeB.cs](../../../templates/Domain.Entities/AbstractAggregateRoots/LeafAggregateRootTypeB.cs)
- [PrimaryAggregateRoot.cs](../../../templates/Domain.Entities/AssociatedAggregateRoots/PrimaryAggregateRoot.cs)

## Navegação

- [Voltar para ADRs](../)
- [AGENTS.md](../../../AGENTS.md) - Hub para code agents
