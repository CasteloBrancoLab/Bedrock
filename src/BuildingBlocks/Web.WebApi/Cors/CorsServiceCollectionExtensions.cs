using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.BuildingBlocks.Web.WebApi.Cors;

public static class CorsServiceCollectionExtensions
{
    // Registra as politicas CORS configuradas via fluent API.
    //
    // Cada BedrockCorsPolicyOptions e traduzida para uma CorsPolicyBuilder
    // do .NET, preservando a API fluente do Bedrock sem reexpor a complexidade
    // do CorsPolicyBuilder.
    //
    // Se SetDefaultPolicy foi chamado, a politica correspondente e aplicada
    // globalmente a todos os endpoints sem [EnableCors] explicito.
    public static IServiceCollection AddBedrockCors(
        this IServiceCollection services,
        BedrockCorsOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        services.AddCors(cors =>
        {
            foreach (var policy in options.Policies)
            {
                cors.AddPolicy(policy.Name, builder => ApplyPolicyOptions(builder, policy));
            }

            if (options.DefaultPolicyName is not null)
            {
                cors.DefaultPolicyName = options.DefaultPolicyName;
            }

            options.ConfigureCors?.Invoke(cors);
        });

        return services;
    }

    private static void ApplyPolicyOptions(CorsPolicyBuilder builder, BedrockCorsPolicyOptions policy)
    {
        ConfigureOrigins(builder, policy);
        ConfigureMethods(builder, policy);
        ConfigureHeaders(builder, policy);
        ConfigureExposedHeaders(builder, policy);
        ConfigureCredentials(builder, policy);
        ConfigurePreflightMaxAge(builder, policy);
    }

    private static void ConfigureOrigins(CorsPolicyBuilder builder, BedrockCorsPolicyOptions policy)
    {
        if (policy.Origins.Count > 0)
        {
            builder.WithOrigins([.. policy.Origins]);
        }
    }

    private static void ConfigureMethods(CorsPolicyBuilder builder, BedrockCorsPolicyOptions policy)
    {
        if (policy.Methods.Count > 0)
        {
            builder.WithMethods([.. policy.Methods]);
        }
    }

    private static void ConfigureHeaders(CorsPolicyBuilder builder, BedrockCorsPolicyOptions policy)
    {
        if (policy.Headers.Count > 0)
        {
            builder.WithHeaders([.. policy.Headers]);
        }
    }

    private static void ConfigureExposedHeaders(CorsPolicyBuilder builder, BedrockCorsPolicyOptions policy)
    {
        if (policy.ExposedHeaders.Count > 0)
        {
            builder.WithExposedHeaders([.. policy.ExposedHeaders]);
        }
    }

    // AllowCredentials e AllowAnyOrigin sao mutuamente exclusivos no .NET.
    // Se o consumidor configurou WithCredentials mas nao adicionou origens,
    // o .NET lanca excecao em runtime. O comentario no BedrockCorsPolicyOptions
    // ja documenta essa restricao.
    private static void ConfigureCredentials(CorsPolicyBuilder builder, BedrockCorsPolicyOptions policy)
    {
        if (policy.AllowCredentials)
        {
            builder.AllowCredentials();
        }
    }

    private static void ConfigurePreflightMaxAge(CorsPolicyBuilder builder, BedrockCorsPolicyOptions policy)
    {
        if (policy.PreflightMaxAge.HasValue)
        {
            builder.SetPreflightMaxAge(policy.PreflightMaxAge.Value);
        }
    }
}
