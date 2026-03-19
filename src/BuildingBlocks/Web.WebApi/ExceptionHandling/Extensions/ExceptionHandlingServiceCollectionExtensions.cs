using Bedrock.BuildingBlocks.Web.WebApi.ExceptionHandling.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.BuildingBlocks.Web.WebApi.ExceptionHandling.Extensions;

public static class ExceptionHandlingServiceCollectionExtensions
{
    // Registra o middleware de exception handling com os mapeamentos configurados.
    // O options e armazenado como singleton para ser consumido pelo middleware.
    public static IServiceCollection AddBedrockExceptionHandling(
        this IServiceCollection services,
        BedrockExceptionHandlingOptions? options = null)
    {
        services.AddSingleton(options ?? new BedrockExceptionHandlingOptions());
        return services;
    }
}
