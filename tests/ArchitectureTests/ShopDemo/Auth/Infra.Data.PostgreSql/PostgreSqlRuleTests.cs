using Bedrock.ArchitectureTests.ShopDemo.Auth.Infra.Data.PostgreSql.Fixtures;
using Bedrock.BuildingBlocks.Testing.Architecture.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Bedrock.ArchitectureTests.ShopDemo.Auth.Infra.Data.PostgreSql;

[Collection("Arch")]
public sealed class PostgreSqlRuleTests(ArchFixture fixture, ITestOutputHelper output)
    : PostgreSqlRuleTestsBase<ArchFixture>(fixture, output);
