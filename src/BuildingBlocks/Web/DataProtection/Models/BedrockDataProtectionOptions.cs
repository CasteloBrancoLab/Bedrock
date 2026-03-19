using Microsoft.AspNetCore.DataProtection;

namespace Bedrock.BuildingBlocks.Web.DataProtection.Models;

// Configuracao fluente para Data Protection do Bedrock.
//
// O Data Protection API do ASP.NET Core protege dados sensiveis
// (cookies, tokens anti-forgery, etc.) com criptografia automatica.
//
// ApplicationName e obrigatorio para isolamento de keys entre servicos.
// Sem ele, dois servicos no mesmo host compartilhariam keys e poderiam
// decriptar dados um do outro.
//
// Uso tipico:
//   new BedrockDataProtectionOptions()
//       .WithApplicationName("ShopDemo.Auth")
//       .WithKeyStoragePath("/var/keys/auth")
//       .WithKeyLifetime(TimeSpan.FromDays(90))
//       .Configure(builder => builder.ProtectKeysWithCertificate(...))
public sealed class BedrockDataProtectionOptions
{
    internal string? ApplicationName { get; private set; }
    internal string? KeyStoragePath { get; private set; }
    internal TimeSpan KeyLifetime { get; private set; } = TimeSpan.FromDays(90);
    internal Action<IDataProtectionBuilder>? ConfigureDataProtection { get; private set; }

    // Define o nome da aplicacao para isolamento de keys.
    // Obrigatorio — servicos diferentes devem ter nomes diferentes.
    public BedrockDataProtectionOptions WithApplicationName(string applicationName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(applicationName);
        ApplicationName = applicationName;
        return this;
    }

    // Define o caminho no filesystem para armazenamento de keys.
    // Util para shared volumes em containers (todos os replicas acessam as mesmas keys).
    public BedrockDataProtectionOptions WithKeyStoragePath(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        KeyStoragePath = path;
        return this;
    }

    // Define o tempo de vida das keys de criptografia.
    // Apos expirar, novas keys sao geradas automaticamente.
    // Default: 90 dias.
    public BedrockDataProtectionOptions WithKeyLifetime(TimeSpan lifetime)
    {
        KeyLifetime = lifetime;
        return this;
    }

    // Callback para estender a configuracao do IDataProtectionBuilder
    // apos os defaults do Bedrock (ex: Azure Blob, Redis, certificado).
    public BedrockDataProtectionOptions Configure(Action<IDataProtectionBuilder> configure)
    {
        ConfigureDataProtection = configure;
        return this;
    }
}
