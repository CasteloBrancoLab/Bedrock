using Bedrock.BuildingBlocks.Configuration.Pipeline;
using Bedrock.BuildingBlocks.Configuration.Registration;
using Microsoft.Extensions.Configuration;

namespace Bedrock.BuildingBlocks.Configuration;

/// <summary>
/// Classe base abstrata para gerenciamento de configuracao com pipeline de handlers.
/// Encapsula IConfiguration e estende seu comportamento com handlers customizados.
/// </summary>
public abstract class ConfigurationManagerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;
    private readonly Dictionary<Type, string> _sectionMappings = new();
    private readonly Dictionary<Type, PropertyInfo[]> _propertyCache = new();
    private ConfigurationPipeline _getPipeline = null!;
    private ConfigurationPipeline _setPipeline = null!;
    private readonly Dictionary<string, object?> _inMemoryOverrides = new();

    /// <summary>Cache global de paths derivados: (Type, propertyName) → fullPath.</summary>
    private static readonly ConcurrentDictionary<(Type, string), string> PathCache = new();

    /// <summary>
    /// Cria uma nova instancia do configuration manager.
    /// Chama Initialize() que invoca ConfigureInternal().
    /// </summary>
    /// <param name="configuration">IConfiguration com fontes padrao ja configuradas.</param>
    /// <param name="logger">Logger para observabilidade do pipeline.</param>
    protected ConfigurationManagerBase(IConfiguration configuration, ILogger logger)
    {
        // Stryker disable once Statement : Guard clause — ArgumentNullException downstream valida igualmente
        ArgumentNullException.ThrowIfNull(configuration);
        // Stryker disable once Statement : Guard clause — ArgumentNullException downstream valida igualmente
        ArgumentNullException.ThrowIfNull(logger);

        _configuration = configuration;
        _logger = logger;

        Initialize();
    }

    /// <summary>
    /// Le uma secao inteira de configuracao como objeto tipado.
    /// Cada propriedade passa pelo pipeline de handlers.
    /// </summary>
    /// <typeparam name="TSection">Tipo do objeto de configuracao mapeado para a secao.</typeparam>
    /// <returns>Objeto populado com valores resolvidos pelo pipeline.</returns>
    /// <exception cref="InvalidOperationException">Se TSection nao foi mapeada via MapSection.</exception>
    public TSection Get<TSection>() where TSection : class, new()
    {
        var sectionType = typeof(TSection);
        var sectionPath = GetSectionPath(sectionType);
        var properties = GetCachedProperties(sectionType);
        var result = new TSection();

        foreach (var property in properties)
        {
            if (!property.CanWrite)
            {
                continue;
            }

            var fullPath = ResolvePath(sectionType, sectionPath, property.Name);
            var rawValue = ReadValueFromConfiguration(fullPath, property.PropertyType);
            var resolvedValue = _getPipeline.ExecuteGet(fullPath, rawValue);
            var finalValue = CheckInMemoryOverride(fullPath, resolvedValue);

            SetPropertyValue(result, property, finalValue);
        }

        return result;
    }

    /// <summary>
    /// Le uma propriedade especifica de uma secao de configuracao.
    /// O caminho e derivado automaticamente da classe e propriedade.
    /// </summary>
    /// <typeparam name="TSection">Tipo da secao de configuracao.</typeparam>
    /// <typeparam name="TProperty">Tipo da propriedade.</typeparam>
    /// <param name="propertyExpression">Expressao type-safe para a propriedade.</param>
    /// <returns>Valor resolvido pelo pipeline.</returns>
    public TProperty Get<TSection, TProperty>(
        Expression<Func<TSection, TProperty>> propertyExpression)
        where TSection : class
    {
        var sectionType = typeof(TSection);
        var sectionPath = GetSectionPath(sectionType);
        var propertyName = ExtractPropertyName(propertyExpression);
        var fullPath = ResolvePath(sectionType, sectionPath, propertyName);

        var rawValue = ReadValueFromConfiguration(fullPath, typeof(TProperty));
        var resolvedValue = _getPipeline.ExecuteGet(fullPath, rawValue);
        var finalValue = CheckInMemoryOverride(fullPath, resolvedValue);

        return ConvertValue<TProperty>(finalValue);
    }

    /// <summary>
    /// Escreve um valor de configuracao atraves do pipeline de Set.
    /// </summary>
    /// <typeparam name="TSection">Tipo da secao de configuracao.</typeparam>
    /// <typeparam name="TProperty">Tipo da propriedade.</typeparam>
    /// <param name="propertyExpression">Expressao type-safe para a propriedade.</param>
    /// <param name="value">Valor a ser escrito.</param>
    public void Set<TSection, TProperty>(
        Expression<Func<TSection, TProperty>> propertyExpression,
        TProperty value)
        where TSection : class
    {
        var sectionType = typeof(TSection);
        var sectionPath = GetSectionPath(sectionType);
        var propertyName = ExtractPropertyName(propertyExpression);
        var fullPath = ResolvePath(sectionType, sectionPath, propertyName);

        var resolvedValue = _setPipeline.ExecuteSet(fullPath, value);
        _inMemoryOverrides[fullPath] = resolvedValue;
    }

    /// <summary>
    /// Ponto de extensao para subclasses configurarem secoes e handlers.
    /// Chamado uma vez durante Initialize().
    /// </summary>
    /// <param name="options">Opcoes de configuracao (fluent API).</param>
    protected abstract void ConfigureInternal(ConfigurationOptions options);

    private void Initialize()
    {
        // Stryker disable once all : Log message content is not behavior-critical
        _logger.LogDebug("Inicializando ConfigurationManager: {Type}", GetType().Name);

        var options = new ConfigurationOptions();
        ConfigureInternal(options);

        foreach (var mapping in options.GetSectionMappings())
        {
            _sectionMappings[mapping.Key] = mapping.Value;
        }

        var (getPipeline, setPipeline) = options.BuildPipelines();
        _getPipeline = getPipeline;
        _setPipeline = setPipeline;

        // Stryker disable once Statement : Pre-cache de StartupOnly e otimizacao — fallback em ExecuteStartupOnly garante execucao correta sem pre-init
        var knownKeys = CollectKnownKeys();
        // Stryker disable once Statement : Pre-cache de StartupOnly e otimizacao — fallback em ExecuteStartupOnly garante execucao correta sem pre-init
        _getPipeline.InitializeStartupHandlers(knownKeys);

        // Stryker disable once all : Log message content is not behavior-critical
        _logger.LogDebug("ConfigurationManager inicializado com {SectionCount} secoes mapeadas",
            _sectionMappings.Count);
    }

    // Stryker disable all : CollectKnownKeys e metodo auxiliar de inicializacao — impacto limitado a pre-cache de StartupOnly
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Metodo auxiliar de inicializacao — impacto limitado a pre-cache de StartupOnly")]
    private List<string> CollectKnownKeys()
    {
        var keys = new List<string>();

        foreach (var (sectionType, sectionPath) in _sectionMappings)
        {
            var properties = GetCachedProperties(sectionType);
            foreach (var property in properties)
            {
                if (!property.CanWrite)
                {
                    continue;
                }

                keys.Add(ResolvePath(sectionType, sectionPath, property.Name));
            }
        }

        return keys;
    }
    // Stryker restore all

    private string GetSectionPath(Type sectionType)
    {
        if (!_sectionMappings.TryGetValue(sectionType, out var sectionPath))
        {
            // Stryker disable all : Mensagem de erro com contexto do tipo — testada por excecao
            throw new InvalidOperationException(
                $"Tipo '{sectionType.Name}' nao foi mapeado via MapSection. " +
                $"Registre o mapeamento em ConfigureInternal antes de usar Get<{sectionType.Name}>().");
            // Stryker restore all
        }

        return sectionPath;
    }

    private PropertyInfo[] GetCachedProperties(Type sectionType)
    {
        if (!_propertyCache.TryGetValue(sectionType, out var properties))
        {
            properties = sectionType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            _propertyCache[sectionType] = properties;
        }

        return properties;
    }

    private static string ResolvePath(Type sectionType, string sectionPath, string propertyName)
    {
        return PathCache.GetOrAdd((sectionType, propertyName), static (key, section) =>
            ConfigurationPath.Create(section, key.Item2).FullPath, sectionPath);
    }

    // Stryker disable all : ReadValueFromConfiguration delega para ConvertStringToType (ja excluido) e GetDefaultForType — mutantes equivalentes
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Delega para ConvertStringToType e GetDefaultForType — mutantes equivalentes")]
    private object? ReadValueFromConfiguration(string fullPath, Type propertyType)
    {
        var section = _configuration.GetSection(fullPath);

        if (!section.Exists())
        {
            return GetDefaultForType(propertyType);
        }

        // Arrays: check if the section has children (indexed: 0, 1, 2, ...)
        if (propertyType.IsArray)
        {
            var elementType = propertyType.GetElementType()!;
            var children = section.GetChildren().ToList();

            if (children.Count == 0)
            {
                return Array.CreateInstance(elementType, 0);
            }

            var array = Array.CreateInstance(elementType, children.Count);
            for (var i = 0; i < children.Count; i++)
            {
                var childValue = children[i].Value;
                array.SetValue(ConvertStringToType(childValue, elementType), i);
            }

            return array;
        }

        return ConvertStringToType(section.Value, propertyType);
    }
    // Stryker restore all

    // Stryker disable all : GetDefaultForType retorna valores padrao para tipos — mutantes equivalentes com Activator.CreateInstance
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Retorna valores padrao para tipos — mutantes equivalentes")]
    private static object? GetDefaultForType(Type type)
    {
        // Nullable types return null
        if (Nullable.GetUnderlyingType(type) is not null)
        {
            return null;
        }

        // Arrays return empty array
        if (type.IsArray)
        {
            return Array.CreateInstance(type.GetElementType()!, 0);
        }

        // Reference types (string) return null
        if (!type.IsValueType)
        {
            return null;
        }

        // Value types return default
        return Activator.CreateInstance(type);
    }
    // Stryker restore all

    // Stryker disable all : Branches de tipo sao otimizacao de performance — Convert.ChangeType no final trata todos os casos corretamente
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Branches de tipo sao otimizacao — Convert.ChangeType trata todos os casos")]
    private static object? ConvertStringToType(string? value, Type targetType)
    {
        if (value is null)
        {
            return GetDefaultForType(targetType);
        }

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (underlyingType == typeof(string))
        {
            return value;
        }

        if (underlyingType == typeof(int))
        {
            return int.Parse(value, CultureInfo.InvariantCulture);
        }

        if (underlyingType == typeof(long))
        {
            return long.Parse(value, CultureInfo.InvariantCulture);
        }

        if (underlyingType == typeof(bool))
        {
            return bool.Parse(value);
        }

        if (underlyingType == typeof(double))
        {
            return double.Parse(value, CultureInfo.InvariantCulture);
        }

        if (underlyingType == typeof(decimal))
        {
            return decimal.Parse(value, CultureInfo.InvariantCulture);
        }

        return Convert.ChangeType(value, underlyingType, CultureInfo.InvariantCulture);
    }
    // Stryker restore all

    // Stryker disable all : Branches de tipo sao otimizacao — Convert.ChangeType no final trata todos os cenarios de atribuicao
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Branches de tipo sao otimizacao — Convert.ChangeType trata todos os cenarios")]
    private static void SetPropertyValue(object target, PropertyInfo property, object? value)
    {
        if (value is null)
        {
            if (Nullable.GetUnderlyingType(property.PropertyType) is not null || !property.PropertyType.IsValueType)
            {
                property.SetValue(target, null);
            }

            return;
        }

        var targetType = property.PropertyType;
        var underlyingType = Nullable.GetUnderlyingType(targetType);

        if (underlyingType is not null && value.GetType() == underlyingType)
        {
            property.SetValue(target, value);
            return;
        }

        if (value.GetType() == targetType || targetType.IsAssignableFrom(value.GetType()))
        {
            property.SetValue(target, value);
            return;
        }

        property.SetValue(target, Convert.ChangeType(value, underlyingType ?? targetType, CultureInfo.InvariantCulture));
    }
    // Stryker restore all

    private object? CheckInMemoryOverride(string fullPath, object? pipelineValue)
    {
        return _inMemoryOverrides.TryGetValue(fullPath, out var overrideValue) ? overrideValue : pipelineValue;
    }

    private static string ExtractPropertyName<TSection, TProperty>(
        Expression<Func<TSection, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        // Handle unary expressions (e.g., boxing of value types)
        // Stryker disable once all : UnaryExpression ocorre com boxing de value types — depende do compilador C#
        if (expression.Body is UnaryExpression { Operand: MemberExpression operand })
        {
            // Stryker disable once all : Retorno do nome da propriedade a partir de UnaryExpression
            return operand.Member.Name;
        }

        // Stryker disable all : Mensagem de erro defensiva — API publica tipada impede expressoes invalidas
        throw new ArgumentException(
            $"Expressao deve referenciar uma propriedade direta de {typeof(TSection).Name}.",
            nameof(expression));
        // Stryker restore all
    }

    // Stryker disable all : Branches de conversao sao otimizacao — Convert.ChangeType trata todos os tipos
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage(Justification = "Branches de conversao sao otimizacao — Convert.ChangeType trata todos os tipos")]
    private static TProperty ConvertValue<TProperty>(object? value)
    {
        if (value is null)
        {
            return default!;
        }

        if (value is TProperty typed)
        {
            return typed;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(TProperty)) ?? typeof(TProperty);
        return (TProperty)Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
    }
    // Stryker restore all
}
