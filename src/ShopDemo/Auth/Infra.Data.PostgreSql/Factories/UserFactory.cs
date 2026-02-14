using Bedrock.BuildingBlocks.Core.EmailAddresses;
using Bedrock.BuildingBlocks.Core.Ids;
using Bedrock.BuildingBlocks.Core.RegistryVersions;
using Bedrock.BuildingBlocks.Core.TenantInfos;
using Bedrock.BuildingBlocks.Domain.Entities.Models;
using ShopDemo.Auth.Domain.Entities.Users;
using ShopDemo.Auth.Domain.Entities.Users.Inputs;
using ShopDemo.Auth.Infra.Data.PostgreSql.DataModels;
using ShopDemo.Core.Entities.Users.Enums;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Factories;

public static class UserFactory
{
    public static User Create(UserDataModel dataModel)
    {
        EntityInfo entityInfo = EntityInfo.CreateFromExistingInfo(
            id: Id.CreateFromExistingInfo(dataModel.Id),
            tenantInfo: TenantInfo.Create(dataModel.TenantCode),
            createdAt: dataModel.CreatedAt,
            createdBy: dataModel.CreatedBy,
            createdCorrelationId: dataModel.CreatedCorrelationId,
            createdExecutionOrigin: dataModel.CreatedExecutionOrigin,
            createdBusinessOperationCode: dataModel.CreatedBusinessOperationCode,
            lastChangedAt: dataModel.LastChangedAt,
            lastChangedBy: dataModel.LastChangedBy,
            lastChangedCorrelationId: dataModel.LastChangedCorrelationId,
            lastChangedExecutionOrigin: dataModel.LastChangedExecutionOrigin,
            lastChangedBusinessOperationCode: dataModel.LastChangedBusinessOperationCode,
            entityVersion: RegistryVersion.CreateFromExistingInfo(dataModel.EntityVersion));

        return User.CreateFromExistingInfo(
            new CreateFromExistingInfoInput(
                entityInfo,
                dataModel.Username,
                EmailAddress.CreateNew(dataModel.Email),
                PasswordHash.CreateNew(dataModel.PasswordHash),
                (UserStatus)dataModel.Status));
    }
}
