namespace Bedrock.BuildingBlocks.Core.ExecutionContexts;

public class ExecutionContextAccessor : IExecutionContextAccessor
{
    public ExecutionContext? Current { get; private set; }

    public void SetCurrent(ExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Current = context;
    }
}
