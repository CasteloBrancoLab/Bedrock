using Bedrock.BuildingBlocks.Core.Validations;
using Bedrock.BuildingBlocks.Domain.Entities;
using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using Templates.Domain.Entities.CompositeAggregateRoots.Inputs;

namespace Templates.Domain.Entities.CompositeAggregateRoots;

public sealed class CompositeAggregateRoot
    : EntityBase<CompositeAggregateRoot>,
    IAggregateRoot
{
    // Metadata
    public static class CompositeAggregateRootMetadata
    {
        // Fields
        private static readonly Lock _lockObject = new();

        // Name
        public static readonly string NamePropertyName = nameof(Name);
        public static bool NameIsRequired { get; private set; } = true;
        public static int NameMinLength { get; private set; } = 1;
        public static int NameMaxLength { get; private set; } = 255;

        // Code
        public static readonly string CodePropertyName = nameof(Code);
        public static bool CodeIsRequired { get; private set; } = true;
        public static int CodeMinLength { get; private set; } = 1;
        public static int CodeMaxLength { get; private set; } = 50;

        // CompositeChildEntities
        public static readonly string CompositeChildEntitiesPropertyName = nameof(CompositeChildEntities);

        // Public Methods
        public static void ChangeNameMetadata(
            bool isRequired,
            int minLength,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                NameIsRequired = isRequired;
                NameMinLength = minLength;
                NameMaxLength = maxLength;
            }
        }

        public static void ChangeCodeMetadata(
            bool isRequired,
            int minLength,
            int maxLength
        )
        {
            lock (_lockObject)
            {
                CodeIsRequired = isRequired;
                CodeMinLength = minLength;
                CodeMaxLength = maxLength;
            }
        }
    }

    /*
    ═══════════════════════════════════════════════════════════════════════════════
    LLM_GUIDANCE: Coleções de Entidades Filhas em Aggregate Roots
    ═══════════════════════════════════════════════════════════════════════════════

    Coleções de entidades filhas DEVEM ser encapsuladas como field privado List<T>.

    ENCAPSULAMENTO:
    - Field privado `List<T>` impede modificação externa direta
    - Propriedade pública retorna `IReadOnlyList<T>` via `.AsReadOnly()`
    - Classes externas não conseguem Add(), Remove() ou Clear()

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Field Sempre Inicializado (Não Nullable)
    ───────────────────────────────────────────────────────────────────────────────

    O field DEVE ser inicializado como lista vazia `= []`, nunca nullable.

    POR QUE SEMPRE INICIALIZAR:
    ✅ Simplifica gestão: elimina null checks em todos os métodos
    ✅ Código mais limpo e menos propenso a NullReferenceException
    ✅ Propriedade pública retorna lista vazia (não null) - melhor UX para consumidores

    CUSTO DE ALOCAÇÃO É IRRELEVANTE:
    ✅ Lista vazia: ~24-40 bytes (header do objeto + ponteiro interno)
    ✅ Comparação com operações comuns que custam MUITO mais:
       - `.ToList()` em qualquer LINQ: aloca nova lista + copia elementos
       - `.Where().Select()`: aloca iteradores + closures
       - String interpolation: aloca novas strings
       - Entity Framework queries: centenas de alocações por query

    A micro-otimização de nullable não compensa a complexidade adicionada.

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Proteção Via API Pública (Não Via Reflection)
    ───────────────────────────────────────────────────────────────────────────────

    O encapsulamento protege contra manipulação via APIs públicas tradicionais.
    Manipulação via reflection é considerada "quebra de contrato" intencional
    e não é responsabilidade do design da entidade prevenir.

    ═══════════════════════════════════════════════════════════════════════════════
    */

    // Fields
    private readonly List<CompositeChildEntity> _compositeChildEntityCollection = [];

    // Properties
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Propriedade Pública Retorna IReadOnlyList<T> via AsReadOnly()
    ───────────────────────────────────────────────────────────────────────────────

    A propriedade pública DEVE retornar `IReadOnlyList<T>` usando `.AsReadOnly()`:
    ✅ Encapsula o field privado `List<T>`
    ✅ Impede Add(), Remove(), Clear() por consumidores externos

    ❌ NUNCA retorne o field diretamente (permite cast para List<T> e modificação)
    ❌ NUNCA use IEnumerable<T> (não garante que é readonly, permite múltiplas enumerações)

    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Field Sempre Inicializado (Não Nullable)
    ───────────────────────────────────────────────────────────────────────────────

    O field DEVE ser inicializado como lista vazia `= []`, nunca nullable.

    POR QUE SEMPRE INICIALIZAR:
    ✅ Simplifica gestão: elimina null checks em todos os métodos
    ✅ Código mais limpo e menos propenso a NullReferenceException
    ✅ Propriedade pública retorna lista vazia (não null) - melhor UX para consumidores

    CUSTO DE ALOCAÇÃO É IRRELEVANTE:
    ✅ Lista vazia: ~24-40 bytes (header do objeto + ponteiro interno)
    ✅ Comparação com operações comuns que custam MUITO mais:
       - `.ToList()` em qualquer LINQ: aloca nova lista + copia elementos
       - `.Where().Select()`: aloca iteradores + closures
       - String interpolation: aloca novas strings
       - Boxing de value types: aloca no heap
       - Entity Framework queries: centenas de alocações por query

    A micro-otimização de nullable não compensa a complexidade adicionada.

    ───────────────────────────────────────────────────────────────────────────────
    */
    public IReadOnlyList<CompositeChildEntity> CompositeChildEntities
    {
        get
        {
            return _compositeChildEntityCollection.AsReadOnly();
        }
    }

    // Constructors
    private CompositeAggregateRoot()
    {
    }

    private CompositeAggregateRoot(
        EntityInfo entityInfo,
        string name,
        string code,
        IEnumerable<CompositeChildEntity> compositeChildEntities
    ) : base(entityInfo)
    {
        Name = name;
        Code = code;
        /*
        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: Defensive Copy de Coleções no Construtor
        ───────────────────────────────────────────────────────────────────────────────

        O construtor DEVE criar uma cópia defensiva da coleção recebida via `.ToList()`.

        POR QUE IEnumerable<T> NO PARÂMETRO:
        ✅ Flexibilidade: aceita List, Array, IReadOnlyList, etc.
        ✅ Não força o chamador a converter para tipo específico

        POR QUE .ToList() NA ATRIBUIÇÃO:
        ✅ Cria nova instância - entidade é dona exclusiva da lista interna
        ✅ Quebra referência com código externo
        ✅ Impede modificações externas via referência compartilhada

        ❌ NUNCA atribua o parâmetro diretamente ao field:
           _compositeChildEntities = (List<T>)compositeChildEntities; // ERRADO!
           Código externo manteria referência e poderia modificar a coleção.

        ───────────────────────────────────────────────────────────────────────────────
        */
        _compositeChildEntityCollection = [.. compositeChildEntities];
    }

    // Public Business Methods
    public static CompositeAggregateRoot? RegisterNew(
        ExecutionContext executionContext,
        RegisterNewInput input
    )
    {
        return RegisterNewInternal(
            executionContext,
            input,
            entityFactory: (executionContext, input) => new CompositeAggregateRoot(),
            handler: static (executionContext, input, instance) =>
            {
                /*
                Orientações para LLM:

                Quando a operação envolve entidades filhas, primeiro nós
                alteramos todo o estado da Aggregate Root (ex: Name, Code),
                e depois iteramos sobre a coleção de entidades filhas para adicioná-las.                
                */
                bool isValid =
                    instance.ChangeNameInternal(executionContext, input.Name)
                    & instance.ChangeCodeInternal(executionContext, input.Code);

                /*
                ───────────────────────────────────────────────────────────────────────────────
                LLM_RULE: Processamento de Entidades Filhas Uma a Uma com Método Específico
                ───────────────────────────────────────────────────────────────────────────────

                Entidades filhas DEVEM ser processadas individualmente, não em lote.

                POR QUE PROCESSAR UMA A UMA:
                ✅ Estado da Aggregate Root pode influenciar validação de cada filha
                ✅ Regras de negócio podem variar por operação (RegisterNew vs outras)
                ✅ Permite coletar todas as mensagens de validação (não falha na primeira)

                PADRÃO DE NOMENCLATURA DO MÉTODO:
                Process[NomeDaEntidadeFilha]For[NomeDaOperação]Internal
                Exemplo: ProcessCompositeChildEntityForRegisterNewInternal

                POR QUE USAR &= (AND com atribuição):
                ✅ Continua iterando mesmo se uma adição falhar
                ✅ Coleta todas as mensagens de validação de todas as entidades
                ✅ Operação em clone - estado original inalterado se falhar

                ───────────────────────────────────────────────────────────────────────────────
                */
                if (input.ChildRegisterNewInputCollection != null)
                {
                    foreach (ChildRegisterNewInput childRegisterNewInput in input.ChildRegisterNewInputCollection)
                    {
                        isValid &= instance.ProcessCompositeChildEntityForRegisterNewInternal(
                            executionContext,
                            childRegisterNewInput
                        );
                    }
                }

                return isValid;
            }
        );
    }

    public static CompositeAggregateRoot CreateFromExistingInfo(
        CreateFromExistingInfoInput input
    )
    {
        return new CompositeAggregateRoot(
            input.EntityInfo,
            input.Name,
            input.Code,
            input.CompositeChildEntities
        );
    }

    public CompositeAggregateRoot? ChangeName(
        ExecutionContext executionContext,
        ChangeNameInput input
    )
    {
        return RegisterChangeInternal<CompositeAggregateRoot, ChangeNameInput>(
            executionContext,
            instance: this,
            input,
            handler: (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeNameInternal(executionContext, input.Name);
            }
        );
    }

    public CompositeAggregateRoot? ChangeCode(
        ExecutionContext executionContext,
        ChangeCodeInput input
    )
    {
        return RegisterChangeInternal<CompositeAggregateRoot, ChangeCodeInput>(
            executionContext,
            instance: this,
            input,
            handler: (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeCodeInternal(executionContext, input.Code);
            }
        );
    }

    public CompositeAggregateRoot? ChangeCompositeChildEntityTitle(
        ExecutionContext executionContext,
        ChangeCompositeChildEntityTitleInput input
    )
    {
        return RegisterChangeInternal<CompositeAggregateRoot, ChangeCompositeChildEntityTitleInput>(
            executionContext,
            instance: this,
            input,
            handler: (executionContext, input, newInstance) =>
            {
                return newInstance.ChangeCompositeChildEntityTitleInternal(
                    executionContext,
                    input.CompositeChildEntityId,
                    input.Title
                );
            }
        );
    }

    public override CompositeAggregateRoot Clone()
    {
        return new CompositeAggregateRoot(
            EntityInfo,
            Name,
            Code,
            _compositeChildEntityCollection
        );
    }

    // Private Business Methods
    private bool ChangeNameInternal(
        ExecutionContext executionContext,
        string name
    )
    {
        bool isSuccess = SetName(executionContext, name);

        return isSuccess;
    }

    private bool ChangeCodeInternal(
        ExecutionContext executionContext,
        string code
    )
    {
        bool isSuccess = SetCode(executionContext, code);

        return isSuccess;
    }

    private bool ChangeCompositeChildEntityTitleInternal(
        ExecutionContext executionContext,
        Guid compositeChildEntityId,
        string title
    )
    {
        bool isValid = ProcessCompositeChildEntityForChangeTitleInternal(
            executionContext,
            compositeChildEntityId,
            title
        );

        return isValid;
    }

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Método Privado de Processamento para Cada Entidade Filha
    ───────────────────────────────────────────────────────────────────────────────

    Para cada operação de negócio que envolve entidades filhas, DEVE existir um
    método privado específico de processamento.

    RESPONSABILIDADES DO MÉTODO:
    ✅ Receber o input específico para criar/atualizar a entidade filha
    ✅ Fazer o registro da entidade filha (RegisterNew, etc.)
    ✅ Validar a entidade filha no contexto da operação
    ✅ Adicionar na coleção interna se tudo estiver ok

    PADRÃO DE NOMENCLATURA:
    Process[NomeDaEntidadeFilha]For[NomeDaOperação]Internal
    Exemplo: ProcessCompositeChildEntityForRegisterNewInternal

    ───────────────────────────────────────────────────────────────────────────────
    */
    private bool ProcessCompositeChildEntityForRegisterNewInternal(
        ExecutionContext executionContext,
        ChildRegisterNewInput childRegisterNewInput
    )
    {
        /*
        ───────────────────────────────────────────────────────────────────────────────
        LLM_NOTE: Padrão "Criar Primeiro, Validar Depois"
        ───────────────────────────────────────────────────────────────────────────────

        O fluxo padrão é criar a entidade filha e depois validá-la no contexto da operação.

        ALTERNATIVA MAIS EFICIENTE (não usada por padrão):
        ✅ Validar o input antes de criar a entidade (ex: duplicidade via HashSet<string>)
        ✅ Evita alocação de objetos que serão descartados

        POR QUE MANTEMOS O PADRÃO "CRIAR PRIMEIRO":
        ✅ Código mais simples e uniforme
        ✅ Validação contextual pode precisar da entidade completa
        ✅ Otimizações pontuais podem ser feitas caso a caso quando necessário

        ───────────────────────────────────────────────────────────────────────────────
        */
        var registeredChild = CompositeChildEntity.RegisterNew(
            executionContext,
            childRegisterNewInput
        );

        if (registeredChild is null)
            return false;

        /*
        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: Validação Específica da Operação Após Criação
        ───────────────────────────────────────────────────────────────────────────────

        Após criar a entidade filha, DEVE-SE validá-la no contexto da operação atual.

        FLUXO DE VALIDAÇÃO:
        ✅ Chamar método de validação específico para a operação (ex: RegisterNew)
        ✅ Se validação falhar, retornar false SEM adicionar à coleção
        ✅ Método de validação segue padrão: Validate[Entidade]For[Operação]Internal

        POR QUE VALIDAÇÃO ESPECÍFICA POR OPERAÇÃO:
        ✅ Regras podem variar entre RegisterNew, Update, etc.
        ✅ Mantém coesão: processamento e validação da mesma operação juntos

        ───────────────────────────────────────────────────────────────────────────────
        */
        bool isValid = ValidateCompositeChildEntityForRegisterNewInternal(
            executionContext,
            registeredChild
        );

        if (!isValid)
            return false;

        _compositeChildEntityCollection.Add(registeredChild);

        return true;
    }

    private bool ProcessCompositeChildEntityForChangeTitleInternal(
        ExecutionContext executionContext,
        Guid compositeChildEntityId,
        string title
    )
    {
        /*
        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: Localização de Entidade Filha por Id
        ───────────────────────────────────────────────────────────────────────────────

        Antes de modificar uma entidade filha, DEVE-SE localizá-la na coleção pelo Id.

        FLUXO DE LOCALIZAÇÃO:
        ✅ Buscar entidade filha na coleção pelo Id
        ✅ Se não encontrar, adicionar mensagem de erro e retornar false
        ✅ Se encontrar, prosseguir com a operação

        ───────────────────────────────────────────────────────────────────────────────
        */
        CompositeChildEntity? existingChild = null;
        int existingChildIndex = -1;

        for (int i = 0; i < _compositeChildEntityCollection.Count; i++)
        {
            if (_compositeChildEntityCollection[i].EntityInfo.Id.Value == compositeChildEntityId)
            {
                existingChild = _compositeChildEntityCollection[i];
                existingChildIndex = i;
                break;
            }
        }

        if (existingChild is null)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<CompositeAggregateRoot>(propertyName: CompositeAggregateRootMetadata.CompositeChildEntitiesPropertyName)}.NotFound"
            );

            return false;
        }

        /*
        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: Modificação de Entidade Filha Via Método de Negócio Dela
        ───────────────────────────────────────────────────────────────────────────────

        A modificação da entidade filha DEVE ser feita chamando o método de negócio
        da própria entidade filha (ex: ChangeTitle), que segue o padrão clone-modify-return.

        FLUXO DE MODIFICAÇÃO:
        ✅ Chamar método de negócio da entidade filha (retorna nova instância ou null)
        ✅ Se retornar null, a validação falhou - retornar false
        ✅ Se retornar nova instância, prosseguir com validação contextual

        ───────────────────────────────────────────────────────────────────────────────
        */
        CompositeChildEntity? updatedChild = existingChild.ChangeTitle(
            executionContext,
            new ChildChangeTitleInput(title)
        );

        if (updatedChild is null)
            return false;

        bool isValid = ValidateCompositeChildEntityForChangeTitleInternal(
            executionContext,
            updatedChild,
            existingChildIndex
        );

        if (!isValid)
            return false;

        _compositeChildEntityCollection[existingChildIndex] = updatedChild;

        return true;
    }

    // Validation Methods
    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Validação de Entidades Relacionadas é Opcional no IsValid Estático
    ───────────────────────────────────────────────────────────────────────────────

    O método IsValid SEMPRE valida todos os campos da Aggregate Root.
    Apenas a validação de entidades relacionadas (composição, associação) é opcional.

    VALIDAÇÃO OBRIGATÓRIA (campos da entidade):
    ✅ EntityInfo, Name, Code - sempre validados

    VALIDAÇÃO OPCIONAL (entidades relacionadas):
    ✅ Parâmetro nullable com default null
    ✅ Se fornecido: valida cada entidade relacionada via IsValid()
    ✅ Se null: ignora validação de relacionamentos

    POR QUE RELACIONAMENTOS SÃO OPCIONAIS:
    ✅ Fail-Fast para camadas externas sem exigir grafo completo
    ✅ Chamador decide se quer validar apenas a entidade ou incluir relacionamentos
    ✅ Útil quando relacionamentos ainda não foram carregados

    ───────────────────────────────────────────────────────────────────────────────
    */
    public static bool IsValid(
        ExecutionContext executionContext,
        EntityInfo entityInfo,
        string? name,
        string? code,
        IEnumerable<CompositeChildEntity>? compositeChildEntityCollection
    )
    {
        bool isValid =
            EntityBaseIsValid(executionContext, entityInfo)
            & ValidateName(executionContext, name)
            & ValidateCode(executionContext, code);

        if (compositeChildEntityCollection is not null)
        {
            foreach (CompositeChildEntity compositeChildEntity in compositeChildEntityCollection)
            {
                isValid &= compositeChildEntity.IsValid(executionContext);
            }
        }

        return isValid;
    }

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Método de Instância IsValid SEMPRE Valida Entidades Relacionadas
    ───────────────────────────────────────────────────────────────────────────────

    Diferente do método estático, o método de instância SEMPRE valida o grafo completo.

    DIFERENÇA ENTRE OS MÉTODOS:
    - Estático: validação de valores individuais, relacionamentos opcionais (Fail-Fast)
    - Instância: estado completo já carregado, valida tudo automaticamente

    POR QUE VALIDAR TUDO NA INSTÂNCIA:
    ✅ Aggregate Root é responsável pela consistência do agregado inteiro
    ✅ Estado completo já está disponível na memória
    ✅ Não faz sentido ignorar relacionamentos quando já estão carregados

    ───────────────────────────────────────────────────────────────────────────────
    */
    protected override bool IsValidInternal(
        ExecutionContext executionContext
    )
    {
        return IsValid(
            executionContext,
            EntityInfo,
            Name,
            Code,
            _compositeChildEntityCollection
        );
    }

    public static bool ValidateName(
        ExecutionContext executionContext,
        string? name
    )
    {
        bool nameIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<CompositeAggregateRoot>(propertyName: CompositeAggregateRootMetadata.NamePropertyName),
            isRequired: CompositeAggregateRootMetadata.NameIsRequired,
            value: name
        );

        if (!nameIsRequiredValidation)
            return false;

        bool nameMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<CompositeAggregateRoot>(propertyName: CompositeAggregateRootMetadata.NamePropertyName),
            minLength: CompositeAggregateRootMetadata.NameMinLength,
            value: name!.Length
        );

        bool nameMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<CompositeAggregateRoot>(propertyName: CompositeAggregateRootMetadata.NamePropertyName),
            maxLength: CompositeAggregateRootMetadata.NameMaxLength,
            value: name!.Length
        );

        return nameIsRequiredValidation
            && nameMinLengthValidation
            && nameMaxLengthValidation;
    }

    public static bool ValidateCode(
        ExecutionContext executionContext,
        string? code
    )
    {
        bool codeIsRequiredValidation = ValidationUtils.ValidateIsRequired(
            executionContext,
            propertyName: CreateMessageCode<CompositeAggregateRoot>(propertyName: CompositeAggregateRootMetadata.CodePropertyName),
            isRequired: CompositeAggregateRootMetadata.CodeIsRequired,
            value: code
        );

        if (!codeIsRequiredValidation)
            return false;

        bool codeMinLengthValidation = ValidationUtils.ValidateMinLength(
            executionContext,
            propertyName: CreateMessageCode<CompositeAggregateRoot>(propertyName: CompositeAggregateRootMetadata.CodePropertyName),
            minLength: CompositeAggregateRootMetadata.CodeMinLength,
            value: code!.Length
        );

        bool codeMaxLengthValidation = ValidationUtils.ValidateMaxLength(
            executionContext,
            propertyName: CreateMessageCode<CompositeAggregateRoot>(propertyName: CompositeAggregateRootMetadata.CodePropertyName),
            maxLength: CompositeAggregateRootMetadata.CodeMaxLength,
            value: code!.Length
        );

        return codeIsRequiredValidation
            && codeMinLengthValidation
            && codeMaxLengthValidation;
    }

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Validação de Entidade Filha Específica por Operação
    ───────────────────────────────────────────────────────────────────────────────

    Toda entidade filha adicionada à coleção DEVE ser validada individualmente durante o processamento.

    POR QUE VALIDAÇÃO ESPECÍFICA POR OPERAÇÃO:
    ✅ Regras de validação podem variar conforme a operação de negócio (ex: RegisterNew vs Update)
    ✅ Método privado garante encapsulamento das regras específicas
    ✅ Aggregate Root mantém controle total sobre validação do agregado

    PADRÃO DE NOMENCLATURA:
    Validate[NomeDaEntidadeFilha]For[NomeDaOperação]Internal
    Exemplo: ValidateCompositeChildEntityForRegisterNewInternal

    ───────────────────────────────────────────────────────────────────────────────
    */
    public bool ValidateCompositeChildEntityForRegisterNewInternal(
        ExecutionContext executionContext,
        CompositeChildEntity compositeChildEntity
    )
    {
        /*
        ───────────────────────────────────────────────────────────────────────────────
        LLM_NOTE: Simplicidade Sobre Otimização Prematura (Exemplo Didático)
        ───────────────────────────────────────────────────────────────────────────────

        O algoritmo abaixo é O(n) para verificar duplicidade de títulos.

        ALTERNATIVA MAIS EFICIENTE (não implementada aqui):
        ✅ Usar HashSet<string> no método público para coletar títulos únicos
        ✅ Verificar duplicidade no próprio input antes de processar
        ✅ Depois checar contra _compositeChildEntityCollection

        POR QUE MANTEMOS A VERSÃO SIMPLES:
        ✅ Este é um exemplo didático - clareza > performance
        ✅ Para coleções pequenas, a diferença é irrelevante
        ✅ Otimização prematura adiciona complexidade desnecessária

        ───────────────────────────────────────────────────────────────────────────────
        */
        bool hasDuplicatedTitle = false;
        foreach (CompositeChildEntity existingCompositeChildEntity in _compositeChildEntityCollection)
        {
            if (existingCompositeChildEntity.Title == compositeChildEntity.Title)
            {
                hasDuplicatedTitle = true;
                break;
            }
        }

        if (hasDuplicatedTitle)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<CompositeAggregateRoot>(propertyName: CompositeAggregateRootMetadata.CompositeChildEntitiesPropertyName)}.DuplicateTitle"
            );

            return false;
        }

        return compositeChildEntity.IsValid(executionContext);
    }

    private bool ValidateCompositeChildEntityForChangeTitleInternal(
        ExecutionContext executionContext,
        CompositeChildEntity compositeChildEntity,
        int currentIndex
    )
    {
        /*
        ───────────────────────────────────────────────────────────────────────────────
        LLM_RULE: Validação de Duplicidade Deve Ignorar a Própria Entidade
        ───────────────────────────────────────────────────────────────────────────────

        Ao validar duplicidade durante uma operação de alteração, DEVE-SE ignorar
        a própria entidade sendo alterada (identificada pelo índice).

        POR QUE IGNORAR A PRÓPRIA ENTIDADE:
        ✅ Se o título não mudou, não é duplicidade consigo mesma
        ✅ O índice identifica a posição da entidade original na coleção
        ✅ Comparamos apenas com as outras entidades da coleção

        ───────────────────────────────────────────────────────────────────────────────
        */
        bool hasDuplicatedTitle = false;

        for (int i = 0; i < _compositeChildEntityCollection.Count; i++)
        {
            if (i == currentIndex)
                continue;

            if (_compositeChildEntityCollection[i].Title == compositeChildEntity.Title)
            {
                hasDuplicatedTitle = true;
                break;
            }
        }

        if (hasDuplicatedTitle)
        {
            executionContext.AddErrorMessage(
                code: $"{CreateMessageCode<CompositeAggregateRoot>(propertyName: CompositeAggregateRootMetadata.CompositeChildEntitiesPropertyName)}.DuplicateTitle"
            );

            return false;
        }

        return compositeChildEntity.IsValid(executionContext);
    }

    // Set Methods
    private bool SetName(
        ExecutionContext executionContext,
        string name
    )
    {
        bool isValid = ValidateName(
            executionContext,
            name
        );

        if (!isValid)
            return false;

        Name = name;

        return true;
    }

    private bool SetCode(
        ExecutionContext executionContext,
        string code
    )
    {
        bool isValid = ValidateCode(
            executionContext,
            code
        );

        if (!isValid)
            return false;

        Code = code;

        return true;
    }

    /*
    ───────────────────────────────────────────────────────────────────────────────
    LLM_RULE: Coleções de Entidades Filhas NÃO Têm Método Set*
    ───────────────────────────────────────────────────────────────────────────────

    Coleções filhas são gerenciadas exclusivamente pela Aggregate Root.

    ✅ Use métodos de negócio específicos: AddChild(), RemoveChild(), etc.
    ✅ Cada operação passa por validação e regras de negócio
    ✅ Aggregate Root mantém controle total sobre a coleção

    ❌ NUNCA exponha SetCompositeChildEntities() - substituição direta viola invariantes
    ❌ NUNCA permita atribuição externa da coleção inteira

    RAZÃO: A Aggregate Root é responsável por garantir consistência do agregado.
    Permitir substituição direta da coleção bypassa validações e regras de negócio.

    ───────────────────────────────────────────────────────────────────────────────
    */
}
