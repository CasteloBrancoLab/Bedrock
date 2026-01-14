using Bedrock.BuildingBlocks.Core.ExecutionContexts.Models.Enums;
using Bedrock.BuildingBlocks.Core.Ids;

namespace Bedrock.BuildingBlocks.Core.ExecutionContexts.Models;

public readonly record struct Message
{
    // Properties
    public Id Id { get; }
    public DateTimeOffset TimeStamp { get; }
    public MessageType MessageType { get; }
    public string Code { get; }
    public string? Text { get; }

    // Constructors
    private Message(
        Id id,
        DateTimeOffset timeStamp,
        MessageType messageType,
        string code,
        string? text
    )
    {
        Id = id;
        TimeStamp = timeStamp;
        MessageType = messageType;
        Code = code;
        Text = text;
    }

    // Public Methods
    public static Message Create(
        TimeProvider timeProvider,
        MessageType messageType,
        string code,
        string? text = null
    )
    {
        ArgumentNullException.ThrowIfNull(timeProvider, nameof(timeProvider));
        ArgumentException.ThrowIfNullOrWhiteSpace(code, nameof(code));
        ThrowIfInvalidMessageType(messageType);

        return new Message(
            id: Id.GenerateNewId(),
            timeStamp: timeProvider.GetUtcNow(),
            messageType: messageType,
            code: code,
            text: text
        );
    }
    public static Message CreateTrace(
        TimeProvider timeProvider,
        string code,
        string? text = null
    )
    {
        return Create(
            timeProvider,
            messageType: MessageType.Trace,
            code: code,
            text: text
        );
    }
    public static Message CreateDebug(
        TimeProvider timeProvider,
        string code,
        string? text = null
    )
    {
        return Create(
            timeProvider,
            messageType: MessageType.Debug,
            code: code,
            text: text
        );
    }
    public static Message CreateInformation(
        TimeProvider timeProvider,
        string code,
        string? text = null
    )
    {
        return Create(
            timeProvider,
            messageType: MessageType.Information,
            code: code,
            text: text
        );
    }
    public static Message CreateWarning(
        TimeProvider timeProvider,
        string code,
        string? text = null
    )
    {
        return Create(
            timeProvider,
            messageType: MessageType.Warning,
            code: code,
            text: text
        );
    }
    public static Message CreateError(
        TimeProvider timeProvider,
        string code,
        string? text = null
    )
    {
        return Create(
            timeProvider,
            messageType: MessageType.Error,
            code: code,
            text: text
        );
    }
    public static Message CreateCritical(
        TimeProvider timeProvider,
        string code,
        string? text = null
    )
    {
        return Create(
            timeProvider,
            messageType: MessageType.Critical,
            code: code,
            text: text
        );
    }

    public static Message CreateNone(
        TimeProvider timeProvider,
        string code,
        string? text = null
    )
    {
        return Create(
            timeProvider,
            messageType: MessageType.None,
            code: code,
            text: text
        );
    }

    public static Message CreateSuccess(
        TimeProvider timeProvider,
        string code,
        string? text = null
    )
    {
        return Create(
            timeProvider,
            messageType: MessageType.Success,
            code: code,
            text: text
        );
    }

    public Message WithMessageType(MessageType newMessageType)
    {
        ThrowIfInvalidMessageType(newMessageType);

        return new Message(
            id: Id,
            timeStamp: TimeStamp,
            messageType: newMessageType,
            code: Code,
            text: Text
        );
    }

    public Message WithText(string? newText)
    {
        return new Message(
            id: Id,
            timeStamp: TimeStamp,
            messageType: MessageType,
            code: Code,
            text: newText
        );
    }

    public static bool ValidateMessageType(MessageType messageType)
    {
        return messageType is >= MessageType.Trace and <= MessageType.Success;
    }

    public static void ThrowIfInvalidMessageType(MessageType messageType)
    {
        if (ValidateMessageType(messageType))
            return;

        throw new ArgumentOutOfRangeException(
            nameof(messageType),
            "MessageType must be a valid MessageType value."
        );
    }
}
