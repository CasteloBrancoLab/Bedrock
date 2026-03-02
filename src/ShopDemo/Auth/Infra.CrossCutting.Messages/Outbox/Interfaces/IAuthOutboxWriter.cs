using Bedrock.BuildingBlocks.Messages;
using Bedrock.BuildingBlocks.Outbox.Interfaces;

namespace ShopDemo.Auth.Infra.CrossCutting.Messages.Outbox.Interfaces;

public interface IAuthOutboxWriter
    : IOutboxWriter<MessageBase>
{
}
