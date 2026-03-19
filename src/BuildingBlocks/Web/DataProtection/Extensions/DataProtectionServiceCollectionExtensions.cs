using Bedrock.BuildingBlocks.Web.DataProtection.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Bedrock.BuildingBlocks.Web.DataProtection.Extensions;

public static class DataProtectionServiceCollectionExtensions
{
    // Registra o Data Protection com os defaults do Bedrock.
    //
    // Configura:
    // - ApplicationName para isolamento de keys entre servicos
    // - KeyStoragePath se fornecido (filesystem/shared volume)
    // - KeyLifetime para rotacao automatica de keys
    // - Callback para extensao (Azure Blob, Redis, certificado, etc.)
    public static IServiceCollection AddBedrockDataProtection(
        this IServiceCollection services,
        BedrockDataProtectionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ApplicationName))
        {
            throw new InvalidOperationException(
                "BedrockDataProtectionOptions.ApplicationName is required. " +
                "Call .WithApplicationName() to set the application name for key isolation.");
        }

        var builder = services.AddDataProtection()
            .SetApplicationName(options.ApplicationName)
            .SetDefaultKeyLifetime(options.KeyLifetime);

        if (!string.IsNullOrWhiteSpace(options.KeyStoragePath))
        {
            builder.PersistKeysToFileSystem(new DirectoryInfo(options.KeyStoragePath));
        }

        options.ConfigureDataProtection?.Invoke(builder);

        return services;
    }
}
