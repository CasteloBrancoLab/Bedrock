namespace Bedrock.BuildingBlocks.Web.WebApi.Envelope.Translation;

public interface IMessageTranslator
{
    string? Translate(string code, string language);
}
