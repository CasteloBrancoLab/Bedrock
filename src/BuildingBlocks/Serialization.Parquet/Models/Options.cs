using Apache.Arrow.Ipc;

namespace Bedrock.BuildingBlocks.Serialization.Parquet.Models;

public class Options
{
    public IpcOptions IpcWriteOptions
    {
        get
        {
            return field ?? new IpcOptions();
        }
        private set;
    }

    public Options WithIpcWriteOptions(IpcOptions options)
    {
        IpcWriteOptions = options;
        return this;
    }
}
