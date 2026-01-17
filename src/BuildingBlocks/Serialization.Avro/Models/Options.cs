namespace Bedrock.BuildingBlocks.Serialization.Avro.Models;

public class Options
{
    public bool IncludeSchemaInOutput
    {
        get
        {
            return field;
        }
        private set;
    }

    public Options WithIncludeSchemaInOutput(bool include)
    {
        IncludeSchemaInOutput = include;
        return this;
    }
}
