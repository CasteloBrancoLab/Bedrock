using Bedrock.BuildingBlocks.Web.WebApi.Envelope.Translation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bedrock.BuildingBlocks.Web.WebApi.Envelope.Extensions;

public static class EnvelopeServiceCollectionExtensions
{
    // Registra o NullMessageTranslator como default e o MessageTranslationActionFilter.
    // O filter e registrado como transient para ser resolvido pelo MVC filter pipeline.
    public static IServiceCollection AddBedrockEnvelope(this IServiceCollection services)
    {
        services.TryAddSingleton<IMessageTranslator, NullMessageTranslator>();
        services.AddTransient<MessageTranslationActionFilter>();
        return services;
    }

    // Overload que permite registrar um translator customizado.
    public static IServiceCollection AddBedrockEnvelope<TTranslator>(this IServiceCollection services)
        where TTranslator : class, IMessageTranslator
    {
        services.AddSingleton<IMessageTranslator, TTranslator>();
        services.AddTransient<MessageTranslationActionFilter>();
        return services;
    }
}
