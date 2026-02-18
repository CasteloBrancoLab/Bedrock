using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.ConsentTerms;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class ConsentTermDataModelAdapter
{
    public static ConsentTermDataModel Adapt(
        ConsentTermDataModel dataModel,
        ConsentTerm entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.Type = (short)entity.Type;
        dataModel.Version = entity.Version;
        dataModel.Content = entity.Content;
        dataModel.PublishedAt = entity.PublishedAt;

        return dataModel;
    }
}
