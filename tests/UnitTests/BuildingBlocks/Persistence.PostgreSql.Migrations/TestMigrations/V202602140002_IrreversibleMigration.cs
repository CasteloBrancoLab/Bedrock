using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Attributes;
using FluentMigrator;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Migrations.TestMigrations;

[Migration(202602140002)]
[SqlScript("Up.V202602140001__create_test_table.sql")]
public sealed class V202602140002_IrreversibleMigration : SqlScriptMigrationBase;
