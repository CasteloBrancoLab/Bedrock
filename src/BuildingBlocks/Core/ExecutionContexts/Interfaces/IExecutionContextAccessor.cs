using Bedrock.BuildingBlocks.Core.ExecutionContexts;

namespace Bedrock.BuildingBlocks.Core.ExecutionContexts.Interfaces;

public interface IExecutionContextAccessor
{
    ExecutionContext? Current { get; }
    void SetCurrent(ExecutionContext context);
}
