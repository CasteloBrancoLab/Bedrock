using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Templates.Domain.Entities.AbstractAggregateRoots.Base.Inputs;
using Templates.Domain.Entities.AbstractAggregateRoots.Enums;

namespace Templates.Domain.Entities.AbstractAggregateRoots.Base;

public abstract class AbstractAggregateRoot
    : EntityBase<AbstractAggregateRoot>,
    IAggregateRoot
{
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Metadados de Validação em Classes Abstratas
    ═══════════════════════════════════════════════════════════════════════════════

    Classes abstratas que possuem propriedades com regras de validação DEVEM
    definir seus próprios metadados e métodos Validate*.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Classe Abstrata Gerencia Seu Próprio Estado
    ───────────────────────────────────────────────────────────────────────────────

    A classe abstrata é responsável por:
    - Definir metadados de suas propriedades (SamplePropertyMetadata)
    - Expor métodos Validate* públicos estáticos para suas propriedades
    - Validar seu próprio estado via métodos Set* privados

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Quando Metadados São Desnecessários
    ───────────────────────────────────────────────────────────────────────────────

    Metadados seriam irrelevantes APENAS se a classe abstrata fosse um simples
    agregador de propriedades SEM lógica de validação própria. Nesse caso raro,
    a classe filha seria responsável por toda a validação.

    ═══════════════════════════════════════════════════════════════════════════════
    */
    public static class AbstractAggregateRootMetadata
    {
        // Fields
        private static readonly Lock _lockObject = new();

        // SampleProperty
        public static readonly string SamplePropertyPropertyName = nameof(SampleProperty);
        public static bool SamplePropertyIsRequired { get; private set; } = true;
        public static int SamplePropertyMinLength { get; private set; } = 1;
        public static int SamplePropertyMaxLength { get; private set; } = 255;

        public static void ChangeSamplePropertyMetadata(
            bool isRequired,
            int minLength,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                SamplePropertyIsRequired = isRequired;
                SamplePropertyMinLength = minLength;
                SamplePropertyMaxLength = maxLength;
            }
        }

        // CategoryType
        public static readonly string CategoryTypePropertyName = nameof(CategoryType);
        public static bool CategoryTypeIsRequired { get; private set; } = true;
        public static byte CategoryTypeMinValue { get; private set; } = (byte)CategoryType.TypeA;
        public static byte CategoryTypeMaxValue { get; private set; } = (byte)CategoryType.TypeB;

        public static void ChangeCategoryTypeMetadata(
            bool isRequired,
            byte minValue,
            byte maxValue
        )
        {
            lock (_lockObject)
            {
                CategoryTypeIsRequired = isRequired;
                CategoryTypeMinValue = minValue;
                CategoryTypeMaxValue = maxValue;
            }
        }
    }

    // Properties
    public string SampleProperty { get; private set; } = string.Empty;
    public CategoryType CategoryType { get; private set; }

    // Constructors
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Construtores em Classes Abstratas
    ═══════════════════════════════════════════════════════════════════════════════

    Classes abstratas precisam de DOIS construtores protegidos:
    1. Construtor VAZIO - para que a classe filha possa ter seu próprio construtor
       vazio usado em RegisterNew (validação incremental)
    2. Construtor COMPLETO - para reconstitution (CreateFromExistingInfo) e Clone

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Ambos Construtores São Protegidos (Não Privados)
    ───────────────────────────────────────────────────────────────────────────────

    DIFERENÇA DE VISIBILIDADE:
    - Classe CONCRETA (sealed): construtores são PRIVATE
    - Classe ABSTRATA: construtores são PROTECTED

    RAZÃO: Classes filhas precisam chamar base(...) em seus próprios construtores
    para inicializar o estado herdado da classe abstrata.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Por Que Construtor Vazio É Necessário em Classes Abstratas
    ───────────────────────────────────────────────────────────────────────────────

    A classe filha sealed precisa de um construtor vazio para RegisterNew:

    public sealed class Employee : Person
    {
        private Employee() : base() { }  // ✅ Chama construtor vazio da pai

        public static Employee? RegisterNew(ExecutionContext ctx, ...)
        {
            var instance = new Employee();  // Usa construtor vazio
            // Valida e atribui propriedade por propriedade...
        }
    }

    Se a classe abstrata não tiver construtor vazio, a filha seria forçada a
    passar valores "placeholder" inválidos (como null!), quebrando o princípio
    de "estado inválido nunca existe na memória".

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Construtor Completo Para Reconstitution e Clone
    ───────────────────────────────────────────────────────────────────────────────

    O construtor completo protegido permite que a classe filha reconstitua
    entidades a partir de dados persistidos:

    public sealed class Employee : Person
    {
        private Employee(EntityInfo info, string firstName, string empNumber)
            : base(info, firstName)  // ✅ Inicializa estado da pai
        {
            EmployeeNumber = empNumber;  // Inicializa estado próprio
        }
    }

    ═══════════════════════════════════════════════════════════════════════════════
    */
    protected AbstractAggregateRoot()
    {
    }

    protected AbstractAggregateRoot(
        EntityInfo entityInfo,
        string sampleProperty,
        CategoryType categoryType
    ) : base(entityInfo)
    {
        SampleProperty = sampleProperty;
        CategoryType = categoryType;
    }

    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Classe Abstrata Não Tem CreateFromExistingInfo (DE-056)
    ═══════════════════════════════════════════════════════════════════════════════

    Classes abstratas NÃO expõem método CreateFromExistingInfo porque:
    1. Classes abstratas não podem ser instanciadas diretamente
    2. Cada classe concreta tem propriedades específicas que precisam ser reconstituídas
    3. O repository conhece o tipo concreto e usa o factory method apropriado

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Reconstitution É Responsabilidade da Classe Concreta
    ───────────────────────────────────────────────────────────────────────────────

    A classe abstrata fornece APENAS o construtor protegido para as filhas usarem.
    NÃO existe CreateFromExistingInfo na classe abstrata, nem input para ela.

    // ❌ CLASSE ABSTRATA NÃO TEM CreateFromExistingInfo
    public abstract class Customer
    {
        // Não existe: public static Customer CreateFromExistingInfo(...)
        // Não existe: CreateFromExistingInfoCustomerInput
    }

    // ✅ CLASSE CONCRETA TEM CreateFromExistingInfo COM SEU PRÓPRIO INPUT
    public sealed class Individual : Customer
    {
        public static Individual CreateFromExistingInfo(
            CreateFromExistingInfoIndividualInput input  // Input COMPLETO da classe concreta
        )
        {
            return new Individual(
                input.EntityInfo,
                input.FirstName,       // Propriedades da classe base
                input.LastName,
                input.BirthDate        // Propriedade específica de Individual
            );
        }
    }

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Classe Abstrata Não Tem Input de Reconstitution
    ───────────────────────────────────────────────────────────────────────────────

    NÃO crie CreateFromExistingInfo*Input para a classe abstrata.
    Cada classe concreta define seu próprio input COMPLETO que inclui:
    - Todas as propriedades herdadas da classe abstrata
    - Suas propriedades específicas

    ❌ CreateFromExistingInfoCustomerInput     // NÃO CRIAR
    ✅ CreateFromExistingInfoIndividualInput   // Input completo para Individual
    ✅ CreateFromExistingInfoLegalEntityInput  // Input completo para LegalEntity

    ═══════════════════════════════════════════════════════════════════════════════
    */

    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: RegisterNewBase em Classes Abstratas
    ═══════════════════════════════════════════════════════════════════════════════

    A classe abstrata DEVE controlar seu processo de registro para garantir que
    TODAS as validações e inicializações de suas propriedades sejam executadas.

    PROBLEMA SEM ENCAPSULAMENTO:
    Se a classe filha chamar RegisterNewInternal diretamente, o desenvolvedor pode
    esquecer de chamar os métodos *Internal da classe pai, resultando em estado inválido.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Classe Abstrata Encapsula Seu Próprio Registro
    ───────────────────────────────────────────────────────────────────────────────

    RegisterNewBase encapsula RegisterNewInternal garantindo que:
    ✅ Validações da classe abstrata SEMPRE executam
    ✅ Inicializações da classe abstrata SEMPRE ocorrem
    ✅ Classe filha só precisa fornecer dados e lógica específica

    PARÂMETROS ADICIONAIS:
    - concreteTypeFactory: Cria instância do tipo concreto
    - baseInputFactory: Mapeia input da filha para input da classe base
    - handler: Lógica específica da classe filha (propriedades da filha)

    ───────────────────────────────────────────────────────────────────────────────
    */
    public static TConcreteType? RegisterNewBase<TConcreteType, TInput>(
        ExecutionContext executionContext,
        TInput input,
        Func<ExecutionContext, TInput, TConcreteType> concreteTypeFactory,
        Func<ExecutionContext, TInput, RegisterNewAbstractAggregateRootInput> baseInputFactory,
        Func<ExecutionContext, TInput, TConcreteType, bool> handler
    ) where TConcreteType : AbstractAggregateRoot
    {
        RegisterNewAbstractAggregateRootInput baseInput = baseInputFactory(executionContext, input);

        return RegisterNewInternal(
            executionContext,
            input: (Input: input, BaseInput: baseInput, ConcreteTypeFactory: concreteTypeFactory, Handler: handler),
            entityFactory: static (executionContext, input) => input.ConcreteTypeFactory(executionContext, input.Input),
            handler: static (executionContext, input, instance) =>
            {
                // Usa bitwise AND (&) para executar TODAS as validações
                // e coletar TODAS as mensagens de erro
                return
                    instance.ChangeSamplePropertyInternal(executionContext, input.BaseInput.SampleProperty)
                    & instance.ChangeCategoryTypeInternal(executionContext, input.BaseInput.CategoryType)
                    & input.Handler(executionContext, input.Input, instance)
                    ;
            }
        );
    }

    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Métodos Públicos de Negócio em Classes Abstratas
    ═══════════════════════════════════════════════════════════════════════════════

    Classes abstratas NÃO expõem métodos de negócio públicos. Apenas:
    - Métodos de validação (Validate*) → públicos estáticos
    - Métodos internos (*Internal) → protegidos
    - Métodos de atribuição (Set*) → privados

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Classe Filha Define Sua Própria API Pública
    ───────────────────────────────────────────────────────────────────────────────

    A classe filha concreta (sealed) é responsável por:
    - Definir seus próprios métodos públicos de negócio
    - Compor operações usando os métodos *Internal protegidos da classe pai
    - Expor factory methods (RegisterNew, CreateFromExistingInfo)

    // ❌ CLASSE ABSTRATA COM MÉTODO PÚBLICO (ERRADO)
    public abstract class Person
    {
        public Person? ChangeName(ExecutionContext ctx, string firstName, string lastName)
        {
            // Se a classe abstrata define isso, a filha não pode customizar
        }
    }

    // ✅ CLASSE ABSTRATA APENAS COM *Internal (CORRETO)
    public abstract class Person
    {
        protected bool ChangeNameInternal(ExecutionContext ctx, string firstName, string lastName)
        {
            // Lógica interna que a filha pode usar
        }
    }

    public sealed class Employee : Person
    {
        // Filha define SUA própria API pública
        public Employee? ChangeName(ExecutionContext ctx, ChangeNameInput input)
        {
            // Pode adicionar lógica específica de Employee
            // Usa ChangeNameInternal da classe pai
        }
    }

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Classe Abstrata Fornece Infraestrutura, Não Interface
    ───────────────────────────────────────────────────────────────────────────────

    A classe abstrata é INFRAESTRUTURA para as filhas, não uma INTERFACE para o mundo:

    - Validações → reutilizáveis pelas filhas e camadas externas
    - *Internal → operações completas que alteram estado da pai
    - Set* → atribuições individuais encapsuladas

    A classe filha COMPÕE essas peças para criar sua própria API pública.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Exceções São Raras e Documentadas
    ───────────────────────────────────────────────────────────────────────────────

    Casos onde a classe abstrata expõe métodos de negócio públicos são EXCEÇÕES:
    - Operações idênticas em TODAS as filhas sem variação
    - Comportamento que NUNCA será customizado
    - Deve ser explicitamente documentado o porquê

    Na dúvida, NÃO exponha. Deixe a filha decidir sua própria API.

    ═══════════════════════════════════════════════════════════════════════════════
    */

    public abstract override AbstractAggregateRoot Clone();

    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Métodos *Internal em Classes Abstratas
    ═══════════════════════════════════════════════════════════════════════════════

    Métodos *Internal são PROTEGIDOS em classes abstratas (diferente de classes
    concretas onde são privados).

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: *Internal Protegido é o Único Acesso ao Estado da Classe Pai
    ───────────────────────────────────────────────────────────────────────────────

    Classes filhas alteram estado da classe pai EXCLUSIVAMENTE via *Internal:

    // Classe filha (Employee) no seu RegisterNew:
    instance.ChangeNameInternal(ctx, firstName, lastName);  // ✅ Protegido - acessível
    instance.SetFirstName(ctx, firstName);                  // ❌ Privado - inacessível

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Private em Concretas, Protected em Abstratas
    ───────────────────────────────────────────────────────────────────────────────

    CLASSE CONCRETA (sealed):
    - *Internal é PRIVADO (ninguém herda, então não precisa ser acessível)

    CLASSE ABSTRATA:
    - *Internal é PROTEGIDO (filhas precisam acessar para compor suas operações)

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Encapsulamento Preservado
    ───────────────────────────────────────────────────────────────────────────────

    Mesmo sendo protected, *Internal mantém encapsulamento porque:
    - Representa operação de negócio COMPLETA (não setter individual)
    - Classe pai controla COMO seu estado é alterado
    - Impossível para a filha quebrar invariantes da pai

    Ver LLM_ANTIPATTERN em Set* para exemplo de por que Set* não pode ser protected.

    ═══════════════════════════════════════════════════════════════════════════════
    */
    protected bool ChangeSamplePropertyInternal(
        ExecutionContext executionContext,
        string sampleProperty
    )
    {
        return SetSampleProperty(executionContext, sampleProperty);
    }

    protected bool ChangeCategoryTypeInternal(
        ExecutionContext executionContext,
        CategoryType categoryType
    )
    {
        return SetCategoryType(executionContext, categoryType);
    }

    // Validation Methods
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Métodos Validate* em Classes Abstratas
    ═══════════════════════════════════════════════════════════════════════════════

    Métodos Validate* permanecem PÚBLICOS e ESTÁTICOS, mesmo em classes abstratas.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Validate* Públicos Para Validação Antecipada
    ───────────────────────────────────────────────────────────────────────────────

    Camadas externas (controllers, serviços de aplicação) precisam validar inputs
    ANTES de tentar criar ou modificar entidades:

    // Controller validando antes de chamar RegisterNew
    if (!AbstractAggregateRoot.ValidateSampleProperty(ctx, request.SampleProperty))
        return BadRequest(ctx.Messages);

    ❌ protected ValidateSampleProperty() // Inacessível às camadas externas
    ✅ public static ValidateSampleProperty() // Acessível de qualquer lugar

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Mesmas Regras de Classes Concretas
    ───────────────────────────────────────────────────────────────────────────────

    Métodos Validate* em classes abstratas seguem EXATAMENTE as mesmas regras
    definidas em SimpleAggregateRoot e nas ADRs DE-009 a DE-011:

    - Públicos e estáticos
    - Parâmetros nullable por design
    - Usam ValidationUtils para validações padrão
    - Usam CreateMessageCode<T> para códigos de mensagem

    ═══════════════════════════════════════════════════════════════════════════════
    */
    public static bool ValidateSampleProperty(
        ExecutionContext executionContext,
        string? sampleProperty
    )
    {
        bool samplePropertyIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<AbstractAggregateRoot>(propertyName: AbstractAggregateRootMetadata.SamplePropertyPropertyName),
            isRequired: AbstractAggregateRootMetadata.SamplePropertyIsRequired,
            value: sampleProperty
        );

        if (!samplePropertyIsRequiredValidation)
            return false;

        bool samplePropertyMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<AbstractAggregateRoot>(propertyName: AbstractAggregateRootMetadata.SamplePropertyPropertyName),
            minLength: AbstractAggregateRootMetadata.SamplePropertyMinLength,
            value: sampleProperty!.Length
        );

        bool samplePropertyMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<AbstractAggregateRoot>(propertyName: AbstractAggregateRootMetadata.SamplePropertyPropertyName),
            maxLength: AbstractAggregateRootMetadata.SamplePropertyMaxLength,
            value: sampleProperty!.Length
        );

        return samplePropertyIsRequiredValidation
            && samplePropertyMinLengthValidation
            && samplePropertyMaxLengthValidation;
    }

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_GUIDANCE: Validação de Enums por Range (Sem Enum.IsDefined)
    ───────────────────────────────────────────────────────────────────────────────

    NUNCA use Enum.IsDefined para validar enums em código de produção.

    ❌ Enum.IsDefined(typeof(CategoryType), value)
       - Usa reflexão internamente
       - Aloca memória (boxing do valor)
       - Performance ruim em hot paths

    ✅ Validação por Range (Min/Max)
       - Zero alocação
       - Comparação direta de bytes/ints
       - Performance ótima

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Metadados MinValue/MaxValue Para Enums
    ───────────────────────────────────────────────────────────────────────────────

    Enums DEVEM ter metadados MinValue e MaxValue que definem o range válido:
    - CategoryTypeMinValue = (byte)CategoryType.TypeA  // Primeiro valor válido
    - CategoryTypeMaxValue = (byte)CategoryType.TypeB  // Último valor válido

    A validação verifica: MinValue <= value <= MaxValue

    ───────────────────────────────────────────────────────────────────────────────
    */
    public static bool ValidateCategoryType(
        ExecutionContext executionContext,
        CategoryType? categoryType
    )
    {
        bool categoryTypeIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<AbstractAggregateRoot>(propertyName: AbstractAggregateRootMetadata.CategoryTypePropertyName),
            isRequired: AbstractAggregateRootMetadata.CategoryTypeIsRequired,
            value: categoryType
        );

        if (!categoryTypeIsRequiredValidation)
            return false;

        byte categoryTypeValue = (byte)categoryType!.Value;

        bool categoryTypeMinValueValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<AbstractAggregateRoot>(propertyName: AbstractAggregateRootMetadata.CategoryTypePropertyName),
            minLength: AbstractAggregateRootMetadata.CategoryTypeMinValue,
            value: categoryTypeValue
        );

        bool categoryTypeMaxValueValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<AbstractAggregateRoot>(propertyName: AbstractAggregateRootMetadata.CategoryTypePropertyName),
            maxLength: AbstractAggregateRootMetadata.CategoryTypeMaxValue,
            value: categoryTypeValue
        );

        return categoryTypeIsRequiredValidation
            && categoryTypeMinValueValidation
            && categoryTypeMaxValueValidation;
    }

    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Método IsValid Estático Público em Classes Abstratas
    ═══════════════════════════════════════════════════════════════════════════════

    Embora a classe abstrata não seja instanciada diretamente, o método IsValid
    estático PÚBLICO é necessário para validação de dados comuns em contextos
    que recebem a abstração como parâmetro.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Composição Por Convenção (Não Por Polimorfismo)
    ───────────────────────────────────────────────────────────────────────────────

    A classe filha sealed expõe seu próprio método IsValid público que COMPÕE
    a validação da classe pai com validações específicas:

    public sealed class Employee : Person
    {
        public static bool IsValid(ExecutionContext ctx, EntityInfo info, string name, string employeeNumber)
        {
            return Person.IsValid(ctx, info, name)  // Valida propriedades da pai
                & ValidateEmployeeNumber(ctx, employeeNumber);  // Valida propriedades próprias
        }
    }

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Por Que Não Usar Herança/Polimorfismo Para IsValid
    ───────────────────────────────────────────────────────────────────────────────

    1. MÉTODOS ESTÁTICOS NÃO SUPORTAM POLIMORFISMO:
       ❌ abstract static bool IsValid(...) // Não compila em C#

    2. ASSINATURAS DIFERENTES POR CLASSE:
       - Cada classe filha tem propriedades diferentes
       - Override não permite alterar assinatura
       - Generic parameter T para argumentos adiciona complexidade desnecessária

    3. GESTÃO SIMPLIFICADA:
       - Composição explícita é mais fácil de entender e manter
       - Validação via Roslyn ou code review garante chamada correta
       - Sem magic methods ou convenções ocultas

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: IsValid de Instância É Protegido
    ───────────────────────────────────────────────────────────────────────────────

    O método IsValidInternal (instância) é PROTEGIDO, permitindo que a classe filha
    chame base para validar dados comuns enquanto adiciona validações específicas.

    ═══════════════════════════════════════════════════════════════════════════════
    */
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        string? sampleProperty,
        CategoryType? categoryType
    )
    {
        return
            EntityBaseIsValid(executionContext, entityInfo)
            & ValidateSampleProperty(executionContext, sampleProperty)
            & ValidateCategoryType(executionContext, categoryType);
    }
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Métodos IsValid em Classes Abstratas
    ═══════════════════════════════════════════════════════════════════════════════

    Métodos IsValid em classes abstratas são PROTEGIDOS, não públicos.
    A classe filha sealed expõe seus próprios métodos IsValid públicos.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: IsValid Estático Protegido Para Composição
    ───────────────────────────────────────────────────────────────────────────────

    O método IsValid estático da classe abstrata é PROTEGIDO para que as classes
    filhas possam compor suas próprias validações:

    public sealed class Employee : Person
    {
        public static bool IsValid(ExecutionContext ctx, ...)
        {
            // Compõe validação da classe pai + validações próprias
            return Person.IsValid(ctx, ...)       // ✅ Protegido - acessível
                & ValidateEmployeeNumber(ctx, ...);
        }
    }

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: IsValid de Instância Protegido Para Composição
    ───────────────────────────────────────────────────────────────────────────────

    O método IsValid de instância também é PROTEGIDO. A classe filha expõe seu
    próprio método público que compõe a validação da pai:

    public sealed class Employee : Person
    {
        public bool IsValid(ExecutionContext ctx)
        {
            return base.IsValid(ctx)  // ✅ Protegido - acessível
                && ValidateEmployeeNumber(ctx, EmployeeNumber);
        }
    }

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Mesma Lógica de Métodos Públicos de Negócio
    ───────────────────────────────────────────────────────────────────────────────

    Assim como métodos de negócio públicos (ADR DE-050), IsValid segue o mesmo
    princípio: a classe abstrata fornece INFRAESTRUTURA (métodos protegidos) e
    a classe filha define SUA PRÓPRIA API pública.

    ═══════════════════════════════════════════════════════════════════════════════
    */
    protected override bool IsValidInternal(ExecutionContext executionContext)
    {
        return
            IsValid(executionContext, EntityInfo, SampleProperty, CategoryType)
            && IsValidConcreteInternal(executionContext);
    }
    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: IsValidConcreteInternal Abstrato Para Validações da Classe Filha
    ───────────────────────────────────────────────────────────────────────────────

    Em classes abstratas, IsValidConcreteInternal é PROTEGIDO e ABSTRATO.
    A classe filha sealed DEVE implementar para adicionar suas validações específicas.

    DIFERENÇA DE VISIBILIDADE:
    - Classe CONCRETA (não herda de abstrata): IsValidConcreteInternal é PRIVADO
    - Classe ABSTRATA: IsValidConcreteInternal é PROTEGIDO ABSTRATO

    RAZÃO: A classe filha precisa implementar o método para validar suas próprias
    propriedades, completando a validação iniciada pela classe pai.

    ───────────────────────────────────────────────────────────────────────────────
    */
    protected abstract bool IsValidConcreteInternal(
        ExecutionContext executionContext
    );

    // Set Methods
    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Métodos Set* em Classes Abstratas
    ═══════════════════════════════════════════════════════════════════════════════

    Em entidades abstratas, métodos Set* permanecem PRIVADOS. A classe abstrata
    é a única responsável por alterar seu próprio estado interno.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Set* Privado, Acesso Via *Internal Protegido
    ───────────────────────────────────────────────────────────────────────────────

    Classes filhas alteram estado da classe pai através de métodos *Internal protegidos:

    ✅ protected ChangeNameInternal() → chama → private SetFirstName() & SetLastName()
    ❌ protected SetFirstName(), SetLastName() // NÃO expor Set* diretamente

    RAZÃO: Mantém encapsulamento - a classe abstrata controla COMO seu estado é alterado.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_ANTIPATTERN: Set* Protegido Permite Estado Inválido
    ───────────────────────────────────────────────────────────────────────────────

    Se Set* fosse protected, a classe filha poderia quebrar invariantes da classe pai:

    // ❌ CLASSE PAI COM Set* PROTEGIDO (ERRADO)
    public abstract class Person
    {
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string FullName { get; private set; }  // Derivado de FirstName + LastName

        protected bool SetFirstName(...) { FirstName = value; return true; }
        protected bool SetLastName(...) { LastName = value; FullName = $"{FirstName} {LastName}"; return true; }
    }

    // ❌ CLASSE FILHA QUEBRANDO INVARIANTE
    public sealed class Employee : Person
    {
        public static Employee? RegisterNew(ExecutionContext ctx, string firstName, string lastName)
        {
            var instance = new Employee();

            // Desenvolvedor esquece de chamar SetFirstName, chama apenas SetLastName
            instance.SetLastName(ctx, lastName);  // FullName = " Silva" (FirstName vazio!)

            return instance;  // 💥 ESTADO INVÁLIDO: FullName inconsistente
        }
    }

    // ✅ SOLUÇÃO: *Internal protegido controla a operação COMPLETA
    public abstract class Person
    {
        protected bool ChangeNameInternal(ExecutionContext ctx, string firstName, string lastName)
        {
            // Classe PAI garante que FirstName, LastName e FullName são SEMPRE atualizados juntos
            return SetFirstName(ctx, firstName)
                & SetLastName(ctx, lastName)
                & SetFullName(ctx, $"{firstName} {lastName}");
        }

        private bool SetFirstName(...) { ... }  // Inacessível à filha
        private bool SetLastName(...) { ... }   // Inacessível à filha
        private bool SetFullName(...) { ... }   // Inacessível à filha
    }

    // ✅ CLASSE FILHA NÃO CONSEGUE QUEBRAR INVARIANTE
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

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Garantia do Liskov Substitution Principle (LSP)
    ───────────────────────────────────────────────────────────────────────────────

    LSP: Subclasses devem ser substituíveis pela classe base sem quebrar comportamento.

    COM Set* PROTEGIDO (VIOLA LSP):

    void ProcessPerson(Person person)
    {
        // Código assume que FullName é consistente com FirstName + LastName
        var parts = person.FullName.Split(' ');
        var firstName = parts[0];  // 💥 Se FullName = " Silva", firstName = ""
    }

    // Employee com FullName inconsistente VIOLA LSP - não pode ser usada onde Person é esperada

    COM *Internal PROTEGIDO (GARANTE LSP):

    - Qualquer instância de Employee respeita as invariantes de Person
    - Substituição segura em qualquer contexto que espera Person
    - LSP garantido por DESIGN, não por "boa vontade" do desenvolvedor
    - O compilador se torna o guardião do princípio

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Classe Abstrata Contém Lógica de Negócio (Não Apenas Estado)
    ───────────────────────────────────────────────────────────────────────────────

    A classe abstrata NÃO é apenas um depósito de propriedades compartilhadas.
    Ela possui lógica de negócio e validação que DEVE ser respeitada pelas filhas.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Métodos Virtual NUNCA São Usados
    ───────────────────────────────────────────────────────────────────────────────

    ❌ virtual SetFirstName() // Permite override que quebra validação
    ✅ abstract Clone()       // Especialização obrigatória pela filha

    RAZÃO: Lógica de negócio da classe pai não deve ser alterada pela filha.
    Se comportamento precisa ser especializado, use método ABSTRATO.

    ═══════════════════════════════════════════════════════════════════════════════
    */
    private bool SetSampleProperty(
        ExecutionContext executionContext,
        string sampleProperty
    )
    {
        bool isValid = ValidateSampleProperty(
            executionContext,
            sampleProperty
        );

        if (!isValid)
            return false;

        SampleProperty = sampleProperty;

        return true;
    }

    private bool SetCategoryType(
        ExecutionContext executionContext,
        CategoryType categoryType
    )
    {
        bool isValid = ValidateCategoryType(
            executionContext,
            categoryType
        );

        if (!isValid)
            return false;

        CategoryType = categoryType;

        return true;
    }
}
