using System.Diagnostics.CodeAnalysis;
using Npgsql;

namespace Bedrock.BuildingBlocks.Persistence.PostgreSql.ExtensionMethods;

public static class DataReaderExtensionMethods
{
    /// <summary>
    /// Gets a value from the reader or default if null.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - NpgsqlDataReader is sealed and cannot be mocked.
    /// Requires integration tests with real database connection.
    /// </remarks>
    // Stryker disable all : NpgsqlDataReader e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlDataReader e sealed e nao pode ser mockado - requer testes de integracao")]
    public static object GetValueOrDefault(this NpgsqlDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? default! : reader.GetValue(ordinal);
    }
    // Stryker restore all

    /// <summary>
    /// Gets a typed value from the reader or default if null.
    /// </summary>
    /// <remarks>
    /// Cannot be unit tested - NpgsqlDataReader is sealed and cannot be mocked.
    /// Requires integration tests with real database connection.
    /// </remarks>
    // Stryker disable all : NpgsqlDataReader e sealed e nao pode ser mockado - requer testes de integracao
    [ExcludeFromCodeCoverage(Justification = "NpgsqlDataReader e sealed e nao pode ser mockado - requer testes de integracao")]
    public static T GetValueOrDefault<T>(this NpgsqlDataReader reader, string columnName)
    {
        int ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? default! : reader.GetFieldValue<T>(ordinal);
    }
    // Stryker restore all

    public static object GetValueOrDbNull(this object? value)
    {
        return value ?? DBNull.Value;
    }
}
