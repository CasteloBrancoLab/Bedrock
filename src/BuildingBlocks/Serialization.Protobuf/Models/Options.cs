namespace Bedrock.BuildingBlocks.Serialization.Protobuf.Models;

public class Options
{
    public IEnumerable<Type>? TypeCollection
    {
        get
        {
            return field ?? [];
        }

        private set;
    }

    public Options WithSupportedTypes(IEnumerable<Type>? typeCollection)
    {
        TypeCollection = typeCollection;

        return this;
    }
    public Options WithSupportedTypes(params Type[] typeCollection)
    {
        TypeCollection = typeCollection;

        return this;
    }
}
