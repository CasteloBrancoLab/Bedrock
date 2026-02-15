using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Attributes;
using FluentMigrator;

namespace Bedrock.UnitTests.BuildingBlocks.Persistence.PostgreSql.Migrations.TestMigrations;

[Migration(202602140004)]
[SqlScript("Up.NonExistent__script.sql", "Down.NonExistent__script.sql")]
public sealed class MigrationWithMissingScript : SqlScriptMigrationBase;
