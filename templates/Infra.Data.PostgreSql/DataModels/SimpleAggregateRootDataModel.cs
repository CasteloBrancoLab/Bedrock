using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace Templates.Infra.Data.PostgreSql.DataModels;

public class SimpleAggregateRootDataModel : DataModelBase
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public DateTimeOffset BirthDate { get; set; }
}
