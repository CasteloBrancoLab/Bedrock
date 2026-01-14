namespace Bedrock.BuildingBlocks.Core.ExecutionContexts;

public interface IExecutionContextAccessor
{
    ExecutionContext? Current { get; }
    void SetCurrent(ExecutionContext context);
}
