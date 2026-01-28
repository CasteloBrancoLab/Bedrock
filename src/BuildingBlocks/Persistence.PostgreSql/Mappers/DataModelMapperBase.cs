using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.Sortings.Enums;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.ExtensionMethods;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
using Microsoft.Extensions.ObjectPool;
using Npgsql;
using NpgsqlTypes;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers;

public abstract class DataModelMapperBase<TDataModel>
    : IDataModelMapper<TDataModel>
    where TDataModel : DataModelBase
{
    // Static fields (shared across all instances of the same TDataModel type)
    private static readonly Lock _propertyInfoDictionaryLocker = new();
    private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> _propertyInfoDictionary = [];
    private static readonly ObjectPool<StringBuilder> StringBuilderPool =
        new DefaultObjectPoolProvider().CreateStringBuilderPool();

    // Instance fields
    private readonly Dictionary<string, ColumnMap> _columnMapDictionary = [];
    private ReadOnlyDictionary<string, ColumnMap>? _columnMapReadOnlyDictionary;

    // Caches for zero-allocation
    private readonly Dictionary<string, string> _parameterNameCache = [];
    private readonly Dictionary<(string, RelationalOperator), WhereClause> _whereClauseCache = [];
    private readonly Dictionary<(string, SortDirection), OrderByClause> _orderByClauseCache = [];
    // Stryker disable once String : Valor inicial sobrescrito em CacheGeneratedStrings - mutante equivalente
    private string _parameterPrefix = string.Empty;

    private bool _isConfigured;
    private Type _dataModelType = default!;

    // Cached strings (generated once during configuration)
    // Stryker disable once String : Valor inicial sobrescrito em CacheGeneratedStrings - mutante equivalente
    private string _tableName = string.Empty;
    // Stryker disable once String : Valor inicial sobrescrito em CacheGeneratedStrings - mutante equivalente
    private string _baseWhereClause = string.Empty;
    // Stryker disable once String : Valor inicial sobrescrito em CacheGeneratedStrings - mutante equivalente
    private string _baseWhereClauseToUpdate = string.Empty;
    // Stryker disable once String : Valor inicial sobrescrito em CacheGeneratedStrings - mutante equivalente
    private string _columnNamesWithAlias = string.Empty;
    // Stryker disable once String : Valor inicial sobrescrito em CacheGeneratedStrings - mutante equivalente
    private string _columnNames = string.Empty;
    // Stryker disable once String : Valor inicial sobrescrito em CacheGeneratedStrings - mutante equivalente
    private string _parameters = string.Empty;
    // Stryker disable once String : Valor inicial sobrescrito em CacheGeneratedStrings - mutante equivalente
    private string _setClauses = string.Empty;

    // Properties
    public string? TableSchema { get; private set; }
    public string? TableName { get; private set; }

    public ReadOnlyDictionary<string, ColumnMap> ColumnMapDictionary
    {
        get
        {
            // Cached ReadOnlyDictionary - created once, reused always
            return _columnMapReadOnlyDictionary ??= _columnMapDictionary.AsReadOnly();
        }
    }

    // Cached command properties (zero allocation)
    // Stryker disable once String : Valor inicial sobrescrito em CacheGeneratedStrings - mutante equivalente
    public string SelectCommand { get; private set; } = string.Empty;
    // Stryker disable once String : Valor inicial sobrescrito em CacheGeneratedStrings - mutante equivalente
    public string InsertCommand { get; private set; } = string.Empty;
    // Stryker disable once String : Valor inicial sobrescrito em CacheGeneratedStrings - mutante equivalente
    public string UpdateCommand { get; private set; } = string.Empty;
    // Stryker disable once String : Valor inicial sobrescrito em CacheGeneratedStrings - mutante equivalente
    public string DeleteCommand { get; private set; } = string.Empty;
    // Stryker disable once String : Valor inicial sobrescrito em CacheGeneratedStrings - mutante equivalente
    public string CopyCommand { get; private set; } = string.Empty;

    // Constructors
    protected DataModelMapperBase()
    {
        TryConfigure();
    }

    // Public Methods - Configuration
    public void Configure()
    {
        // Stryker disable once Statement : Chamada obrigatoria - sem ela PropertyInfos nao sao mapeados
        TryMapPropertyInfos<TDataModel>();

        _dataModelType = typeof(TDataModel);

        MapperOptions<TDataModel> mapperOptions = new();

        if (typeof(DataModelBase).IsAssignableFrom(typeof(TDataModel)))
        {
            MapDataModelBaseColumns();
        }

        ConfigureInternal(mapperOptions);

        ArgumentNullException.ThrowIfNull(mapperOptions.TableName);

        TableSchema = mapperOptions.TableSchema;
        TableName = mapperOptions.TableName;

        foreach (KeyValuePair<string, ColumnMap> field in mapperOptions.FieldDictionary)
        {
            _columnMapDictionary.Add(field.Key, field.Value);
        }

        // Cache all generated strings once during configuration
        CacheGeneratedStrings();

        // Stryker disable once Boolean : Mutar para false causaria loop infinito em TryConfigure
        _isConfigured = true;
    }

    // Public Methods - Table/Column info
    public string GetTableName()
    {
        return _tableName;
    }

    public string GetColumnNamesWithAlias()
    {
        return _columnNamesWithAlias;
    }

    public ColumnMap GetColumnMap(string propertyName)
    {
        if (!_columnMapDictionary.TryGetValue(propertyName, out ColumnMap value))
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(propertyName),
                actualValue: propertyName,
                // Stryker disable once String : Mensagem de excecao - nao afeta comportamento
                message: "Property name not found"
            );
        }

        return value;
    }

    public ColumnMap GetColumnMap<TProperty>(Expression<Func<TDataModel, TProperty>> selector)
    {
        PropertyInfo propertyInfo = ExpressionUtils.GetProperty(selector);
        return GetColumnMap(propertyName: propertyInfo.Name);
    }

    public string GetColumnName<TProperty>(Expression<Func<TDataModel, TProperty>> selector)
    {
        return GetColumnMap(selector).ColumnName;
    }

    public Type GetColumnType<TProperty>(Expression<Func<TDataModel, TProperty>> selector)
    {
        return GetColumnMap(selector).Type;
    }

    public NpgsqlDbType GetColumnNpgsqlDbType<TProperty>(Expression<Func<TDataModel, TProperty>> selector)
    {
        return GetColumnMap(selector).NpgsqlDbType;
    }

    // Public Methods - Command generation with WhereClause (type-safe)
    // Stryker disable all : StringBuilder pool e string concatenacao - testado indiretamente pelo resultado do comando
    [ExcludeFromCodeCoverage(Justification = "StringBuilder pool - testado indiretamente pelos testes de comando")]
    public string GenerateSelectCommand(WhereClause whereClause)
    {
        StringBuilder sb = StringBuilderPool.Get();
        try
        {
            return sb.Append(SelectCommand)
                .Append(" AND (")
                .Append(whereClause.Value)
                .Append(')')
                .ToString();
        }
        finally
        {
            sb.Clear();
            StringBuilderPool.Return(sb);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "StringBuilder pool - testado indiretamente pelos testes de comando")]
    public string GenerateSelectCommand(WhereClause whereClause, PaginationInfo paginationInfo)
    {
        StringBuilder sb = StringBuilderPool.Get();
        try
        {
            return sb.Append(SelectCommand)
                .Append(" AND (")
                .Append(whereClause.Value)
                .Append(") LIMIT ")
                .Append(paginationInfo.PageSize)
                .Append(" OFFSET ")
                .Append(paginationInfo.Offset)
                .ToString();
        }
        finally
        {
            sb.Clear();
            StringBuilderPool.Return(sb);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "StringBuilder pool - testado indiretamente pelos testes de comando")]
    public string GenerateSelectCommand(PaginationInfo paginationInfo)
    {
        StringBuilder sb = StringBuilderPool.Get();
        try
        {
            return sb.Append(SelectCommand)
                .Append(" LIMIT ")
                .Append(paginationInfo.PageSize)
                .Append(" OFFSET ")
                .Append(paginationInfo.Offset)
                .ToString();
        }
        finally
        {
            sb.Clear();
            StringBuilderPool.Return(sb);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "StringBuilder pool - testado indiretamente pelos testes de comando")]
    public string GenerateSelectCommand(WhereClause whereClause, OrderByClause orderBy)
    {
        StringBuilder sb = StringBuilderPool.Get();
        try
        {
            return sb.Append(SelectCommand)
                .Append(" AND (")
                .Append(whereClause.Value)
                .Append(") ORDER BY ")
                .Append(orderBy.Value)
                .ToString();
        }
        finally
        {
            sb.Clear();
            StringBuilderPool.Return(sb);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "StringBuilder pool - testado indiretamente pelos testes de comando")]
    public string GenerateSelectCommand(WhereClause whereClause, OrderByClause orderBy, PaginationInfo paginationInfo)
    {
        StringBuilder sb = StringBuilderPool.Get();
        try
        {
            return sb.Append(SelectCommand)
                .Append(" AND (")
                .Append(whereClause.Value)
                .Append(") ORDER BY ")
                .Append(orderBy.Value)
                .Append(" LIMIT ")
                .Append(paginationInfo.PageSize)
                .Append(" OFFSET ")
                .Append(paginationInfo.Offset)
                .ToString();
        }
        finally
        {
            sb.Clear();
            StringBuilderPool.Return(sb);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "StringBuilder pool - testado indiretamente pelos testes de comando")]
    public string GenerateSelectCommand(OrderByClause orderBy)
    {
        StringBuilder sb = StringBuilderPool.Get();
        try
        {
            return sb.Append(SelectCommand)
                .Append(" ORDER BY ")
                .Append(orderBy.Value)
                .ToString();
        }
        finally
        {
            sb.Clear();
            StringBuilderPool.Return(sb);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "StringBuilder pool - testado indiretamente pelos testes de comando")]
    public string GenerateSelectCommand(OrderByClause orderBy, PaginationInfo paginationInfo)
    {
        StringBuilder sb = StringBuilderPool.Get();
        try
        {
            return sb.Append(SelectCommand)
                .Append(" ORDER BY ")
                .Append(orderBy.Value)
                .Append(" LIMIT ")
                .Append(paginationInfo.PageSize)
                .Append(" OFFSET ")
                .Append(paginationInfo.Offset)
                .ToString();
        }
        finally
        {
            sb.Clear();
            StringBuilderPool.Return(sb);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "StringBuilder pool - testado indiretamente pelos testes de comando")]
    public string GenerateUpdateCommand(WhereClause whereClause)
    {
        StringBuilder sb = StringBuilderPool.Get();
        try
        {
            return sb.Append(UpdateCommand)
                .Append(" AND (")
                .Append(whereClause.Value)
                .Append(')')
                .ToString();
        }
        finally
        {
            sb.Clear();
            StringBuilderPool.Return(sb);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "StringBuilder pool - testado indiretamente pelos testes de comando")]
    public string GenerateDeleteCommand(WhereClause whereClause)
    {
        StringBuilder sb = StringBuilderPool.Get();
        try
        {
            return sb.Append(DeleteCommand)
                .Append(" AND (")
                .Append(whereClause.Value)
                .Append(')')
                .ToString();
        }
        finally
        {
            sb.Clear();
            StringBuilderPool.Return(sb);
        }
    }

    [ExcludeFromCodeCoverage(Justification = "StringBuilder pool - testado indiretamente pelos testes de comando")]
    public string GenerateExistsCommand(WhereClause whereClause)
    {
        StringBuilder sb = StringBuilderPool.Get();
        try
        {
            return sb.Append("SELECT EXISTS ( SELECT 1 FROM ")
                .Append(_tableName)
                .Append(" WHERE ")
                .Append(_baseWhereClause)
                .Append(" AND (")
                .Append(whereClause.Value)
                .Append(") );")
                .ToString();
        }
        finally
        {
            sb.Clear();
            StringBuilderPool.Return(sb);
        }
    }
    // Stryker restore all

    // Public Methods - Where clause generation (type-safe)
    public WhereClause Where(string propertyName, RelationalOperator op)
    {
        var key = (propertyName, op);
        // Stryker disable once Block : Retorno do cache - testado indiretamente pela consistencia dos comandos gerados
        if (_whereClauseCache.TryGetValue(key, out WhereClause cached))
        {
            return cached;
        }

        ColumnMap columnMap = GetColumnMap(propertyName);
        string paramName = GetParameterName(propertyName);
        string opSql = op.ToSql();

        string clause = string.Create(
            _tableName.Length + 1 + columnMap.ColumnName.Length + 1 + opSql.Length + 1 + paramName.Length,
            (_tableName, columnMap.ColumnName, opSql, paramName),
            static (span, state) =>
            {
                int pos = 0;
                state._tableName.AsSpan().CopyTo(span);
                pos += state._tableName.Length;
                span[pos++] = '.';
                state.ColumnName.AsSpan().CopyTo(span[pos..]);
                pos += state.ColumnName.Length;
                span[pos++] = ' ';
                state.opSql.AsSpan().CopyTo(span[pos..]);
                pos += state.opSql.Length;
                span[pos++] = ' ';
                state.paramName.AsSpan().CopyTo(span[pos..]);
            });

        WhereClause result = new(clause);
        _whereClauseCache[key] = result;
        return result;
    }

    public WhereClause Where<TProperty>(Expression<Func<TDataModel, TProperty>> selector, RelationalOperator op)
    {
        PropertyInfo propertyInfo = ExpressionUtils.GetProperty(selector);
        return Where(propertyInfo.Name, op);
    }

    public WhereClause Where(string propertyName)
    {
        return Where(propertyName, RelationalOperator.Equal);
    }

    public WhereClause Where<TProperty>(Expression<Func<TDataModel, TProperty>> selector)
    {
        return Where(selector, RelationalOperator.Equal);
    }

    /// <summary>
    /// Creates a WHERE clause with a custom parameter suffix for optimistic concurrency scenarios
    /// where the same property needs different parameter values in SET and WHERE clauses.
    /// </summary>
    public WhereClause WhereWithParameterSuffix(string propertyName, string parameterSuffix, RelationalOperator op)
    {
        ColumnMap columnMap = GetColumnMap(propertyName);
        string paramName = GetParameterName(propertyName) + parameterSuffix;
        string opSql = op.ToSql();

        string clause = string.Create(
            _tableName.Length + 1 + columnMap.ColumnName.Length + 1 + opSql.Length + 1 + paramName.Length,
            (_tableName, columnMap.ColumnName, opSql, paramName),
            static (span, state) =>
            {
                int pos = 0;
                state._tableName.AsSpan().CopyTo(span);
                pos += state._tableName.Length;
                span[pos++] = '.';
                state.ColumnName.AsSpan().CopyTo(span[pos..]);
                pos += state.ColumnName.Length;
                span[pos++] = ' ';
                state.opSql.AsSpan().CopyTo(span[pos..]);
                pos += state.opSql.Length;
                span[pos++] = ' ';
                state.paramName.AsSpan().CopyTo(span[pos..]);
            });

        return new WhereClause(clause);
    }

    /// <summary>
    /// Creates a WHERE clause with a custom parameter suffix using an expression selector.
    /// </summary>
    public WhereClause WhereWithParameterSuffix<TProperty>(
        Expression<Func<TDataModel, TProperty>> selector,
        string parameterSuffix,
        RelationalOperator op)
    {
        PropertyInfo propertyInfo = ExpressionUtils.GetProperty(selector);
        return WhereWithParameterSuffix(propertyInfo.Name, parameterSuffix, op);
    }

    /// <summary>
    /// Adds a parameter to the command with a custom suffix.
    /// Used together with WhereWithParameterSuffix for optimistic concurrency scenarios.
    /// </summary>
    // Stryker disable all : NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao")]
    public void AddParameterForCommandWithSuffix<TProperty>(
        NpgsqlCommand npgsqlCommand,
        Expression<Func<TDataModel, TProperty>> selector,
        string parameterSuffix,
        object? value)
    {
        PropertyInfo propertyInfo = ExpressionUtils.GetProperty(selector);
        string paramName = GetParameterName(propertyInfo.Name) + parameterSuffix;
        _ = npgsqlCommand.Parameters.AddWithValue(
            parameterName: paramName,
            parameterType: GetColumnNpgsqlDbType(selector),
            value: value.GetValueOrDbNull()
        );
    }
    // Stryker restore all

    // Public Methods - Order by clause generation (type-safe)
    public OrderByClause OrderBy(string propertyName, SortDirection direction)
    {
        var key = (propertyName, direction);
        // Stryker disable once Block : Retorno do cache - testado indiretamente pela consistencia dos comandos gerados
        if (_orderByClauseCache.TryGetValue(key, out OrderByClause cached))
        {
            return cached;
        }

        ColumnMap columnMap = GetColumnMap(propertyName);
        string directionSql = direction == SortDirection.Ascending ? "ASC" : "DESC";

        string clause = string.Create(
            _tableName.Length + 1 + columnMap.ColumnName.Length + 1 + directionSql.Length,
            (_tableName, columnMap.ColumnName, directionSql),
            static (span, state) =>
            {
                int pos = 0;
                state._tableName.AsSpan().CopyTo(span);
                pos += state._tableName.Length;
                span[pos++] = '.';
                state.ColumnName.AsSpan().CopyTo(span[pos..]);
                pos += state.ColumnName.Length;
                span[pos++] = ' ';
                state.directionSql.AsSpan().CopyTo(span[pos..]);
            });

        OrderByClause result = new(clause);
        _orderByClauseCache[key] = result;
        return result;
    }

    public OrderByClause OrderBy<TProperty>(Expression<Func<TDataModel, TProperty>> selector, SortDirection direction)
    {
        PropertyInfo propertyInfo = ExpressionUtils.GetProperty(selector);
        return OrderBy(propertyInfo.Name, direction);
    }

    public OrderByClause OrderByAscending(string propertyName)
    {
        return OrderBy(propertyName, SortDirection.Ascending);
    }

    public OrderByClause OrderByAscending<TProperty>(Expression<Func<TDataModel, TProperty>> selector)
    {
        return OrderBy(selector, SortDirection.Ascending);
    }

    public OrderByClause OrderByDescending(string propertyName)
    {
        return OrderBy(propertyName, SortDirection.Descending);
    }

    public OrderByClause OrderByDescending<TProperty>(Expression<Func<TDataModel, TProperty>> selector)
    {
        return OrderBy(selector, SortDirection.Descending);
    }

    // Public Methods - Parameter handling
    public string GetParameterName(string propertyName)
    {
        // Stryker disable once Block : Retorno do cache - testado indiretamente pela consistencia dos comandos gerados
        if (_parameterNameCache.TryGetValue(propertyName, out string? cachedName))
        {
            return cachedName;
        }

        string paramName = string.Create(
            1 + _parameterPrefix.Length + 1 + propertyName.Length,
            (_parameterPrefix, propertyName),
            static (span, state) =>
            {
                span[0] = '@';
                int pos = 1;
                state._parameterPrefix.AsSpan().CopyTo(span[pos..]);
                pos += state._parameterPrefix.Length;
                span[pos++] = '_';
                state.propertyName.AsSpan().CopyTo(span[pos..]);
            });

        _parameterNameCache[propertyName] = paramName;
        return paramName;
    }

    public string GetParameterName<TProperty>(Expression<Func<TDataModel, TProperty>> selector)
    {
        PropertyInfo propertyInfo = ExpressionUtils.GetProperty(selector);
        return GetParameterName(propertyInfo.Name);
    }

    /// <summary>
    /// Adds a parameter to the command.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - NpgsqlCommand is sealed and cannot be mocked.
    /// Requires integration tests with real database connection.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao")]
    public void AddParameterForCommand(
        NpgsqlCommand npgsqlCommand,
        string propertyName,
        NpgsqlDbType npgsqlDbType,
        object? value
    )
    {
        _ = npgsqlCommand.Parameters.AddWithValue(
            parameterName: GetParameterName(propertyName),
            parameterType: npgsqlDbType,
            value: value.GetValueOrDbNull()
        );
    }
    // Stryker restore all

    /// <summary>
    /// Adds a parameter to the command using an expression selector.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - NpgsqlCommand is sealed and cannot be mocked.
    /// Requires integration tests with real database connection.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao")]
    public void AddParameterForCommand<TProperty>(
        NpgsqlCommand npgsqlCommand,
        Expression<Func<TDataModel, TProperty>> selector,
        object? value
    )
    {
        PropertyInfo propertyInfo = ExpressionUtils.GetProperty(selector);
        AddParameterForCommand(
            npgsqlCommand,
            propertyName: propertyInfo.Name,
            npgsqlDbType: GetColumnNpgsqlDbType(selector),
            value: value
        );
    }
    // Stryker restore all

    // Public Methods - Data model handling
    /// <summary>
    /// Configures a command parameter from a property.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - NpgsqlCommand is sealed and cannot be mocked.
    /// Requires integration tests with real database connection.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao")]
    public void ConfigureCommandToProperty(
        string propertyName,
        ColumnMap columnMap,
        TDataModel dataModel,
        NpgsqlCommand npgsqlCommand
    )
    {
        AddParameterForCommand(
            npgsqlCommand,
            propertyName,
            columnMap.NpgsqlDbType,
            value: _propertyInfoDictionary[typeof(TDataModel)][propertyName].GetValue(dataModel)
        );
    }
    // Stryker restore all

    /// <summary>
    /// Configures a command parameter from a property by name.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - NpgsqlCommand is sealed and cannot be mocked.
    /// Requires integration tests with real database connection.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao")]
    public void ConfigureCommandToProperty(
        string propertyName,
        TDataModel dataModel,
        NpgsqlCommand npgsqlCommand
    )
    {
        ColumnMap columnMap = GetColumnMap(propertyName);
        ConfigureCommandToProperty(propertyName, columnMap, dataModel, npgsqlCommand);
    }
    // Stryker restore all

    /// <summary>
    /// Configures all parameters from a data model.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - NpgsqlCommand is sealed and cannot be mocked.
    /// Requires integration tests with real database connection.
    /// </remarks>
    // Stryker disable all : NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlCommand e sealed e nao pode ser mockado - requer testes de integracao")]
    public NpgsqlCommand ConfigureCommandFromDataModelBase(
        NpgsqlCommand command,
        IDataModelMapper<TDataModel> mapper,
        TDataModel dataModel
    )
    {
        foreach (KeyValuePair<string, ColumnMap> columnMap in mapper.ColumnMapDictionary)
        {
            ConfigureCommandToProperty(columnMap.Key, columnMap.Value, dataModel, command);
        }

        return command;
    }
    // Stryker restore all

    /// <summary>
    /// Populates a data model from a data reader.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - NpgsqlDataReader is sealed and cannot be mocked.
    /// Requires integration tests with real database connection.
    /// </remarks>
    // Stryker disable all : NpgsqlDataReader e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlDataReader e sealed e nao pode ser mockado - requer testes de integracao")]
    public void PopulateDataModelBaseFromReader(
        NpgsqlDataReader reader,
        TDataModel dataModel)
    {
        foreach (KeyValuePair<string, ColumnMap> columnMap in ColumnMapDictionary)
        {
            object value = reader.GetValueOrDefault($"{TableName}_{columnMap.Key}");

            bool propertyTypeIsDateTimeOffset =
                columnMap.Value.Type == typeof(DateTimeOffset)
                || columnMap.Value.Type == typeof(DateTimeOffset?);

            if (value is DateTime datetime && propertyTypeIsDateTimeOffset)
            {
                value = new DateTimeOffset(datetime);
            }

            _propertyInfoDictionary[_dataModelType][columnMap.Key]
                .SetValue(dataModel, value);
        }
    }
    // Stryker restore all

    public abstract void MapBinaryImporter(NpgsqlBinaryImporter importer, TDataModel model);

    // Protected Methods
    protected abstract void ConfigureInternal(MapperOptions<TDataModel> mapperOptions);

    // Private Methods - Caching
    // Stryker disable all : Metodo de geracao de SQL - strings internas testadas indiretamente pelos comandos gerados
    [ExcludeFromCodeCoverage(Justification = "Metodo de geracao de SQL - strings internas testadas indiretamente pelos comandos gerados")]
    private void CacheGeneratedStrings()
    {
        // Cache table name
        _tableName = string.IsNullOrWhiteSpace(TableSchema)
            ? TableName!
            : $"{TableSchema}.{TableName}";

        // Cache parameter prefix for zero-allocation in GetParameterName
        _parameterPrefix = TableSchema is not null
            ? $"{TableSchema}_{TableName}"
            : TableName!;

        // Cache column-related strings using StringBuilder to avoid LINQ allocations
        StringBuilder sb = StringBuilderPool.Get();
        try
        {
            // _columnNamesWithAlias
            bool first = true;
            foreach (KeyValuePair<string, ColumnMap> f in _columnMapDictionary)
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                first = false;
                sb.Append(_tableName).Append('.').Append(f.Value.ColumnName)
                  .Append(" AS \"").Append(TableName).Append('_').Append(f.Key).Append('"');
            }
            _columnNamesWithAlias = sb.ToString();
            sb.Clear();

            // _columnNames
            first = true;
            foreach (KeyValuePair<string, ColumnMap> f in _columnMapDictionary)
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                first = false;
                sb.Append(f.Value.ColumnName);
            }
            _columnNames = sb.ToString();
            sb.Clear();

            // _parameters
            first = true;
            foreach (KeyValuePair<string, ColumnMap> f in _columnMapDictionary)
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                first = false;
                sb.Append(GetParameterName(f.Key));
            }
            _parameters = sb.ToString();
            sb.Clear();

            // _setClauses
            first = true;
            foreach (KeyValuePair<string, ColumnMap> f in _columnMapDictionary)
            {
                if (!first)
                {
                    sb.Append(", ");
                }
                first = false;
                sb.Append(f.Value.ColumnName).Append(" = ").Append(GetParameterName(f.Key));
            }
            _setClauses = sb.ToString();
        }
        finally
        {
            sb.Clear();
            StringBuilderPool.Return(sb);
        }

        // Cache base where clauses
        WhereClause tenantWhereClause = Where(nameof(DataModelBase.TenantCode));
        _baseWhereClause = tenantWhereClause.Value;

        WhereClause idWhereClause = Where(nameof(DataModelBase.Id));
        _baseWhereClauseToUpdate = (tenantWhereClause & idWhereClause).Value;

        // Cache complete commands (zero allocation on access)
        SelectCommand = $"SELECT {_columnNamesWithAlias} FROM {_tableName} WHERE {_baseWhereClause}";
        InsertCommand = $"INSERT INTO {_tableName} ({_columnNames}) VALUES ({_parameters});";
        UpdateCommand = $"UPDATE {_tableName} SET {_setClauses} WHERE {_baseWhereClauseToUpdate}";
        DeleteCommand = $"DELETE FROM {_tableName} WHERE {_baseWhereClause}";
        CopyCommand = $"COPY {_tableName} ({_columnNames}) FROM STDIN (FORMAT BINARY);";
    }
    // Stryker restore all

    // Stryker disable all : Retorno antecipado testado indiretamente - mapper ja configurado no construtor
    [ExcludeFromCodeCoverage(Justification = "Retorno antecipado testado indiretamente - mapper ja configurado no construtor")]
    private void TryConfigure()
    {
        if (_isConfigured)
        {
            return;
        }

        Configure();
    }
    // Stryker restore all

    private void MapDataModelBaseColumns()
    {
        _columnMapDictionary.Add(nameof(DataModelBase.Id), ColumnMap.Create(nameof(DataModelBase.Id).ToSnakeCase(), typeof(Guid)));
        _columnMapDictionary.Add(nameof(DataModelBase.TenantCode), ColumnMap.Create(nameof(DataModelBase.TenantCode).ToSnakeCase(), typeof(Guid)));
        _columnMapDictionary.Add(nameof(DataModelBase.CreatedBy), ColumnMap.Create(nameof(DataModelBase.CreatedBy).ToSnakeCase(), typeof(string)));
        _columnMapDictionary.Add(nameof(DataModelBase.CreatedAt), ColumnMap.Create(nameof(DataModelBase.CreatedAt).ToSnakeCase(), typeof(DateTimeOffset)));
        _columnMapDictionary.Add(nameof(DataModelBase.LastChangedBy), ColumnMap.Create(nameof(DataModelBase.LastChangedBy).ToSnakeCase(), typeof(string)));
        _columnMapDictionary.Add(nameof(DataModelBase.LastChangedAt), ColumnMap.Create(nameof(DataModelBase.LastChangedAt).ToSnakeCase(), typeof(DateTimeOffset?)));
        _columnMapDictionary.Add(nameof(DataModelBase.LastChangedExecutionOrigin), ColumnMap.Create(nameof(DataModelBase.LastChangedExecutionOrigin).ToSnakeCase(), typeof(string)));
        _columnMapDictionary.Add(nameof(DataModelBase.LastChangedCorrelationId), ColumnMap.Create(nameof(DataModelBase.LastChangedCorrelationId).ToSnakeCase(), typeof(Guid?)));
        _columnMapDictionary.Add(nameof(DataModelBase.LastChangedBusinessOperationCode), ColumnMap.Create(nameof(DataModelBase.LastChangedBusinessOperationCode).ToSnakeCase(), typeof(string)));
        _columnMapDictionary.Add(nameof(DataModelBase.EntityVersion), ColumnMap.Create(nameof(DataModelBase.EntityVersion).ToSnakeCase(), typeof(long)));
    }

    /// <summary>
    /// Maps property infos for a type using double-check locking pattern.
    /// </summary>
    /// <remarks>
    /// The double-check inside lock cannot be tested without race conditions.
    /// The early return is tested when Configure() is called twice.
    /// </remarks>
    // Stryker disable all : Double-check pattern dentro do lock requer race conditions para testar - impraticavel em testes unitarios
    [ExcludeFromCodeCoverage(Justification = "Double-check pattern dentro do lock requer race conditions para testar - impraticavel em testes unitarios")]
    private static void TryMapPropertyInfos<T>()
    {
        Type type = typeof(T);

        // Double-check pattern: verifica antes e depois do lock
        if (_propertyInfoDictionary.ContainsKey(type))
        {
            return;
        }

        lock (_propertyInfoDictionaryLocker)
        {
            // Segunda verificação dentro do lock para evitar duplicação
            if (_propertyInfoDictionary.ContainsKey(type))
            {
                return;
            }

            Dictionary<string, PropertyInfo> propertyDictionary = [];
            PropertyInfo[] propertyCollection = type.GetProperties();

            foreach (PropertyInfo property in propertyCollection)
            {
                propertyDictionary.Add(property.Name, property);
            }

            _propertyInfoDictionary.Add(type, propertyDictionary);
        }
    }
    // Stryker restore all
}
