using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using Templates.Domain.Entities.SimpleAggregateRoots;
using Templates.Infra.Data.PostgreSql.DataModels;

namespace Templates.Infra.Data.PostgreSql.Adapters;

public static class SimpleAggregateRootDataModelAdapter
{
    public static SimpleAggregateRootDataModel Adapt(
        SimpleAggregateRootDataModel dataModel,
        SimpleAggregateRoot entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.FirstName = entity.FirstName;
        dataModel.LastName = entity.LastName;
        dataModel.FullName = entity.FullName;
        dataModel.BirthDate = entity.BirthDate;

        return dataModel;
    }
}
