using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using Bedrock.BuildingBlocks.Persistence.PostgreSql.DataModels;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.Mappers.Models;

public sealed class MapperOptions<TDataModel>
    where TDataModel : DataModelBase
{
    // Fields
    private readonly Dictionary<string, ColumnMap> _fieldDictionary = [];
    private ReadOnlyDictionary<string, ColumnMap>? _fieldDictionaryReadOnly;

    // Properties
    public string? TableSchema { get; private set; }
    public string? TableName { get; private set; }
    public ReadOnlyDictionary<string, ColumnMap> FieldDictionary
    {
        get
        {
            // Stryker disable once all : Cache null-coalescing - mutante equivalente (apenas afeta primeira chamada)
            return _fieldDictionaryReadOnly ??= _fieldDictionary.AsReadOnly();
        }
    }

    // Public Methods
    public MapperOptions<TDataModel> MapTable(string? schema, string name)
    {
        TableSchema = schema;
        TableName = name;

        return this;
    }
    public MapperOptions<TDataModel> MapColumn(string propertyName, string columnName, Type columnType)
    {
        _fieldDictionary.Add(propertyName, ColumnMap.Create(columnName, columnType));

        return this;
    }
    public MapperOptions<TDataModel> MapColumn<T>(string propertyName, string columnName)
    {
        return MapColumn(propertyName, columnName, columnType: typeof(T));
    }
    public MapperOptions<TDataModel> MapColumn<T>(string propertyName)
    {
        return MapColumn<T>(propertyName, propertyName.ToSnakeCase());
    }
    public MapperOptions<TDataModel> MapColumn<TProperty>(
        Expression<Func<TDataModel, TProperty>> selector,
        string columnName
    )
    {
        PropertyInfo propertyInfo = ExpressionUtils.GetProperty(selector);

        return MapColumn(
            propertyName: propertyInfo.Name,
            columnName,
            columnType: propertyInfo.PropertyType
        );
    }
    public MapperOptions<TDataModel> MapColumn<TProperty>(
        Expression<Func<TDataModel, TProperty>> selector,
        Type columnType
    )
    {
        PropertyInfo propertyInfo = ExpressionUtils.GetProperty(selector);

        return MapColumn(
            propertyName: propertyInfo.Name,
            columnName: propertyInfo.Name.ToSnakeCase(),
            columnType: columnType
        );
    }
    public MapperOptions<TDataModel> MapColumn<TProperty, TType>(
        Expression<Func<TDataModel, TProperty>> selector
    )
    {
        PropertyInfo propertyInfo = ExpressionUtils.GetProperty(selector);

        return MapColumn<TType>(propertyName: propertyInfo.Name, columnName: propertyInfo.Name.ToSnakeCase());
    }
    public MapperOptions<TDataModel> MapColumn<TProperty>(
        Expression<Func<TDataModel, TProperty>> selector
    )
    {
        PropertyInfo propertyInfo = ExpressionUtils.GetProperty(selector);

        return MapColumn(
            propertyName: propertyInfo.Name,
            columnName: propertyInfo.Name.ToSnakeCase(),
            columnType: propertyInfo.PropertyType
        );
    }
    public MapperOptions<TDataModel> AutoMapColumns()
    {
        PropertyInfo[] propertyInfoCollection = [..
            typeof(TDataModel)
            .GetProperties()
            .Where(p => p.CanRead && p.CanWrite && p.GetIndexParameters().Length == 0 && p.DeclaringType == typeof(TDataModel))
        ];

        foreach (PropertyInfo propertyInfo in propertyInfoCollection)
        {
            _ = MapColumn(
                propertyName: propertyInfo.Name,
                columnName: propertyInfo.Name.ToSnakeCase(),
                columnType: propertyInfo.PropertyType
            );
        }

        return this;
    }
}
