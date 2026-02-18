using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using ShopDemo.Auth.Domain.Entities.ConsentTerms;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class ConsentTermDataModelFactory
{
    public static ConsentTermDataModel Create(ConsentTerm entity)
    {
        ConsentTermDataModel dataModel =
            DataModelBaseFactory.Create<ConsentTermDataModel, ConsentTerm>(entity);

        dataModel.Type = (short)entity.Type;
        dataModel.Version = entity.Version;
        dataModel.Content = entity.Content;
        dataModel.PublishedAt = entity.PublishedAt;

        return dataModel;
    }
}
