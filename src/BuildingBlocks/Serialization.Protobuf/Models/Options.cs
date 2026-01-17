using System.Diagnostics.CodeAnalysis;

namespace Bedrock.BuildingBlocks.Serialization.Protobuf.Models;

public class Options
{
    // Stryker disable all : Options initialization is infrastructure code - default values are configuration choices
    [ExcludeFromCodeCoverage(Justification = "Inicializacao de opcoes padrao - valores sao escolhas de configuracao")]
    public IEnumerable<Type>? TypeCollection
    {
        get
        {
            return field ?? [];
        }

        private set;
    }
    // Stryker restore all

    // Stryker disable all : Fluent API return pattern - return value tested through builder pattern usage
    [ExcludeFromCodeCoverage(Justification = "Padrao fluent API - valor de retorno testado atraves de encadeamento")]
    public Options WithSupportedTypes(IEnumerable<Type>? typeCollection)
    {
        TypeCollection = typeCollection;

        return this;
    }

    [ExcludeFromCodeCoverage(Justification = "Padrao fluent API - valor de retorno testado atraves de encadeamento")]
    public Options WithSupportedTypes(params Type[] typeCollection)
    {
        TypeCollection = typeCollection;

        return this;
    }
    // Stryker restore all
}
