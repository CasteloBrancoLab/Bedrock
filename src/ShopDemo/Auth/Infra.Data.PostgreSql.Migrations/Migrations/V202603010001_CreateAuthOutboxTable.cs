using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Migrations.Attributes;
using FluentMigrator;

namespace ShopDemo.Auth.Infra.Data.PostgreSql.Migrations.Migrations;

[Migration(202603010001)]
[SqlScript(
    "Up/V202603010001__create_auth_outbox_table.sql",
    "Down/V202603010001__create_auth_outbox_table.sql")]
public sealed class V202603010001_CreateAuthOutboxTable : SqlScriptMigrationBase;
