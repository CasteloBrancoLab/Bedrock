namespace Bedrock.BuildingBlocks.Web.WebApi.Envelope.Translation;

// Implementacao default no-op: retorna o code como description.
// Consumers registram sua propria implementacao via AddBedrockEnvelope<TTranslator>().
public sealed class NullMessageTranslator : IMessageTranslator
{
    public string? Translate(string code, string language)
    {
        return code;
    }
}
