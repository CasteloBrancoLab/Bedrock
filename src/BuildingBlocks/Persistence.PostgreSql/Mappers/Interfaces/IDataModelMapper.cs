using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.Sortings.Enums;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Npgsql;
using NpgsqlTypes;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;

public interface IDataModelMapper<TDataModel>
    where TDataModel : DataModelBase
{
    // Properties
    public string? TableSchema { get; }
    public string? TableName { get; }
    public ReadOnlyDictionary<string, ColumnMap> ColumnMapDictionary { get; }

    // Command generation (cached - zero allocation on repeated calls)
    public string SelectCommand { get; }
    public string InsertCommand { get; }
    public string UpdateCommand { get; }
    public string DeleteCommand { get; }
    public string CopyCommand { get; }

    // Command generation with where clause (type-safe)
    public string GenerateSelectCommand(WhereClause whereClause);
    public string GenerateSelectCommand(WhereClause whereClause, OrderByClause orderBy);
    public string GenerateSelectCommand(WhereClause whereClause, OrderByClause orderBy, PaginationInfo paginationInfo);
    public string GenerateSelectCommand(WhereClause whereClause, PaginationInfo paginationInfo);
    public string GenerateSelectCommand(OrderByClause orderBy);
    public string GenerateSelectCommand(OrderByClause orderBy, PaginationInfo paginationInfo);
    public string GenerateSelectCommand(PaginationInfo paginationInfo);
    public string GenerateUpdateCommand(WhereClause whereClause);
    public string GenerateDeleteCommand(WhereClause whereClause);
    public string GenerateExistsCommand(WhereClause whereClause);

    // Table/Column info
    public string GetTableName();
    public string GetColumnNamesWithAlias();
    public ColumnMap GetColumnMap(string propertyName);
    public ColumnMap GetColumnMap<TProperty>(Expression<Func<TDataModel, TProperty>> selector);
    public string GetColumnName<TProperty>(Expression<Func<TDataModel, TProperty>> selector);
    public Type GetColumnType<TProperty>(Expression<Func<TDataModel, TProperty>> selector);
    public NpgsqlDbType GetColumnNpgsqlDbType<TProperty>(Expression<Func<TDataModel, TProperty>> selector);

    // Where clause generation (type-safe)
    public WhereClause Where(string propertyName, RelationalOperator op);
    public WhereClause Where<TProperty>(Expression<Func<TDataModel, TProperty>> selector, RelationalOperator op);
    public WhereClause Where(string propertyName);
    public WhereClause Where<TProperty>(Expression<Func<TDataModel, TProperty>> selector);

    // Order by clause generation (type-safe)
    public OrderByClause OrderBy(string propertyName, SortDirection direction);
    public OrderByClause OrderBy<TProperty>(Expression<Func<TDataModel, TProperty>> selector, SortDirection direction);
    public OrderByClause OrderByAscending(string propertyName);
    public OrderByClause OrderByAscending<TProperty>(Expression<Func<TDataModel, TProperty>> selector);
    public OrderByClause OrderByDescending(string propertyName);
    public OrderByClause OrderByDescending<TProperty>(Expression<Func<TDataModel, TProperty>> selector);

    // Parameter handling
    public string GetParameterName(string propertyName);
    public string GetParameterName<TProperty>(Expression<Func<TDataModel, TProperty>> selector);
    public void AddParameterForCommand(NpgsqlCommand npgsqlCommand, string propertyName, NpgsqlDbType npgsqlDbType, object? value);
    public void AddParameterForCommand<TProperty>(NpgsqlCommand npgsqlCommand, Expression<Func<TDataModel, TProperty>> selector, object? value);

    // Data model handling
    public void ConfigureCommandToProperty(string propertyName, TDataModel dataModel, NpgsqlCommand npgsqlCommand);
    public NpgsqlCommand ConfigureCommandFromDataModelBase(NpgsqlCommand command, IDataModelMapper<TDataModel> mapper, TDataModel dataModel);
    public void PopulateDataModelBaseFromReader(NpgsqlDataReader reader, TDataModel dataModel, Action<NpgsqlDataReader, TDataModel, IDataModelMapper<TDataModel>>? additionalMap);
    public void MapBinaryImporter(NpgsqlBinaryImporter importer, TDataModel model);
}
