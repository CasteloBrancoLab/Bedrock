using Bedrock.BuildingBlocks.Persistence.PostgreSql.Adapters;
using ShopDemo.Auth.Domain.Entities.Claims;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Adapters;

public static class ClaimDataModelAdapter
{
    public static ClaimDataModel Adapt(
        ClaimDataModel dataModel,
        Claim entity
    )
    {
        DataModelBaseAdapter.Adapt(dataModel, entity);

        dataModel.Name = entity.Name;
        dataModel.Description = entity.Description;

        return dataModel;
    }
}
