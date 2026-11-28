![Provident Foundations banner](../../Core/Provident%20Foundations%20Framework/Provident.Foundations/PFBanner.png)

# Provident Foundations SQL Server Framework

A .NET library of SQL Server data-access, schema, SQL-generation, and database-maintenance components. It builds on the `Provident.Foundations` core library and uses Microsoft.Data.SqlClient.

## Requirements

- .NET 11 (`net11.0`)
- SQL Server and a connection string for operations that access a database
- A compatible x64 runtime; the project sets `PlatformTarget` to `x64`

The library references Microsoft.Data.SqlClient, SQL Server DacFx, SQL Server Management Objects, and `Provident.Foundations`.

## Install

```sh
dotnet add package Provident.Foundations.SqlServer
```

## What the library provides

- **SQL provider and data access** (`Provident.Foundations.Sql.Provider`): create a `SqlDataProvider` from a connection string and execute SQL, stored procedures, commands, batches, and transactions with synchronous and asynchronous APIs.
- **Schema models** (`Provident.Foundations.Sql.Schema`): represent servers, databases, tables, columns, keys, indexes, and related SQL metadata; load schema information through the SQL provider.
- **SQL DOM and writers** (`Provident.Foundations.Sql.Schema.DOM`): compose SQL statements and expressions and render them as T-SQL.
- **Maintenance and statistics** (`Provident.Foundations.Sql.Maintenance`): database and index statistics plus SQL Server maintenance operations.
- **SQL Maker project support**: read and write `.adbx` project files, discover database tables, and generate selected entity and stored-procedure source.

## Discover tables

`SqlMakerTableDiscovery` queries the user tables visible in the database specified by the connection string and returns them ordered by schema and name:

```csharp
using Provident.Foundations.Sql.SqlMaker;

var tables = await SqlMakerTableDiscovery.GetTablesAsync(connectionString, cancellationToken);
foreach (var table in tables)
{
    Console.WriteLine($"{table.Schema}.{table.Name}");
}
```

The account in the connection string must be able to connect to the target database and read its table metadata.

## SQL Maker projects and generation

The SQL Maker project model (`Provident.Foundations.SqlMaker`) stores project options, data sources, table profiles, and stored-procedure definitions in `.adbx` XML files. `SqlDataProject.Load()` and `Save()` read and write its configured `FileName`. The model writes connection strings supplied by callers as-is; do not save credentials in a project file unless that is intentional.

The generator and discovery APIs are in `Provident.Foundations.Sql.SqlMaker`. Given project options, a table profile, and a loaded `SqlTable` schema, `SqlMakerCodeGenerator.GenerateEntityClass(...)` returns nullable-enabled C# source, and `GenerateStoredProcedures(...)` returns the configured T-SQL procedures as `SqlMakerGeneratedProcedure` values (`Name` and `Source`). These methods return source in memory; they do not create files, data-access classes, relationship/join code, or custom procedure bodies. Only configured procedure names are generated. Unsupported SQL types, invalid configuration, or insufficient key metadata result in exceptions rather than silently producing incomplete output. Key-based procedures require schema metadata confirming a single-column primary key or unique constraint.

The SQL Maker project model uses the `Provident.Foundations.SqlMaker` namespace for compatibility; the generator and table-discovery types use `Provident.Foundations.Sql.SqlMaker`.

## SQL statement generation

For a `SqlCodeSelectStatement`, add expressions to `GroupByExpressions` and `SqlCodeOrderByItem` values to `OrderByItems` to include optional `GROUP BY` and `ORDER BY` clauses. The SQL writer supports both synchronous and asynchronous output; empty collections omit their clauses.
