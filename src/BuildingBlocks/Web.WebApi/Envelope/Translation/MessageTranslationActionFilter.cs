using Bedrock.BuildingBlocks.Web.WebApi.Envelope.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bedrock.BuildingBlocks.Web.WebApi.Envelope.Translation;

// Action filter que intercepta responses do tipo ApiResponse<T> e
// traduz os message codes via IMessageTranslator registrado no DI.
public sealed class MessageTranslationActionFilter : IAsyncResultFilter
{
    private readonly IMessageTranslator _translator;

    public MessageTranslationActionFilter(IMessageTranslator translator)
    {
        _translator = translator;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult &&
            objectResult.Value is IApiResponse apiResponse &&
            apiResponse.Messages.Count > 0)
        {
            var translatedMessages = TranslateMessages(apiResponse);
            objectResult.Value = apiResponse.WithTranslatedMessages(translatedMessages);
        }

        await next();
    }

    private List<ResponseMessage> TranslateMessages(IApiResponse apiResponse)
    {
        var translated = new List<ResponseMessage>(apiResponse.Messages.Count);

        foreach (var message in apiResponse.Messages)
        {
            var description = _translator.Translate(message.Code, ExtractLanguage(apiResponse)) ?? message.Description;
            translated.Add(message with { Description = description });
        }

        return translated;
    }

    private static string ExtractLanguage(IApiResponse apiResponse)
    {
        // IApiResponse nao expoe Language diretamente para manter a interface simples.
        // Usamos pattern matching no tipo concreto.
        if (apiResponse is ApiResponse<object> typed)
            return typed.Language;

        // Fallback via reflection para tipos genericos desconhecidos.
        var languageProp = apiResponse.GetType().GetProperty("Language");
        return languageProp?.GetValue(apiResponse) as string ?? "en";
    }
}
