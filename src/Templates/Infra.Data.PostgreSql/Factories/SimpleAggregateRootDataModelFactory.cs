using Bedrock.BuildingBlocks.Persistence.PostgreSql.Factories;
using Templates.Domain.Entities.SimpleAggregateRoots;
using Templates.Infra.Data.PostgreSql.DataModels;

namespace Templates.Infra.Data.PostgreSql.Factories;

public static class SimpleAggregateRootDataModelFactory
{
    public static SimpleAggregateRootDataModel Create(SimpleAggregateRoot entity)
    {
        SimpleAggregateRootDataModel dataModel =
            DataModelBaseFactory.Create<SimpleAggregateRootDataModel, SimpleAggregateRoot>(entity);

        dataModel.FirstName = entity.FirstName;
        dataModel.LastName = entity.LastName;
        dataModel.FullName = entity.FullName;
        dataModel.BirthDate = entity.BirthDate;

        return dataModel;
    }

    public static SimpleAggregateRootDataModel Create(
        ExecutionContext executionContext,
        string firstName,
        string lastName,
        string fullName,
        DateTimeOffset birthDate
    )
    {
        SimpleAggregateRootDataModel dataModel =
            DataModelBaseFactory.Create<SimpleAggregateRootDataModel>(executionContext);

        dataModel.FirstName = firstName;
        dataModel.LastName = lastName;
        dataModel.FullName = fullName;
        dataModel.BirthDate = birthDate;

        return dataModel;
    }
}
