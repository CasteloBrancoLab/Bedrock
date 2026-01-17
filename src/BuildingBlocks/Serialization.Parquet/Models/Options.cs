using System.Diagnostics.CodeAnalysis;
using Apache.Arrow.Ipc;

namespace Bedrock.BuildingBlocks.Serialization.Parquet.Models;

public class Options
{
    // Stryker disable all : Options initialization is infrastructure code - default values are configuration choices
    [ExcludeFromCodeCoverage(Justification = "Inicializacao de opcoes padrao - valores sao escolhas de configuracao")]
    public IpcOptions IpcWriteOptions
    {
        get
        {
            return field ?? new IpcOptions();
        }
        private set;
    }
    // Stryker restore all

    // Stryker disable all : Fluent API return pattern - return value tested through builder pattern usage
    [ExcludeFromCodeCoverage(Justification = "Padrao fluent API - valor de retorno testado atraves de encadeamento")]
    public Options WithIpcWriteOptions(IpcOptions options)
    {
        IpcWriteOptions = options;
        return this;
    }
    // Stryker restore all
}
