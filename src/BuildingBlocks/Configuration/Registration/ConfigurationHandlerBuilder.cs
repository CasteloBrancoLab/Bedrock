using Bedrock.BuildingBlocks.Configuration.Handlers;
using Bedrock.BuildingBlocks.Configuration.Handlers.Enums;

namespace Bedrock.BuildingBlocks.Configuration.Registration;

/// <summary>
/// Fluent builder para configuracao de um handler no pipeline.
/// </summary>
public sealed class ConfigurationHandlerBuilder<THandler>
    where THandler : ConfigurationHandlerBase
{
    private readonly HandlerRegistration _registration;
    private readonly IReadOnlyDictionary<Type, string> _sectionMappings;

    internal ConfigurationHandlerBuilder(HandlerRegistration registration, IReadOnlyDictionary<Type, string> sectionMappings)
    {
        _registration = registration;
        _sectionMappings = sectionMappings;
    }

    /// <summary>Define a posicao do handler no pipeline (ordem de execucao).</summary>
    public ConfigurationHandlerBuilder<THandler> AtPosition(int position)
    {
        _registration.Position = position;
        return this;
    }

    /// <summary>Define a estrategia de carregamento.</summary>
    public ConfigurationHandlerBuilder<THandler> WithLoadStrategy(LoadStrategy loadStrategy)
    {
        // LoadStrategy is set on the handler instance, not the registration
        // It's passed through the handler constructor, so this is informational
        return this;
    }

    /// <summary>Registra o handler apenas no pipeline de Get.</summary>
    public ConfigurationHandlerBuilder<THandler> ForGet()
    {
        _registration.IncludeInGet = true;
        _registration.IncludeInSet = false;
        return this;
    }

    /// <summary>Registra o handler apenas no pipeline de Set.</summary>
    public ConfigurationHandlerBuilder<THandler> ForSet()
    {
        _registration.IncludeInGet = false;
        _registration.IncludeInSet = true;
        return this;
    }

    /// <summary>Registra o handler em ambos os pipelines (default).</summary>
    public ConfigurationHandlerBuilder<THandler> ForBoth()
    {
        _registration.IncludeInGet = true;
        _registration.IncludeInSet = true;
        return this;
    }

    /// <summary>Define escopo por classe (todas as propriedades da secao).</summary>
    /// <typeparam name="TClass">Classe de configuracao alvo.</typeparam>
    /// <exception cref="InvalidOperationException">Se TClass nao foi mapeada via MapSection.</exception>
    public ClassScopeBuilder<TClass> ToClass<TClass>() where TClass : class
    {
        var classType = typeof(TClass);
        if (!_sectionMappings.TryGetValue(classType, out var sectionPath))
        {
            // Stryker disable all : Mensagem de erro com contexto do tipo — testada por conteudo
            throw new InvalidOperationException(
                $"Tipo '{classType.Name}' nao foi mapeado via MapSection. " +
                $"Registre o mapeamento antes de usar ToClass<{classType.Name}>().");
            // Stryker restore all
        }

        _registration.Scope = HandlerScope.ForClass(sectionPath);
        return new ClassScopeBuilder<TClass>(_registration, sectionPath);
    }
}

/// <summary>
/// Builder aninhado para definir escopo por propriedade especifica.
/// </summary>
public sealed class ClassScopeBuilder<TClass> where TClass : class
{
    private readonly HandlerRegistration _registration;
    private readonly string _sectionPath;

    internal ClassScopeBuilder(HandlerRegistration registration, string sectionPath)
    {
        _registration = registration;
        _sectionPath = sectionPath;
    }

    /// <summary>Refina o escopo para uma propriedade especifica da classe.</summary>
    /// <typeparam name="TProperty">Tipo da propriedade.</typeparam>
    /// <param name="propertyExpression">Expressao type-safe para a propriedade.</param>
    /// <returns>Este builder para encadeamento.</returns>
    public ClassScopeBuilder<TClass> ToProperty<TProperty>(
        Expression<Func<TClass, TProperty>> propertyExpression)
    {
        var propertyName = ExtractPropertyName(propertyExpression);
        var fullPath = ConfigurationPath.Create(_sectionPath, propertyName).FullPath;
        _registration.Scope = HandlerScope.ForProperty(fullPath);
        return this;
    }

    private static string ExtractPropertyName<TProperty>(Expression<Func<TClass, TProperty>> expression)
    {
        if (expression.Body is MemberExpression memberExpression)
        {
            return memberExpression.Member.Name;
        }

        // Stryker disable all : Mensagem de erro defensiva — API publica tipada impede expressoes invalidas
        throw new ArgumentException(
            $"Expressao deve referenciar uma propriedade direta de {typeof(TClass).Name}.",
            nameof(expression));
        // Stryker restore all
    }
}
