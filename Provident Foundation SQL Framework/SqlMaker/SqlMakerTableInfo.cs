namespace Provident.Foundations.SqlMaker;

/// <summary>Describes a table discovered in a SQL Server database.</summary>
/// <param name="Schema">The table's schema name.</param>
/// <param name="Name">The table name.</param>
public sealed record SqlMakerTableInfo(string Schema, string Name);
