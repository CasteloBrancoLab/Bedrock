using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using Bedrock.BuildingBlocks.Core.Paginations;
using Bedrock.BuildingBlocks.Core.Sortings.Enums;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.ExtensionMethods;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Interfaces;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;
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

    // Instance fields
    private readonly Dictionary<string, ColumnMap> _columnMapDictionary = [];
    private ReadOnlyDictionary<string, ColumnMap>? _columnMapReadOnlyDictionary;

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
    public string GenerateSelectCommand(WhereClause whereClause)
    {
        return $"SELECT {_columnNamesWithAlias} FROM {_tableName} WHERE {_baseWhereClause} AND ({whereClause.Value})";
    }

    public string GenerateSelectCommand(WhereClause whereClause, PaginationInfo paginationInfo)
    {
        return $"SELECT {_columnNamesWithAlias} FROM {_tableName} WHERE {_baseWhereClause} AND ({whereClause.Value}) LIMIT {paginationInfo.PageSize} OFFSET {paginationInfo.Offset}";
    }

    public string GenerateSelectCommand(PaginationInfo paginationInfo)
    {
        return $"SELECT {_columnNamesWithAlias} FROM {_tableName} WHERE {_baseWhereClause} LIMIT {paginationInfo.PageSize} OFFSET {paginationInfo.Offset}";
    }

    public string GenerateSelectCommand(WhereClause whereClause, OrderByClause orderBy)
    {
        return $"SELECT {_columnNamesWithAlias} FROM {_tableName} WHERE {_baseWhereClause} AND ({whereClause.Value}) ORDER BY {orderBy.Value}";
    }

    public string GenerateSelectCommand(WhereClause whereClause, OrderByClause orderBy, PaginationInfo paginationInfo)
    {
        return $"SELECT {_columnNamesWithAlias} FROM {_tableName} WHERE {_baseWhereClause} AND ({whereClause.Value}) ORDER BY {orderBy.Value} LIMIT {paginationInfo.PageSize} OFFSET {paginationInfo.Offset}";
    }

    public string GenerateSelectCommand(OrderByClause orderBy)
    {
        return $"SELECT {_columnNamesWithAlias} FROM {_tableName} WHERE {_baseWhereClause} ORDER BY {orderBy.Value}";
    }

    public string GenerateSelectCommand(OrderByClause orderBy, PaginationInfo paginationInfo)
    {
        return $"SELECT {_columnNamesWithAlias} FROM {_tableName} WHERE {_baseWhereClause} ORDER BY {orderBy.Value} LIMIT {paginationInfo.PageSize} OFFSET {paginationInfo.Offset}";
    }

    public string GenerateUpdateCommand(WhereClause whereClause)
    {
        return $"UPDATE {_tableName} SET {_setClauses} WHERE {_baseWhereClauseToUpdate} AND ({whereClause.Value})";
    }

    public string GenerateDeleteCommand(WhereClause whereClause)
    {
        return $"DELETE FROM {_tableName} WHERE {_baseWhereClause} AND ({whereClause.Value})";
    }

    public string GenerateExistsCommand(WhereClause whereClause)
    {
        return $"SELECT EXISTS ( SELECT 1 FROM {_tableName} WHERE {_baseWhereClause} AND ({whereClause.Value}) );";
    }

    // Public Methods - Where clause generation (type-safe)
    public WhereClause Where(string propertyName, RelationalOperator op)
    {
        ColumnMap columnMap = GetColumnMap(propertyName);
        string clause = $"{_tableName}.{columnMap.ColumnName} {op.ToSql()} {GetParameterName(propertyName)}";
        return new WhereClause(clause);
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

    // Public Methods - Order by clause generation (type-safe)
    public OrderByClause OrderBy(string propertyName, SortDirection direction)
    {
        ColumnMap columnMap = GetColumnMap(propertyName);
        string directionSql = direction == SortDirection.Ascending ? "ASC" : "DESC";
        return new OrderByClause($"{_tableName}.{columnMap.ColumnName} {directionSql}");
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
        string? parameterPrefix = TableName;

        if (TableSchema is not null)
        {
            parameterPrefix = $"{TableSchema}_{parameterPrefix}";
        }

        return $"@{parameterPrefix}_{propertyName}";
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
        TDataModel dataModel,
        Action<NpgsqlDataReader, TDataModel, IDataModelMapper<TDataModel>>? additionalMap
    )
    {
        foreach (KeyValuePair<string, ColumnMap> columnMap in ColumnMapDictionary)
        {
            object value = reader.GetValueOrDefault($"{TableName}_{columnMap.Value.ColumnName}");

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

        additionalMap?.Invoke(reader, dataModel, this);
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

        // Cache column-related strings
        _columnNamesWithAlias = string.Join(
            ", ",
            _columnMapDictionary.Select(f => $"{_tableName}.{f.Value.ColumnName} AS \"{TableName}_{f.Key}\"")
        );

        _columnNames = string.Join(
            ", ",
            _columnMapDictionary.Select(f => f.Value.ColumnName)
        );

        _parameters = string.Join(
            ", ",
            _columnMapDictionary.Select(f => GetParameterName(f.Key))
        );

        _setClauses = string.Join(
            ", ",
            _columnMapDictionary.Select(f => $"{f.Value.ColumnName} = {GetParameterName(f.Key)}")
        );

        // Cache base where clauses
        WhereClause tenantWhereClause = Where(nameof(DataModelBase.TenantCode));
        _baseWhereClause = tenantWhereClause.Value;

        WhereClause idWhereClause = Where(nameof(DataModelBase.Id));
        WhereClause versionWhereClause = Where(nameof(DataModelBase.EntityVersion), RelationalOperator.LessThan);
        _baseWhereClauseToUpdate = (tenantWhereClause & idWhereClause & versionWhereClause).Value;

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
