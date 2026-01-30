using System.Text.Json;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Observability.ExtensionMethods;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var executionContext = Bedrock.BuildingBlocks.Core.ExecutionContexts.ExecutionContext.Create(
    correlationId: Id.GenerateNewId(),
    tenantInfo: TenantInfo.Create(code: Id.GenerateNewId(), name: "PlaygroundTenant"),
    executionUser: "PlaygroundUser",
    executionOrigin: "PlaygroundApp",
    businessOperationCode: "PlaygroundRun",
    minimumMessageType: Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums.MessageType.Information,
    timeProvider: TimeProvider.System
);

var services = new ServiceCollection();
services.AddLogging(builder => builder
    .AddJsonConsole(options =>
    {
        options.IncludeScopes = true;
        options.UseUtcTimestamp = true;
        options.JsonWriterOptions = new JsonWriterOptions
        {
            Indented = true
        };
    })
);

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();

logger.LogInformationForDistributedTracing(
    executionContext, 
    "Playground iniciado 2. Nome: {Nome}",
    args: [
        "Marcelo"
    ]
);

logger.LogInformation(
    "Playground iniciado. Nome: {Nome}",
    args: [
        "Marcelo"
    ]
);

logger.LogInformation("Press [Enter] to exit...");
Console.ReadLine();
