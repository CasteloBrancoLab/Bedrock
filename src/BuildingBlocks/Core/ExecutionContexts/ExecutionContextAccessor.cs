using Bedrock.BuildingBlocks.Core.ExecutionContexts.Interfaces;

namespace Bedrock.BuildingBlocks.Core.ExecutionContexts;

public sealed class ExecutionContextAccessor : IExecutionContextAccessor
{
    private volatile ExecutionContext? _current;

    public ExecutionContext? Current => _current;

    public void SetCurrent(ExecutionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _current = context;
    }
}
