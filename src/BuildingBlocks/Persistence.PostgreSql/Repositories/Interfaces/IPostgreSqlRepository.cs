using Bedrock.BuildingBlocks.Domain.Entities.Interfaces;
using Bedrock.BuildingBlocks.Domain.Repositories;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Repositories.Interfaces;

public interface IPostgreSqlRepository<TAggregateRoot>
    : IRepository<TAggregateRoot>
    where TAggregateRoot : IAggregateRoot
{
}
