namespace Bedrock.BuildingBlocks.Web.WebApi.ApiDocumentation;

// Remove todos os headers do Bedrock da spec OpenAPI para a action ou controller anotada.
// Util para controllers inteiras que nao usam ExecutionContext (ex: health, public endpoints).
//
// Uso:
//   [ExcludeAllBedrockHeaders] na action ou na controller.
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class ExcludeAllBedrockHeadersAttribute : Attribute;
