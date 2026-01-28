using System;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;
using Templates.Domain.Entities.SimpleAggregateRoots;

namespace Templates.Infra.Data.PostgreSql.Repositories.Interfaces;

public interface ISimpleAggregateRootPostgreSqlRepository
    : IPostgreSqlRepository<SimpleAggregateRoot>
{

}
