using Bedrock.ArchitectureTests.BuildingBlocks.Messages.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.BuildingBlocks.Messages;

[Collection("Arch")]
public sealed class MessagesRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : MessagesRuleTestsBase<ArchFixture>(fixture, output);
