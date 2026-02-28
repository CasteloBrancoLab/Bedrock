using Bedrock.BuildingBlocks.Application.UseCases.Interfaces;
using Templates.Application.UseCases.GetSimpleAggregateRootById.Models;

namespace Templates.Application.UseCases.GetSimpleAggregateRootById.Interfaces;

public interface IGetSimpleAggregateRootByIdUseCase
    : IUseCase<GetSimpleAggregateRootByIdInput, GetSimpleAggregateRootByIdOutput>;
