using Microsoft.Data.SqlClient;

namespace Adaptive.SqlMaker;

/// <summary>Describes a table discovered in a SQL Server database.</summary>
/// <param name="Schema">The table's schema name.</param>
/// <param name="Name">The table name.</param>
public sealed record SqlMakerTableInfo(string Schema, string Name);

/// <summary>Discovers user tables for a SQL Maker project.</summary>
public static class SqlMakerTableDiscovery
{
    /// <summary>Reads the user tables visible to the supplied SQL Server connection.</summary>
    /// <param name="connectionString">The connection string for the target database.</param>
    /// <param name="cancellationToken">A token that cancels the discovery operation.</param>
    /// <returns>The tables ordered by schema and name.</returns>
    public static async Task<IReadOnlyList<SqlMakerTableInfo>> GetTablesAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        var tables = new List<SqlMakerTableInfo>();
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT s.name, t.name FROM sys.tables AS t INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id WHERE t.is_ms_shipped = 0 ORDER BY s.name, t.name";
        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            tables.Add(new SqlMakerTableInfo(reader.GetString(0), reader.GetString(1)));
        return tables;
    }
}
