using Microsoft.AspNetCore.OutputCaching;

namespace Bedrock.BuildingBlocks.Web.WebApi.OutputCaching.Models;

// Configuracao fluente para output caching do Bedrock.
//
// Permite definir politicas nomeadas com duracao e vary-by-query,
// ou controle total via callback do OutputCachePolicyBuilder.
//
// Uso nas controllers via [OutputCache(PolicyName = "...")] nas actions.
//
// Uso tipico:
//   new BedrockOutputCachingOptions()
//       .WithDefaultExpiration(TimeSpan.FromMinutes(5))
//       .AddPolicy("short", TimeSpan.FromSeconds(30))
//       .AddPolicy("custom", builder => builder.Expire(TimeSpan.FromHours(1)).Tag("products"))
public sealed class BedrockOutputCachingOptions
{
    internal TimeSpan DefaultExpiration { get; private set; } = TimeSpan.FromSeconds(60);
    internal List<OutputCachePolicy> Policies { get; } = [];
    internal Action<OutputCacheOptions>? ConfigureOutputCache { get; private set; }

    // Define a expiracao default para policies que nao especificam duracao.
    // Default: 60 segundos.
    public BedrockOutputCachingOptions WithDefaultExpiration(TimeSpan expiration)
    {
        DefaultExpiration = expiration;
        return this;
    }

    // Adiciona uma policy nomeada simples com duracao e vary-by-query configuravel.
    // varyByQuery = true (default) cria cache entries separadas por query string.
    public BedrockOutputCachingOptions AddPolicy(
        string name,
        TimeSpan duration,
        bool varyByQuery = true)
    {
        Policies.Add(new OutputCachePolicy(name, duration, varyByQuery, null));
        return this;
    }

    // Adiciona uma policy nomeada com controle total via OutputCachePolicyBuilder.
    // Permite configurar tags, vary-by, predicados de cache, etc.
    public BedrockOutputCachingOptions AddPolicy(
        string name,
        Action<OutputCachePolicyBuilder> configure)
    {
        Policies.Add(new OutputCachePolicy(name, null, true, configure));
        return this;
    }

    // Callback para estender ou sobrescrever a configuracao do OutputCacheOptions
    // apos os defaults do Bedrock.
    public BedrockOutputCachingOptions Configure(Action<OutputCacheOptions> configure)
    {
        ConfigureOutputCache = configure;
        return this;
    }
}
