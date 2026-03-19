using System.Globalization;
using Bedrock.BuildingBlocks.Web.Hosting.Models;

namespace Bedrock.BuildingBlocks.Web.Hosting;

// Centraliza configuracoes de runtime que devem ser aplicadas antes de qualquer
// inicializacao do host (WebApplicationBuilder, DI, middleware). Deve ser chamado
// na primeira linha do Main para garantir comportamento determinístico.
public static class BedrockHost
{
    public static void ConfigureDefaults(out StartupInfo startupInfo)
    {
        ConfigureInvariantCulture();
        ConfigureThreadPool();

        startupInfo = StartupInfo.Create();
    }

    // Forca InvariantCulture em todas as threads para garantir comportamento
    // consistente de formatacao (datas, numeros, comparacao de strings)
    // independente da cultura do sistema operacional do host.
    // Sem isso, um servidor configurado em pt-BR formataria "1.234,56"
    // enquanto um em en-US formataria "1,234.56", causando bugs sutis
    // em parsing, serialização e logs.
    private static void ConfigureInvariantCulture()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
    }

    // Define o minimo de threads do ThreadPool baseado no numero de processadores
    // para eliminar latencia de cold-start. O .NET inicia com um minimo baixo e
    // escala sob demanda (~1-2 threads/segundo), causando thread starvation nas
    // primeiras requisicoes concorrentes. Pre-aquecer com ProcessorCount garante
    // que o pool esteja pronto para atender carga imediata sem delays de ramp-up.
    private static void ConfigureThreadPool()
    {
        var processorCount = Environment.ProcessorCount;
        ThreadPool.SetMinThreads(processorCount, processorCount);
    }
}
