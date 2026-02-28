using Bedrock.BuildingBlocks.Application.UseCases.Interfaces;
using Templates.Application.UseCases.RegisterSimpleAggregateRoot.Models;

namespace Templates.Application.UseCases.RegisterSimpleAggregateRoot.Interfaces;

public interface IRegisterSimpleAggregateRootUseCase
    : IUseCase<RegisterSimpleAggregateRootInput, RegisterSimpleAggregateRootOutput>;
