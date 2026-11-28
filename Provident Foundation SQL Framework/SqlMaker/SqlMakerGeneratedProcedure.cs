namespace Provident.Foundations.Sql.SqlMaker;

/// <summary>Represents one generated SQL Maker stored procedure artifact.</summary>
/// <param name="Name">The schema-qualified stored procedure name.</param>
/// <param name="Source">The generated T-SQL source.</param>
public sealed record SqlMakerGeneratedProcedure(string Name, string Source);
