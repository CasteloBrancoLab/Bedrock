using Bedrock.BuildingBlocks.Core.BirthDates;
using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Templates.Domain.Entities.SimpleAggregateRoots.Inputs;

namespace Templates.Domain.Entities.SimpleAggregateRoots;

/*
═══════════════════════════════════════════════════════════════════════════════
LLM_GUIDANCE: Princípios Fundamentais de Entidades de Domínio
═══════════════════════════════════════════════════════════════════════════════

Toda entidade de domínio gerencia 100% do seu próprio estado e regras de negócio.
A entidade é a ÚNICA autoridade sobre a validade do seu estado.

PRINCÍPIOS FUNDAMENTAIS:

1. VALIDAÇÃO OBRIGATÓRIA - Estado inválido NUNCA existe na memória
2. IMUTABILIDADE - Métodos retornam NOVAS instâncias, nunca modificam a existente
3. FACTORY METHODS - Construtores private forçam uso de métodos validados

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Padrões de Criação e Modificação
───────────────────────────────────────────────────────────────────────────────

CRIAÇÃO DE NEGÓCIO (RegisterNew):
- Valida TODOS os dados com regras ATUAIS
- Gera novo Id, versão, timestamps
- Retorna null se validação falhar
- ExecutionContext coleta TODAS as mensagens

RECONSTITUTION (CreateFromExistingInfo):
- Reconstitui dados já persistidos (banco, event store)
- NÃO valida (dados foram validados no passado)
- NUNCA retorna null
- Usado por repositories/ORMs

MODIFICAÇÃO (ChangeName, ChangeBirthDate, etc.):
- Clone-Modify-Return pattern (imutabilidade)
- Valida mudança na cópia
- Retorna nova instância ou null

───────────────────────────────────────────────────────────────────────────────
LLM_TEMPLATE: Padrões de Uso
───────────────────────────────────────────────────────────────────────────────

// CRIAÇÃO
var person = SimpleAggregateRoot.RegisterNew(
    executionContext,
    new RegisterNewInput("John", "Doe", birthDate)
);

if (person == null)
{
    // Validação falhou - consultar executionContext.Messages
}

// MODIFICAÇÃO
var updatedPerson = person.ChangeName(
    executionContext,
    new ChangeNameInput("Jane", "Smith")
);

if (updatedPerson == null)
{
    // person original permanece inalterada
}

// RECONSTITUTION (repositories)
return SimpleAggregateRoot.CreateFromExistingInfo(
    new CreateFromExistingInfoInput(
        dto.EntityInfo,
        dto.FirstName,
        dto.LastName,
        dto.BirthDate
    )
);

───────────────────────────────────────────────────────────────────────────────
LLM_ANTIPATTERN: O Que Não Fazer
───────────────────────────────────────────────────────────────────────────────

❌ Criar diretamente com construtor
var person = new SimpleAggregateRoot(); // NÃO COMPILA - construtor é private

❌ Modificar propriedades diretamente
person.FirstName = "Jane"; // NÃO COMPILA - setter é private

❌ Usar construtores públicos (permite estado inválido temporário)
❌ Usar setters públicos (permite modificação sem validação)
❌ Lançar exceções para validação de negócio (use nullable return)

───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Validação Completa com Operador & (bitwise AND)
───────────────────────────────────────────────────────────────────────────────

SEMPRE use `&` (bitwise AND), NUNCA `&&` (logical AND) em validações:

bool isSuccess =
    instance.RegisterNewInternal<SimpleAggregateRoot>(executionContext)
    & instance.ChangeNameInternal(executionContext, input.FirstName, input.LastName)
    & instance.ChangeBirthDateInternal(executionContext, input.BirthDate);

RAZÃO: `&` NÃO tem short-circuit - TODAS as validações executam.

✅ COM `&`: Executa todas, coleta TODAS as mensagens (UX melhor)
❌ COM `&&`: Para na primeira falha, usuário vê 1 erro por vez (UX ruim)

BENEFÍCIO: Usuário recebe feedback COMPLETO em uma única tentativa.

═══════════════════════════════════════════════════════════════════════════════
*/
/*
───────────────────────────────────────────────────────────────────────────────
LLM_RULE: Classes de Entidade DEVEM Ser Sealed
───────────────────────────────────────────────────────────────────────────────

Entidades de domínio DEVEM ser sealed para:
- Garantir comportamento previsível (sem override inesperado de validações)
- Facilitar otimização do compilador (inlining, devirtualização)
- Prevenir herança que quebre invariantes de validação

PARA VARIAÇÕES DE COMPORTAMENTO:
✅ Composição (entidades relacionadas dentro do agregado)
✅ Strategy Pattern (comportamentos diferentes injetados via IOC)
✅ Interfaces (para polimorfismo controlado)
❌ Herança (quebra encapsulamento de validação, permite bypass de regras)

RAZÃO TÉCNICA:
Métodos privados de validação (Set*, *Internal) não podem ser override.
Se a classe fosse aberta, subclasses poderiam:
- Adicionar construtores públicos (bypass de validação)
- Sobrescrever métodos públicos com lógica diferente
- Quebrar a garantia de "estado sempre válido"

───────────────────────────────────────────────────────────────────────────────
*/
public sealed class SimpleAggregateRoot
    : EntityBase<SimpleAggregateRoot>,
    IAggregateRoot
{
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Metadata de Validação Estática
    ═══════════════════════════════════════════════════════════════════════════════

    Entidades expõem metadados através de propriedades estáticas públicas ao invés de
    Data Annotations ou reflexão.

    BENEFÍCIOS:
    - Performance: acesso direto (zero overhead) vs reflexão
    - Segurança: sem reflexão sobre tipos internos
    - Clareza: metadados explícitos no código
    - AOT Compatibility: Blazor WASM, Xamarin/MAUI, Unity, Native AOT

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Single Source of Truth
    ───────────────────────────────────────────────────────────────────────────────

    Camadas externas (API, UI) DEVEM ler metadados da entidade para validação antecipada.

    ❌ ANTIPATTERN: API valida com MaxLength=100, Domínio com MaxLength=255
    ✅ PATTERN: API usa SimpleAggregateRootMetadata.FirstNameMaxLength

    ───────────────────────────────────────────────────────────────────────────────
    LLM_TEMPLATE: Uso em Camadas Externas
    ───────────────────────────────────────────────────────────────────────────────

    public class CreatePersonRequest
    {
        [MaxLength(SimpleAggregateRootMetadata.FirstNameMaxLength)]
        public string FirstName { get; set; }

        [MaxLength(SimpleAggregateRootMetadata.LastNameMaxLength)]
        public string LastName { get; set; }
    }

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Customização em Runtime (Startup Only)
    ───────────────────────────────────────────────────────────────────────────────

    Change*Metadata() permite customização em RUNTIME para:
    - Multitenancy (diferentes deployments)
    - Configuração externa (appsettings.json)
    - Políticas de compliance por região

    ⚠️ THREAD-SAFETY: Chamar APENAS no startup, NUNCA durante processamento de requests.

    Para regras diferentes por tenant em runtime, use Strategy Pattern ao invés de
    modificar metadados globais.

    ═══════════════════════════════════════════════════════════════════════════════
    */
    public static class SimpleAggregateRootMetadata
    {
        /*
        ═══════════════════════════════════════════════════════════════════════════════
        LLM_RULE: Inline Initialization de Metadados
        ═══════════════════════════════════════════════════════════════════════════════

        SEMPRE inicialize metadados inline (não em construtores estáticos).

        ✅ BENEFÍCIOS:
        - Valor visível ao lado da declaração
        - Code review mais rápido
        - Sem scroll para encontrar inicialização

        Agrupe metadados por propriedade (FirstName, LastName, etc.)

        ═══════════════════════════════════════════════════════════════════════════════
        */

        // Fields
        private static readonly Lock _lockObject = new();

        /*
        ═══════════════════════════════════════════════════════════════════════════════
        LLM_RULE: Convenção de Nomenclatura - <PropertyName><ConstraintType>
        ═══════════════════════════════════════════════════════════════════════════════

        FORMATO OBRIGATÓRIO: <PropertyName><ConstraintType>

        Exemplos:
        ✅ FirstNameIsRequired, FirstNameMinLength, FirstNameMaxLength
        ✅ BirthDateMinAgeInYears, BirthDateMaxAgeInYears

        ❌ FirstName_IsRequired (sem underscores)
        ❌ FirstNameRequired (sem omitir "Is")
        ❌ FNameMaxLength (sem abreviações)

        ───────────────────────────────────────────────────────────────────────────────
        LLM_GUIDANCE: Por Que Este Padrão É Crítico
        ───────────────────────────────────────────────────────────────────────────────

        1. REPRODUTIBILIDADE: LLMs geram novos metadados corretamente
        2. ANÁLISE ESTÁTICA: Roslyn infere regras automaticamente
           - "MinLength" → valida valor >= 0 e <= MaxLength
           - "IsRequired" → valida tipo bool
        3. SIMPLICIDADE: Sem attributes, configuração externa ou reflexão

        ───────────────────────────────────────────────────────────────────────────────
        LLM_TEMPLATE: Tipos de Constraints Suportados
        ───────────────────────────────────────────────────────────────────────────────

        BOOLEANOS:
        - IsRequired → Propriedade obrigatória

        NUMÉRICOS (comprimento):
        - MinLength, MaxLength → Strings

        NUMÉRICOS (idade/tempo):
        - MinAgeInYears, MaxAgeInYears → Datas

        NUMÉRICOS (valores):
        - MinValue, MaxValue → Tipos numéricos

        ───────────────────────────────────────────────────────────────────────────────
        LLM_TEMPLATE: Adicionando Novos Metadados
        ───────────────────────────────────────────────────────────────────────────────

        // Email (agrupe por propriedade, ordem alfabética de constraints)
        public static bool EmailIsRequired { get; private set; } = true;
        public static int EmailMinLength { get; private set; } = 5;
        public static int EmailMaxLength { get; private set; } = 255;
        public static string EmailPattern { get; private set; } = @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$";

        ═══════════════════════════════════════════════════════════════════════════════
        */

        // FirstName
        public static readonly string FirstNamePropertyName = nameof(FirstName);
        public static bool FirstNameIsRequired { get; private set; } = true;
        public static int FirstNameMinLength { get; private set; } = 1;
        public static int FirstNameMaxLength { get; private set; } = 255;

        // LastName
        public static readonly string LastNamePropertyName = nameof(LastName);
        public static bool LastNameIsRequired { get; private set; } = true;
        public static int LastNameMinLength { get; private set; } = 1;
        public static int LastNameMaxLength { get; private set; } = 255;

        // FullName
        public static readonly string FullNamePropertyName = nameof(FullName);
        public static bool FullNameIsRequired { get; private set; } = true;
        public static int FullNameMinLength { get; private set; } = FirstNameMinLength + LastNameMinLength + 1; // Soma dos mínimos + espaço
        public static int FullNameMaxLength { get; private set; } = FirstNameMaxLength + LastNameMaxLength + 1; // Soma dos máximos + espaço

        // BirthDate
        public static readonly string BirthDatePropertyName = nameof(BirthDate);
        public static bool BirthDateIsRequired { get; private set; } = true;
        public static int BirthDateMinAgeInYears { get; private set; }
        public static int BirthDateMaxAgeInYears { get; private set; } = 150;

        /*
        ═══════════════════════════════════════════════════════════════════════════════
        LLM_RULE: Métodos Change*Metadata() - Startup Only
        ═══════════════════════════════════════════════════════════════════════════════

        Alteram metadados GLOBALMENTE. Usar APENAS no STARTUP para:
        ✅ Deployment específico (on-premises vs cloud)
        ✅ Compliance regional (GDPR, LGPD)
        ✅ Planos comerciais (básico, premium, enterprise)

        ❌ NÃO usar para regras diferentes por tenant em runtime
        ❌ NÃO chamar durante processamento de requests

        ───────────────────────────────────────────────────────────────────────────────
        LLM_TEMPLATE: Validação no Startup
        ───────────────────────────────────────────────────────────────────────────────

        var config = _configuration.GetSection("ValidationRules");
        int minLength = config.GetValue<int>("FirstNameMinLength");
        int maxLength = config.GetValue<int>("FirstNameMaxLength");

        if (minLength > maxLength)
            throw new InvalidOperationException(
                $"FirstNameMinLength ({minLength}) cannot exceed MaxLength ({maxLength})"
            );

        SimpleAggregateRootMetadata.ChangeFirstNameMetadata(true, minLength, maxLength);

        ───────────────────────────────────────────────────────────────────────────────
        LLM_ANTIPATTERN: Regras Diferentes Por Tenant em Runtime
        ───────────────────────────────────────────────────────────────────────────────

        ❌ NÃO use Change*Metadata() para tenants diferentes no mesmo deployment

        ✅ Use Strategy Pattern:

        public interface ITenantValidationStrategy
        {
            bool ValidateFirstName(string? firstName);
        }

        // Resolva por tenant via DI
        var strategy = _tenantStrategyProvider.GetStrategy(tenantId);

        ═══════════════════════════════════════════════════════════════════════════════
        */
        public static void ChangeFirstNameMetadata(
            bool isRequired,
            int minLength,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                FirstNameIsRequired = isRequired;
                FirstNameMinLength = minLength;
                FirstNameMaxLength = maxLength;
            }
        }

        public static void ChangeLastNameMetadata(
            bool isRequired,
            int minLength,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                LastNameIsRequired = isRequired;
                LastNameMinLength = minLength;
                LastNameMaxLength = maxLength;
            }
        }

        public static void ChangeBirthDateMetadata(
            bool isRequired,
            int minAgeInYears,
            int maxAgeInYears
        )
        {
            lock (_lockObject)
            {
                BirthDateIsRequired = isRequired;
                BirthDateMinAgeInYears = minAgeInYears;
                BirthDateMaxAgeInYears = maxAgeInYears;
            }
        }
    }

    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: EntityInfo - Metadados Herdados de EntityBase
    ═══════════════════════════════════════════════════════════════════════════════

    A propriedade EntityInfo é HERDADA de EntityBase<T> e contém:
    - Id: Identificador único (UUIDv7 com ordenação monotônica)
    - TenantInfo: Informações de multitenancy (Code, Name)
    - EntityChangeInfo: Auditoria (CreatedAt/By, LastChangedAt/By)
    - EntityVersion: Controle de concorrência otimista (monotônico)

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: EntityInfo É Gerenciado Pela Classe Base
    ───────────────────────────────────────────────────────────────────────────────

    A entidade concreta NÃO manipula EntityInfo diretamente:
    ✅ RegisterNewInternal() cria EntityInfo automaticamente
    ✅ RegisterChangeInternal() atualiza versão e timestamps automaticamente
    ✅ CreateFromExistingInfo() recebe EntityInfo já existente

    ❌ NUNCA atribua EntityInfo diretamente na entidade concreta
    ❌ NUNCA incremente EntityVersion manualmente

    ───────────────────────────────────────────────────────────────────────────────
    LLM_GUIDANCE: Optimistic Locking com EntityVersion
    ───────────────────────────────────────────────────────────────────────────────

    EntityVersion é incrementado automaticamente em RegisterChangeInternal().
    Repositórios DEVEM usar para controle de concorrência:

    UPDATE Entities
    SET FirstName = @FirstName, ..., EntityVersion = @NewVersion
    WHERE Id = @Id AND EntityVersion = @ExpectedVersion

    Se nenhuma linha for afetada → ConcurrencyException (outro usuário alterou)

    BENEFÍCIOS:
    - Detecta conflitos de edição simultânea
    - Sem locks pessimistas no banco
    - Versão monotônica evita problemas de clock drift

    ═══════════════════════════════════════════════════════════════════════════════
    */

    // Properties
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_GUIDANCE: Propriedades Derivadas Persistidas vs Calculadas
    ───────────────────────────────────────────────────────────────────────────────

    FullName é DERIVADO de FirstName + LastName, mas é ARMAZENADO (não calculado).

    POR QUE NÃO USAR `public string FullName => $"{FirstName} {LastName}";`?

    1. REGRAS DE COMPOSIÇÃO MUDAM:
       - 2020: "FirstName LastName" (João Silva)
       - 2025: "LastName, FirstName" (Silva, João)
       - Entidades antigas devem manter formato original após reconstitution

    2. RECONSTITUTION SEM LÓGICA:
       - CreateFromExistingInfo() NÃO recalcula valores
       - Se FullName fosse calculado, reconstitution aplicaria regra ATUAL
       - Dados históricos seriam corrompidos silenciosamente

    3. AUDITORIA E COMPLIANCE:
       - LGPD/GDPR exigem preservação exata de dados históricos
       - FullName armazenado = evidência do valor exato naquele momento

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Propriedades Derivadas DEVEM Ser Armazenadas Se Regra Pode Mudar
    ───────────────────────────────────────────────────────────────────────────────

    ARMAZENAR (como FullName):
    ✅ Regra de composição pode mudar no futuro
    ✅ Valor precisa ser auditável/rastreável
    ✅ Reconstitution deve preservar valor original

    CALCULAR (expression body):
    ✅ Regra é IMUTÁVEL e UNIVERSAL (ex: Age = Today - BirthDate)
    ✅ Valor é transitório e não precisa de auditoria
    ✅ Cálculo é determinístico dado o estado atual

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Propriedade Derivada Segue Mesmo Padrão de Validação
    ───────────────────────────────────────────────────────────────────────────────

    FullName é atualizado em ChangeNameInternal() junto com FirstName/LastName.
    NÃO precisa de:
    - Metadados próprios (FullNameIsRequired, etc.) - derivado das partes
    - Método ValidateFullName() - validação das partes é suficiente
    - Método SetFullName() separado - atualizado junto com nome

    ───────────────────────────────────────────────────────────────────────────────
    */
    public string FullName { get; private set; } = string.Empty;

    public BirthDate BirthDate { get; private set; }

    // Constructors
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Dois Construtores Privados
    ═══════════════════════════════════════════════════════════════════════════════

    1. CONSTRUTOR VAZIO - private SimpleAggregateRoot()
       - Usado em RegisterNew() para validação incremental
       - Permite coletar TODAS as mensagens de validação

    2. CONSTRUTOR COMPLETO - private SimpleAggregateRoot(EntityInfo, ...)
       - Usado em CreateFromExistingInfo() para reconstitution
       - Usado em CloneInternal() para imutabilidade (Clone-Modify-Return)
       - Assume valores já validados

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Construtores DEVEM Ser Privados - Encapsulamento Completo
    ───────────────────────────────────────────────────────────────────────────────

    De nada adianta:
    ✅ Criar propriedades para não expor fields
    ✅ Privar os setters das propriedades
    ❌ ...e deixar o construtor público aceitando qualquer coisa

    Construtor público QUEBRA todo o encapsulamento:

    public Person(string firstName) // Público = buraco no encapsulamento
    {
        FirstName = firstName; // Bypass de TODA validação!
    }

    var person = new Person(null); // Compila! Estado inválido criado.

    REGRA: Construtores private + Factory methods = encapsulamento COMPLETO.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Construtor Público com Validação Impede Reconstitution
    ───────────────────────────────────────────────────────────────────────────────

    Construtores públicos que validam PARECEM seguros, mas QUEBRAM reconstitution:

    public Person(string firstName)
    {
        if (firstName.Length > MaxLength) // MaxLength = 20 (regra de 2025)
            throw new ArgumentException("Nome muito longo");
        FirstName = firstName;
    }

    PROBLEMA - Dados históricos se tornam inválidos:

    // 2020: MaxLength era 100, usuário cadastrou "João da Silva Pereira Santos" (30 chars)
    // 2025: MaxLength mudou para 20

    // Repository tenta carregar do banco:
    var dto = _db.Query("SELECT * FROM Persons WHERE Id = @id");
    var person = new Person(dto.FirstName); // 💥 EXCEÇÃO! Nome tem 30 chars, max é 20

    CONSEQUÊNCIAS:
    ❌ Dados válidos no passado não podem ser reconstituídos
    ❌ Event sourcing quebra (eventos históricos falham replay)
    ❌ Migração de dados impossível sem "limpar" dados antigos
    ❌ Sistema para de funcionar quando regras mudam

    SOLUÇÃO - Separar criação de reconstitution:
    ✅ RegisterNew() - valida com regras ATUAIS (para dados NOVOS)
    ✅ CreateFromExistingInfo() - NÃO valida (para dados EXISTENTES)
    ✅ Ambos usam construtores PRIVADOS

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Imutabilidade Controlada (setters private, não readonly struct)
    ───────────────────────────────────────────────────────────────────────────────

    Setters private permitem:
    ✅ Validação incremental (propriedade por propriedade)
    ✅ Clonagem simples sem reflexão
    ✅ Feedback completo de erros (operador &)

    Imutabilidade EXTERNA garantida por:
    - Setters private
    - Construtores private
    - Métodos públicos retornam novas instâncias

    ───────────────────────────────────────────────────────────────────────────────
    LLM_ANTIPATTERN: Não Use Readonly Struct Para Entidades
    ───────────────────────────────────────────────────────────────────────────────

    ❌ readonly record struct força validação "tudo ou nada"
    ❌ Usuário vê apenas 1 erro por vez
    ❌ Requer tipos auxiliares complexos

    ═══════════════════════════════════════════════════════════════════════════════
    */
    private SimpleAggregateRoot()
    {
    }

    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_RULE: Por Que Construtor Completo NÃO Valida
    ═══════════════════════════════════════════════════════════════════════════════

    RAZÕES:

    1. LIMITAÇÃO TÉCNICA - Construtor sempre retorna instância, não pode retornar null
    2. PERFORMANCE - Exceções são caras, validação de negócio é esperada (não excepcional)
    3. CONTEXTO - Regras variam por tenant/usuário/feature flag (ExecutionContext)
    4. DADOS HISTÓRICOS - Regras mudam ao longo do tempo, dados antigos permanecem válidos
    5. EVENT SOURCING - Eventos passados são imutáveis, devem ser aplicáveis sempre
    6. AUDITORIA - Compliance exige preservação exata de dados históricos

    ───────────────────────────────────────────────────────────────────────────────
    LLM_GUIDANCE: Separação de Responsabilidades
    ───────────────────────────────────────────────────────────────────────────────

    CONSTRUTOR COMPLETO:
    - Apenas atribui propriedades
    - Assume valores já validados
    - Usado em CreateFromExistingInfo() e CloneInternal()

    FACTORY METHOD RegisterNew:
    - Valida com regras ATUAIS e CONTEXTUAIS
    - Retorna null se inválido
    - Coleta TODAS as mensagens

    FACTORY METHOD CreateFromExistingInfo:
    - NÃO valida (assume dados validados no passado)
    - NUNCA retorna null
    - Usado para reconstitution

    ═══════════════════════════════════════════════════════════════════════════════
    */
    private SimpleAggregateRoot(
        EntityInfo entityInfo,
        string firstName,
        string lastName,
        string fullname,
        BirthDate birthDate
    ) : base(entityInfo)
    {
        FirstName = firstName;
        LastName = lastName;
        FullName = fullname;
        BirthDate = birthDate;
    }

    // Public Business Methods
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Factory Methods Estáticos
    ═══════════════════════════════════════════════════════════════════════════════

    Factory methods estáticos na própria classe (não factories externas):
    ✅ Validação obrigatória (construtores private)
    ✅ Encapsulamento total
    ✅ Nomes expressivos (RegisterNew vs CreateFromExistingInfo)

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Retorno Nullable (não Result Pattern)
    ───────────────────────────────────────────────────────────────────────────────

    Métodos retornam Entity? ao invés de Result<Entity>:
    ✅ Simplicidade (menos código)
    ✅ Mensagens no ExecutionContext
    ✅ Compatível com IAsyncEnumerable<T>

    ❌ Result Pattern adiciona complexidade desnecessária para validação de negócio

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Exceções vs Retorno Nullable
    ───────────────────────────────────────────────────────────────────────────────

    RETORNO NULL (preferido para validação de negócio):
    - Validações de domínio esperadas (formato, tamanho, regras de negócio)
    - Usuário pode corrigir o input
    - Mensagens coletadas no ExecutionContext

    EXCEÇÕES (para falhas inesperadas/bugs):
    - ArgumentNullException: dependências obrigatórias null
    - InvalidOperationException: configuração inválida do sistema
    - Violação de invariantes internas (bugs no código)

    ❌ NUNCA lance exceção para: "FirstName muito longo", "BirthDate inválida"
    ✅ Use exceção para: TimeProvider null, ExecutionContext null (bug de configuração)

    RAZÃO: Validação de negócio é ESPERADA, não excepcional.
    Exceções têm custo de performance e stack trace desnecessário para input inválido.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: ExecutionContext Explícito
    ───────────────────────────────────────────────────────────────────────────────

    ExecutionContext é SEMPRE parâmetro explícito (não ThreadLocal/AsyncLocal):
    ✅ Friendly para iniciantes (explícito > implícito)
    ✅ Facilita testes
    ✅ Passivo - não interfere no fluxo de controle

    TimeProvider encapsulado no ExecutionContext:
    - Coesão (tempo faz parte do contexto)
    - Testabilidade (timestamps determinísticos)
    - Fontes de tempo centralizadas (NTP em sistemas distribuídos)

    ═══════════════════════════════════════════════════════════════════════════════
    */
    public static SimpleAggregateRoot? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        var instance = new SimpleAggregateRoot();

        /*
        ═══════════════════════════════════════════════════════════════════════════════
        LLM_RULE: SEMPRE Chamar RegisterNewInternal()
        ═══════════════════════════════════════════════════════════════════════════════

        ⚠️ CRÍTICO: SEMPRE use RegisterNewInternal() ao criar nova entidade.

        RegisterNewInternal() gerencia automaticamente:
        - ID, RegistryInfo (CreatedAt, CreatedBy, Version)
        - Eventos de criação
        - Validações da classe base
        - Estado de tracking

        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: Handler DEVE Ser Static Para Evitar Closures Acidentais
        ───────────────────────────────────────────────────────────────────────────────

        O parâmetro handler DEVE usar a keyword `static` para prevenir captura acidental
        de variáveis do escopo externo (closures):

        ✅ CORRETO - static lambda:
        handler: static (executionContext, input, instance) => { ... }

        ❌ ERRADO - lambda não-static pode capturar variáveis acidentalmente:
        handler: (executionContext, input, instance) => { ... }

        RAZÕES:
        1. SEGURANÇA: `static` causa erro de compilação se tentar capturar variável externa
        2. PERFORMANCE: static lambdas não alocam objetos de closure no heap
        3. CLAREZA: Torna explícito que o handler depende APENAS dos parâmetros recebidos
        4. PREVENÇÃO DE BUGS: Evita captura acidental de `this` ou outras variáveis

        EXEMPLO DE BUG PREVENIDO:
        var valorExterno = 42;
        handler: (ctx, inp, inst) => {
            // Sem static, isto compila mas cria closure:
            Console.WriteLine(valorExterno); // Captura acidental!
        }

        handler: static (ctx, inp, inst) => {
            // Com static, isto NÃO compila - erro de compilação:
            Console.WriteLine(valorExterno); // CS8820: Cannot use 'valorExterno'
        }

        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: Centralizar Validações em Métodos Internos
        ───────────────────────────────────────────────────────────────────────────────

        Métodos públicos SEMPRE chamam métodos internos (*Internal) para reutilização:

        ✅ CORRETO:
        return instance.ChangeNameInternal(...) & instance.ChangeBirthDateInternal(...);

        ❌ ERRADO (duplicação):
        return instance.SetFirstName(...) & instance.SetLastName(...);

        BENEFÍCIO: Mudanças em regras refletem em TODOS os métodos automaticamente.

        ═══════════════════════════════════════════════════════════════════════════════
        */
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: (executionContext, input) => new SimpleAggregateRoot(),
            handler: static (executionContext, input, instance) =>
            {
                return
                    instance.ChangeNameInternal(executionContext, input.FirstName, input.LastName)
                    & instance.ChangeBirthDateInternal(executionContext, input.BirthDate);
            }
        );
    }
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Reconstitution Pattern (CreateFromExistingInfo)
    ═══════════════════════════════════════════════════════════════════════════════

    Reconstitui entidades de fontes persistidas (banco, event store, cache) SEM validação.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: CreateFromExistingInfo NÃO Valida Dados
    ───────────────────────────────────────────────────────────────────────────────

    RAZÕES:

    1. EVOLUÇÃO DE REGRAS - Entidades criadas com regras passadas devem permanecer válidas
       2020: MaxLength=100 → "João da Silva Pereira Santos" (30 chars) ✅
       2025: MaxLength=20  → Mesma entidade agora seria inválida ❌

    2. EVENT SOURCING - Eventos históricos são imutáveis, replay deve sempre funcionar
       Se evento de 2020 falhar validação atual, replay para completamente

    3. COMPLIANCE - Dados históricos preservados exatamente como foram criados
       LGPD/GDPR/HIPAA exigem auditoria consistente

    4. TEMPORAL QUERIES - Consultar estado em momento passado exige dados originais

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Quando Usar CreateFromExistingInfo
    ───────────────────────────────────────────────────────────────────────────────

    USO CORRETO:
    ✅ Repositories carregando do banco
    ✅ Event handlers aplicando eventos (replay)
    ✅ Cache/deserialização
    ✅ Importação de dados legados

    USO INCORRETO:
    ❌ Criar novas entidades (use RegisterNew)
    ❌ Receber input de usuário
    ❌ "Pular" validações intencionalmente

    ───────────────────────────────────────────────────────────────────────────────
    LLM_TEMPLATE: Uso em Repository e Event Sourcing
    ───────────────────────────────────────────────────────────────────────────────

    // Repository
    var dto = _database.Query("SELECT * FROM Persons WHERE Id = @id", new { id });
    return SimpleAggregateRoot.CreateFromExistingInfo(
        new CreateFromExistingInfoInput(dto.EntityInfo, dto.FirstName, dto.LastName, dto.BirthDate)
    );

    // Event Handler
    public void Apply(PersonCreatedEvent @event)
    {
        _state = SimpleAggregateRoot.CreateFromExistingInfo(
            new CreateFromExistingInfoInput(@event.EntityInfo, @event.FirstName, @event.LastName, @event.BirthDate)
        );
    }

    ═══════════════════════════════════════════════════════════════════════════════
    */
    public static SimpleAggregateRoot CreateFromExistingInfo(
        CreateFromExistingInfoInput input
    )
    {
        return new SimpleAggregateRoot(
            input.EntityInfo,
            input.FirstName,
            input.LastName,
            input.FullName,
            input.BirthDate
        );
    }
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Input Objects Pattern
    ═══════════════════════════════════════════════════════════════════════════════

    Métodos recebem objetos Input (readonly record struct) ao invés de parâmetros individuais.

    BENEFÍCIOS:
    - Evoluibilidade: adicionar parâmetros sem quebrar assinatura
    - Legibilidade: named arguments implícitos
    - Customização: factories por tenant via IOC
    - Performance: stack allocation (struct)
    - Imutabilidade: previne modificações ocultas (readonly)

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Customização Por Tenant com Factories
    ───────────────────────────────────────────────────────────────────────────────

    Input objects permitem factories customizadas para multitenancy via IOC:

    public interface IChangeNameInputFactory
    {
        ChangeNameInput Create(string firstName, string lastName);
    }

    // Tenant Brasil: nome separado
    public class BrazilFactory : IChangeNameInputFactory
    {
        public ChangeNameInput Create(string firstName, string lastName)
            => new(firstName, lastName);
    }

    // Tenant Espanha: nome completo
    public class SpainFactory : IChangeNameInputFactory
    {
        public ChangeNameInput Create(string firstName, string lastName)
            => new($"{firstName} {lastName}", string.Empty);
    }

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Sempre Use readonly record struct
    ───────────────────────────────────────────────────────────────────────────────

    RAZÕES:
    - STRUCT: stack allocation (zero GC pressure)
    - READONLY: previne modificações ocultas entre camadas
    - RECORD: equality por valor, ToString() automático

    ❌ input.FirstName = "Jane";  // NÃO COMPILA - readonly

    ═══════════════════════════════════════════════════════════════════════════════
    */
    public SimpleAggregateRoot? ChangeName(
        ExecutionContext executionContext,
        ChangeNameInput input
    )
    {
        /*
        ═══════════════════════════════════════════════════════════════════════════════
        LLM_GUIDANCE: Imutabilidade Controlada (Clone-Modify-Return Pattern)
        ═══════════════════════════════════════════════════════════════════════════════

        Métodos públicos NUNCA modificam a instância atual:
        1. CLONE - RegisterChangeInternal clona instância atual
        2. MODIFY - Aplica mudanças na CÓPIA
        3. RETURN - Retorna cópia (sucesso) ou null (falha)

        BENEFÍCIOS:
        - Estado sempre consistente (original nunca modificado)
        - Rollback automático (se falhar, original intacto)
        - Thread-safety natural
        - Facilita auditoria e event sourcing

        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: SEMPRE Chamar RegisterChangeInternal
        ───────────────────────────────────────────────────────────────────────────────

        RegisterChangeInternal gerencia automaticamente:
        ✅ Clone da instância
        ✅ Atualização de RegistryInfo (ModifiedAt, ModifiedBy, Version)
        ✅ Incremento de versão (concurrency control)
        ✅ Eventos de modificação
        ✅ Estado de tracking

        ❌ NUNCA clone manualmente - duplicação de código e fácil esquecer versão/auditoria

        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: Handler DEVE Ser Static
        ───────────────────────────────────────────────────────────────────────────────

        O handler passado para RegisterChangeInternal DEVE ser static para evitar
        closures acidentais. Ver regra completa em RegisterNew().

        ✅ handler: static (ctx, inp, newInstance) => { ... }
        ❌ handler: (ctx, inp, newInstance) => { ... }

        ───────────────────────────────────────────────────────────────────────────────
        LLM_ANTIPATTERN: Mutabilidade Direta
        ───────────────────────────────────────────────────────────────────────────────

        ❌ Modificar this diretamente leva a estado inconsistente:

        public void Anonymize()
        {
            FirstName = "Anonymous";  // ✅ Modificado
            LastName = "User";        // ✅ Modificado
            if (!SetBirthDate(...))   // ❌ Falha aqui
                throw new Exception(); // PROBLEMA: FirstName e LastName JÁ mudaram!
        }

        ✅ Clone-Modify-Return previne estado parcialmente modificado.

        ═══════════════════════════════════════════════════════════════════════════════
        */

        return RegisterChangeInternal<SimpleAggregateRoot, ChangeNameInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeNameInternal(executionContext, input.FirstName, input.LastName);
            }
        );
    }
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Métodos Públicos vs Métodos Internos
    ═══════════════════════════════════════════════════════════════════════════════

    MÉTODOS PÚBLICOS:
    - Orquestram operações de negócio
    - Chamam Register*Internal() UMA ÚNICA VEZ
    - Retornam nova instância ou null

    MÉTODOS INTERNOS (*Internal):
    - Implementam regras de negócio específicas
    - Reutilizáveis por múltiplos métodos públicos
    - Retornam bool (sucesso/falha)
    - NÃO criam clones

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Métodos Públicos Reutilizam Métodos Internos
    ───────────────────────────────────────────────────────────────────────────────

    RegisterNew e ChangeName AMBOS chamam ChangeNameInternal:
    ✅ Mudança em validação reflete em TODOS os métodos automaticamente
    ✅ Zero duplicação de código

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Register*Internal Chamado UMA ÚNICA VEZ
    ───────────────────────────────────────────────────────────────────────────────

    ✅ CORRETO - Uma chamada, múltiplos métodos internos, handler static:

    return RegisterChangeInternal(..., handler: static (ctx, inp, newInstance) =>
    {
        return newInstance.ChangeNameInternal(...)
            & newInstance.ChangeBirthDateInternal(...)
            & newInstance.ChangeAddressInternal(...);
    });

    ❌ ERRADO - Múltiplas chamadas a RegisterChangeInternal:
    - Múltiplos clones criados
    - Versão incrementada múltiplas vezes
    - Estado inconsistente

    ═══════════════════════════════════════════════════════════════════════════════
    */
    public SimpleAggregateRoot? ChangeBirthDate(
        ExecutionContext executionContext,
        ChangeBirthDateInput input
    )
    {
        return RegisterChangeInternal<SimpleAggregateRoot, ChangeBirthDateInput>(
            executionContext,
            instance: this,
            input,
            handler: static (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeBirthDateInternal(executionContext, input.BirthDate);
            }
        );
    }

    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Método Clone
    ═══════════════════════════════════════════════════════════════════════════════

    Cria nova instância com mesmos valores (imutabilidade controlada).

    USOS:
    - RegisterChangeInternal() para Clone-Modify-Return
    - Serviços de domínio para cache ou cópias independentes

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Clone Usa Construtor Privado (não CreateFromExistingInfo)
    ───────────────────────────────────────────────────────────────────────────────

    Clone chama construtor privado diretamente, NÃO CreateFromExistingInfo.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Método Público NUNCA Chama Outro Método Público
    ───────────────────────────────────────────────────────────────────────────────

    PROBLEMA - Lateralidade entre métodos públicos causa:

    1. OPERAÇÕES DUPLICADAS ACIDENTAIS:
       Se Clone() chamasse CreateFromExistingInfo(), e CreateFromExistingInfo()
       registrasse evento de "entidade carregada", Clone() também registraria
       esse evento indevidamente durante modificações normais.

    2. EFEITOS COLATERAIS IMPREVISÍVEIS:
       Métodos públicos podem ter side-effects (logging, eventos, métricas).
       Chamar um método público de outro acumula side-effects inesperados.

    3. DIFICULDADE DE MANUTENÇÃO:
       Mudança em CreateFromExistingInfo() afetaria Clone() sem intenção.
       Rastrear bugs se torna difícil com múltiplos caminhos de execução.

    4. VIOLAÇÃO DO PRINCÍPIO DE RESPONSABILIDADE ÚNICA:
       Cada método público deve ter caminho de execução isolado e previsível.

    SOLUÇÃO: Se dois métodos públicos precisam da mesma lógica, extraia para
    um método *Internal compartilhado que não tem side-effects de orquestração.

    ═══════════════════════════════════════════════════════════════════════════════
    */
    public override SimpleAggregateRoot Clone()
    {
        return new SimpleAggregateRoot(
            EntityInfo,
            FirstName,
            LastName,
            FullName,
            BirthDate
        );
    }


    // Private Business Methods
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Métodos Privados *Internal
    ═══════════════════════════════════════════════════════════════════════════════

    Cada método público tem um análogo privado *Internal que implementa a lógica real:

    MÉTODO PÚBLICO:
    - Gerencia ciclo de vida (clonagem, versionamento, eventos)
    - Chama RegisterNewInternal() ou RegisterChangeInternal()
    - Retorna nova instância ou null

    MÉTODO *Internal:
    - Foca exclusivamente em lógica de negócio e validação
    - Retorna bool (sucesso/falha)
    - NÃO gerencia ciclo de vida (clonagem, versionamento)
    - Opera na instância atual (this) - chamado após clone

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Métodos *Internal Recebem Parâmetros Diretos (não Input Objects)
    ───────────────────────────────────────────────────────────────────────────────

    Métodos privados recebem parâmetros individuais, NÃO objetos Input.

    RAZÃO: Input objects existem para customização via factories (IOC).
    Métodos privados são chamados apenas internamente - não há ponto de extensão.
    Mudanças em regras internas são feitas via herança, não injeção de factories.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Usar Operador & (bitwise AND) em Métodos *Internal
    ───────────────────────────────────────────────────────────────────────────────

    Métodos *Internal DEVEM usar `&` para combinar validações:

    bool isSuccess =
        SetFirstName(executionContext, firstName)
        & SetLastName(executionContext, lastName);

    RAZÃO: Garantir que TODAS as validações são executadas, mesmo se alguma falhar.

    POR QUE MÚLTIPLAS FALHAS SÃO ACEITÁVEIS:
    - Métodos *Internal operam no CLONE, não na instância original
    - Se qualquer validação falhar, o clone é descartado
    - A instância original permanece intacta (imutabilidade)
    - Usuário recebe feedback COMPLETO de todos os erros de uma vez

    ❌ COM `&&`: Para na primeira falha → usuário vê 1 erro por vez (UX ruim)
    ✅ COM `&`: Executa todas → usuário corrige tudo de uma vez (UX melhor)

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Entidades Não Têm Dependências Externas
    ───────────────────────────────────────────────────────────────────────────────

    Entidades de domínio NÃO dependem de:
    ❌ Repositórios
    ❌ Serviços externos
    ❌ Factories injetadas

    ÚNICAS DEPENDÊNCIAS PERMITIDAS:
    ✅ ExecutionContext (contexto de execução)
    ✅ Outras entidades de domínio (associação/composição)

    ═══════════════════════════════════════════════════════════════════════════════
    */
    private bool ChangeNameInternal(
        ExecutionContext executionContext,
        string firstName,
        string lastName
    )
    {
        string fullName = $"{firstName} {lastName}";

        // LLM_RULE: Variável intermediária isSuccess facilita debug e análise estática
        bool isSuccess =
            SetFirstName(executionContext, firstName)
            & SetLastName(executionContext, lastName)
            & SetFullName(executionContext, fullName);

        return isSuccess;
    }

    private bool ChangeBirthDateInternal(
        ExecutionContext executionContext,
        BirthDate birthDate
    )
    {
        return SetBirthDate(
            executionContext,
            birthDate
        );
    }

    // Validation Methods
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Métodos de Validação Estáticos Por Propriedade
    ═══════════════════════════════════════════════════════════════════════════════

    Cada propriedade com regras DEVE ter um método de validação estático público.

    BENEFÍCIOS:
    - Camadas externas (controllers, consumers, serviços) validam inputs ANTES de
      criar ou modificar a entidade
    - Fail-fast: erros detectados no ponto de entrada, não no domínio
    - Reutilização: mesma lógica usada internamente e externamente

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Estratégia de Validação Depende da Origem do Input
    ───────────────────────────────────────────────────────────────────────────────

    INPUT DE USUÁRIO (UI, API pública):
    ✅ Retornar TODOS os erros (operador &)
    ✅ UX melhor: usuário corrige tudo de uma vez
    ✅ Menos roundtrips

    INPUT SISTÊMICO (filas, integrações, eventos):
    ✅ Fail-fast (operador &&) é aceitável
    ✅ Processamento será rejeitado de qualquer forma
    ✅ Menos overhead quando erro é inevitável
    ✅ Logs mais concisos

    ───────────────────────────────────────────────────────────────────────────────
    LLM_TEMPLATE: Validação em Controller (UI - todos os erros)
    ───────────────────────────────────────────────────────────────────────────────

    // UI: retorna TODOS os erros para o usuário
    bool isValid =
        SimpleAggregateRoot.ValidateFirstName(executionContext, request.FirstName)
        & SimpleAggregateRoot.ValidateLastName(executionContext, request.LastName);

    if (!isValid)
        return BadRequest(executionContext.Messages);

    ───────────────────────────────────────────────────────────────────────────────
    LLM_TEMPLATE: Validação em Consumer (Sistêmico - fail-fast)
    ───────────────────────────────────────────────────────────────────────────────

    // Fila/Integração: fail-fast é aceitável
    if (!SimpleAggregateRoot.ValidateFirstName(executionContext, message.FirstName))
    {
        _logger.LogWarning("Mensagem rejeitada: {Messages}", executionContext.Messages);
        return;
    }

    ═══════════════════════════════════════════════════════════════════════════════
    */
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        string? firstName,
        string? lastName,
        BirthDate? birthDate
    )
    {
        return
            EntityBaseIsValid(executionContext, entityInfo)
            & ValidateFirstName(executionContext, firstName)
            & ValidateLastName(executionContext, lastName)
            & ValidateBirthDate(executionContext, birthDate);
    }
    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_GUIDANCE: Método de Instância IsValid
    ───────────────────────────────────────────────────────────────────────────────

    Valida a instância atual contra as regras ATUAIS.

    POR QUE É NECESSÁRIO:
    - Entidades reconstituídas (CreateFromExistingInfo) NÃO passam por validação
    - Regras podem ter mudado desde a criação original
    - Permite verificar compliance com regras atuais após carregamento

    CASOS DE USO:
    ✅ Verificar se entidade histórica está em compliance com regras atuais
    ✅ Migração de dados: identificar registros que precisam de correção
    ✅ Auditoria: listar entidades fora de conformidade

    ───────────────────────────────────────────────────────────────────────────────
    */
    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            FirstName,
            LastName,
            BirthDate
        );
    }

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Métodos Validate* São Puros (Sem Side-Effects)
    ───────────────────────────────────────────────────────────────────────────────

    Métodos Validate* NUNCA alteram estado da entidade.

    COMPORTAMENTO:
    - Retornam true (válido) ou false (inválido)
    - Adicionam mensagens ao ExecutionContext quando inválido
    - São públicos e estáticos para uso em camadas externas

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Parâmetros São Nullable Por Design
    ───────────────────────────────────────────────────────────────────────────────

    Parâmetros são declarados como nullable (string?, BirthDate?) porque:
    - A regra IsRequired é DINÂMICA (pode ser alterada em runtime via Change*Metadata)
    - Se IsRequired=false, null é um valor válido
    - A validação de obrigatoriedade ocorre em RUNTIME, não em compile-time

    ❌ ValidateFirstName(string firstName)  // Força non-null em compile-time
    ✅ ValidateFirstName(string? firstName) // Permite null, valida em runtime

    ───────────────────────────────────────────────────────────────────────────────
    */
    public static bool ValidateFirstName(
        ExecutionContext executionContext,
        string? firstName
    )
    {
        /*
        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: Usar ValidationUtils Para Validações Padrão
        ───────────────────────────────────────────────────────────────────────────────

        ValidationUtils fornece métodos de validação reutilizáveis:
        - ValidateIsRequired: verifica obrigatoriedade
        - ValidateMinLength/ValidateMaxLength: verifica limites de tamanho

        BENEFÍCIOS:
        - Consistência: mesma lógica em todas as entidades
        - Mensagens padronizadas: formato uniforme no sistema
        - Manutenção: correções propagam automaticamente

        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: Usar CreateMessageCode<T> Para Códigos de Mensagem
        ───────────────────────────────────────────────────────────────────────────────

        CreateMessageCode<T>(propertyName) gera códigos consistentes:
        - Formato: {EntityName}.{PropertyName} (ex: SimpleAggregateRoot.FirstNameIsRequired)
        - Facilita i18n: código único para lookup de traduções
        - Rastreabilidade: origem do erro identificável

        ───────────────────────────────────────────────────────────────────────────────
        LLM_TEMPLATE: Padrão de Validação de Propriedade
        ───────────────────────────────────────────────────────────────────────────────

        bool isRequiredValid = ValidationUtils.ValidateIsRequired(
            executionContext,
            messageCode: CreateMessageCode<SimpleAggregateRoot>(nameof(Metadata.PropertyIsRequired)),
            isRequired: Metadata.PropertyIsRequired,
            value: propertyValue
        );

        if (!isRequiredValid)
            return false;

        bool minLengthValid = ValidationUtils.ValidateMinLength(...);
        bool maxLengthValid = ValidationUtils.ValidateMaxLength(...);

        return isRequiredValid && minLengthValid && maxLengthValid;

        ───────────────────────────────────────────────────────────────────────────────
        */
        bool firstNameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(propertyName: SimpleAggregateRootMetadata.FirstNamePropertyName),
            isRequired: SimpleAggregateRootMetadata.FirstNameIsRequired,
            value: firstName
        );

        if (!firstNameIsRequiredValidation)
            return false;

        bool firstNameMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(propertyName: SimpleAggregateRootMetadata.FirstNamePropertyName),
            minLength: SimpleAggregateRootMetadata.FirstNameMinLength,
            value: firstName!.Length
        );

        bool firstNameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(propertyName: SimpleAggregateRootMetadata.FirstNamePropertyName),
            maxLength: SimpleAggregateRootMetadata.FirstNameMaxLength,
            value: firstName!.Length
        );

        /*
        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: Variáveis Intermediárias Para Legibilidade e Análise Estática
        ───────────────────────────────────────────────────────────────────────────────

        Cada validação é armazenada em variável separada ao invés de expressão inline.

        BENEFÍCIOS:
        - Legibilidade: expressões booleanas longas são difíceis de entender
        - Debug: breakpoints em cada validação, inspeção de valores individuais
        - Análise estática: Roslyn/SonarQube rastreiam cada resultado separadamente

        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: Usar && No Retorno Final (Todas Validações Já Executadas)
        ───────────────────────────────────────────────────────────────────────────────

        O retorno usa `&&` porque TODAS as validações já foram executadas acima.
        Neste ponto, `&&` é apenas combinação lógica dos resultados já computados.

        ❌ NÃO confundir com short-circuit durante execução das validações
        ✅ Aqui é apenas: "todas passaram?" → true/false

        ───────────────────────────────────────────────────────────────────────────────
        */

        return firstNameIsRequiredValidation
            && firstNameMinLengthValidation
            && firstNameMaxLengthValidation;
    }

    public static bool ValidateLastName(
        ExecutionContext executionContext,
        string? lastName
    )
    {
        bool lastNameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(propertyName: SimpleAggregateRootMetadata.LastNamePropertyName),
            isRequired: SimpleAggregateRootMetadata.LastNameIsRequired,
            value: lastName
        );

        if (!lastNameIsRequiredValidation)
            return false;

        bool lastNameMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(propertyName: SimpleAggregateRootMetadata.LastNamePropertyName),
            minLength: SimpleAggregateRootMetadata.LastNameMinLength,
            value: lastName!.Length
        );

        bool lastNameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(propertyName: SimpleAggregateRootMetadata.LastNamePropertyName),
            maxLength: SimpleAggregateRootMetadata.LastNameMaxLength,
            value: lastName!.Length
        );

        return lastNameIsRequiredValidation
            && lastNameMinLengthValidation
            && lastNameMaxLengthValidation;
    }

    public static bool ValidateFullName(
        ExecutionContext executionContext,
        string? fullName
    )
    {
        bool fullNameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(propertyName: SimpleAggregateRootMetadata.FullNamePropertyName),
            isRequired: SimpleAggregateRootMetadata.FullNameIsRequired,
            value: fullName
        );

        if (!fullNameIsRequiredValidation)
            return false;

        bool fullNameMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(propertyName: SimpleAggregateRootMetadata.FullNamePropertyName),
            minLength: SimpleAggregateRootMetadata.FullNameMinLength,
            value: fullName!.Length
        );

        bool fullNameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(propertyName: SimpleAggregateRootMetadata.FullNamePropertyName),
            maxLength: SimpleAggregateRootMetadata.FullNameMaxLength,
            value: fullName!.Length
        );

        return fullNameIsRequiredValidation
            && fullNameMinLengthValidation
            && fullNameMaxLengthValidation;
    }

    public static bool ValidateBirthDate(
        ExecutionContext executionContext,
        BirthDate? birthDate
    )
    {
        bool birthDateIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(propertyName: SimpleAggregateRootMetadata.BirthDatePropertyName),
            isRequired: SimpleAggregateRootMetadata.BirthDateIsRequired,
            value: birthDate
        );

        if (!birthDateIsRequiredValidation)
            return false;

        /*
        ───────────────────────────────────────────────────────────────────────────────
        LLM_GUIDANCE: Validação de Datas com Contexto Temporal
        ───────────────────────────────────────────────────────────────────────────────

        Datas podem ser validadas de duas formas:
        - ABSOLUTA: MinDate/MaxDate (ex: não aceitar datas antes de 1900)
        - RELATIVA: MinAge/MaxAge em anos (ex: idade mínima 18 anos)

        Validação RELATIVA requer contexto temporal:
        - A idade depende da data de REFERÊNCIA (hoje)
        - "Hoje" varia conforme o momento da execução
        - Testes precisam de datas determinísticas

        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: Usar TimeProvider do ExecutionContext Para Cálculos Temporais
        ───────────────────────────────────────────────────────────────────────────────

        SEMPRE use executionContext.TimeProvider para obter a data/hora atual:
        ✅ Testabilidade: testes usam FakeTimeProvider com data fixa
        ✅ Consistência: mesma referência temporal em toda a operação
        ✅ Sistemas distribuídos: fonte de tempo centralizada (NTP)

        ❌ DateTime.Now / DateTime.UtcNow - não testável, não consistente

        ───────────────────────────────────────────────────────────────────────────────
        */
        int ageInYears = birthDate!.Value.CalculateAgeInYears(executionContext.TimeProvider);

        bool birthDateMinAgeValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(propertyName: SimpleAggregateRootMetadata.BirthDatePropertyName),
            minLength: SimpleAggregateRootMetadata.BirthDateMinAgeInYears,
            value: ageInYears
        );

        bool birthDateMaxAgeValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<SimpleAggregateRoot>(propertyName: SimpleAggregateRootMetadata.BirthDatePropertyName),
            maxLength: SimpleAggregateRootMetadata.BirthDateMaxAgeInYears,
            value: ageInYears
        );

        return birthDateIsRequiredValidation
            && birthDateMinAgeValidation
            && birthDateMaxAgeValidation;
    }

    // Set Methods
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Métodos Set* Privados
    ═══════════════════════════════════════════════════════════════════════════════

    Cada propriedade tem um método Set* privado que:
    1. Chama o método Validate* correspondente
    2. Se válido, atribui o valor à propriedade
    3. Retorna bool (sucesso/falha)

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Atribuição SOMENTE Após Validação
    ───────────────────────────────────────────────────────────────────────────────

    O SET da propriedade DEVE ocorrer:
    ✅ SOMENTE se validação passar
    ✅ SOMENTE dentro do método Set*

    Isso GARANTE que a entidade NUNCA entre em estado inválido.

    ÚNICA EXCEÇÃO: Construtor privado atribui diretamente (sem validação) porque:
    - CreateFromExistingInfo assume dados já validados no passado
    - Clone assume dados já validados na instância original

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Set* vs *Internal - Responsabilidades Diferentes
    ───────────────────────────────────────────────────────────────────────────────

    MÉTODO *Internal (ex: ChangeNameInternal):
    - Orquestra múltiplos Set* em conjunto
    - Combina resultados com operador &
    - Representa operação de negócio

    MÉTODO Set* (ex: SetFirstName):
    - Valida e atribui UMA ÚNICA propriedade
    - Responsabilidade atômica e isolada
    - Não conhece outras propriedades

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Parâmetro Set* Segue Nullability da Propriedade
    ───────────────────────────────────────────────────────────────────────────────

    VALIDATE* → Parâmetro SEMPRE nullable (regra IsRequired é dinâmica)
    SET*      → Parâmetro SEGUE o tipo da propriedade

    Propriedade non-null → SetFirstName(string firstName)
    Propriedade nullable → SetMiddleName(string? middleName)

    RAZÃO: Se Validate* passou e IsRequired=true, o valor NÃO é null.
    O compilador garante type-safety no momento da atribuição.

    ═══════════════════════════════════════════════════════════════════════════════
    */
    private bool SetFirstName(
        ExecutionContext executionContext,
        string firstName
    )
    {
        bool isValid = ValidateFirstName(
            executionContext,
            firstName
        );

        if (!isValid)
            return false;

        FirstName = firstName;

        return true;
    }

    private bool SetLastName(
        ExecutionContext executionContext,
        string lastName
    )
    {
        bool isValid = ValidateLastName(
            executionContext,
            lastName
        );

        if (!isValid)
            return false;

        LastName = lastName;

        return true;
    }

    private bool SetFullName(
        ExecutionContext executionContext,
        string fullName
    )
    {
        bool isValid = ValidateFullName(
            executionContext,
            fullName
        );

        if (!isValid)
            return false;

        FullName = fullName;

        return true;
    }

    private bool SetBirthDate(
        ExecutionContext executionContext,
        BirthDate birthDate
    )
    {
        bool isValid = ValidateBirthDate(
            executionContext,
            birthDate
        );

        if (!isValid)
            return false;

        BirthDate = birthDate;

        return true;
    }
}
