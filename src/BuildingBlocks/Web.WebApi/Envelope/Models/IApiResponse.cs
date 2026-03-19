namespace Bedrock.BuildingBlocks.Web.WebApi.Envelope.Models;

// Interface marker nao-generica para que o action filter de traducao
// possa trabalhar sem reflection sobre o tipo generico.
public interface IApiResponse
{
    IReadOnlyList<ResponseMessage> Messages { get; }
    IApiResponse WithTranslatedMessages(IReadOnlyList<ResponseMessage> messages);
}
