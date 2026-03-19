using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Bedrock.BuildingBlocks.Web.WebApi.Controllers.Models;

// Callbacks opcionais para estender as configuracoes padrao do Bedrock
// em AddBedrockControllers. Cada callback e invocado APOS os defaults,
// permitindo que o cliente sobreponha ou complemente qualquer configuracao
// sem perder os defaults do framework.
public sealed class BedrockControllersOptions
{
    public Action<RouteOptions>? ConfigureRouting { get; set; }
    public Action<ApiVersioningOptions>? ConfigureApiVersioning { get; set; }
    public Action<ApiExplorerOptions>? ConfigureApiExplorer { get; set; }
    public Action<MvcOptions>? ConfigureMvc { get; set; }
    public Action<JsonSerializerOptions>? ConfigureJson { get; set; }
}
