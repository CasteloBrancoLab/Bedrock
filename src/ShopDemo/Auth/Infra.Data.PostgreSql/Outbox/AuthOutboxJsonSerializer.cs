using System.Diagnostics.CodeAnalysis;
using Bedrock.BuildingBlocks.Serialization.Json;
using Bedrock.BuildingBlocks.Serialization.Json.Models;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Outbox;

/// <summary>
/// JSON serializer concreto para o Auth bounded context.
/// Usado pelo outbox para serializar/deserializar messages como UTF-8 bytes.
/// </summary>
// Stryker disable all : Configuracao de serializacao — opcoes sao escolhas de infraestrutura
[ExcludeFromCodeCoverage(Justification = "Configuracao de serializacao — opcoes sao escolhas de infraestrutura")]
public sealed class AuthOutboxJsonSerializer : JsonSerializerBase
{
    protected override void ConfigureInternal(Options options)
    {
        // Defaults: PropertyNameCaseInsensitive = false, WhenWritingDefault
        // Suficiente para records posicionais com tipos primitivos
    }
}
// Stryker restore all
