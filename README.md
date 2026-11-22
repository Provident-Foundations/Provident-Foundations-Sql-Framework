![Provident Foundations banner](../../Core/Provident%20Foundations%20Framework/Provident.Foundations/PFBanner.png)

# Provident Foundations SQL Server Framework

A .NET library for SQL Server application development, providing data-access foundations, schema models and SQL generation alongside database maintenance helpers. It builds on the core Provident Foundations library.

## What's included

- Data-access base classes and SQL Server provider helpers.
- Schema and metadata models, a SQL DOM and writers for building SQL statements.
- Database maintenance and statistics types.
- SQL Maker project persistence, table discovery and generation of selected artifacts, described below.

## Requirements and use

The project targets **.NET 11** (`net11.0`) and references the core framework, Microsoft.Data.SqlClient, DacFx and SQL Server Management Objects. Reference the SQL framework project or `Provident.Foundations.SqlServer` package from a compatible application. Operations that connect to SQL Server require an accessible server and appropriate credentials and permissions.

## SQL Maker projects

The framework includes the SQL Maker project model and `.adbx` reader and writer in the `Adaptive.SqlMaker` namespace. The namespace is retained for compatibility with the archived SQL Maker application. Projects store connection information, project options, table profiles, and stored procedures. Existing files produced by the older writer with tables directly under the project root can also be loaded.

The Visual Studio Toolset provides **SQL > New SQL Maker Project** to enter project options, load and select SQL Server tables, and save a project. Passwords entered in the toolset are removed before the connection string is saved; SQL authentication passwords must be supplied again for future connections. The project model itself writes the connection strings supplied by callers verbatim: callers must remove credentials before saving if those credentials should not be stored in the project file.

The archived SQL Maker exposed project/profile options but did not ship a code-generation implementation. The new `SqlMakerCodeGenerator` provides explicit, in-memory generation from a supplied SQL table schema and table profile. `GenerateEntityClass` emits a nullable-enabled C# entity using the configured entity namespace, standard imports, region option, and data definition class name. `GenerateStoredProcedures` emits configured Get All, Get By ID, Insert, Update, and Delete procedure scripts, applying project/profile procedure name prefixes and suffixes, the configured ID field, and soft-delete settings. Only procedures named in the table profile are emitted; callers decide where to write returned source. Both methods require a matching profile table name; profile-qualified schema names are checked against the supplied table schema. Procedure scripts use that schema and `CREATE OR ALTER PROCEDURE`.

Generation deliberately does not create data-access classes, relationship/join members or queries, user-custom procedure bodies, or files on disk. Requested joins raise `NotSupportedException` unless the table profile explicitly opts out with `NoSubJoins`; missing key/soft-delete configuration, invalid names, and unsupported SQL types are reported as errors instead of being silently skipped. Entity generation requires `DataDefinitionClassName` and rejects a generated property colliding with the class identifier. Key-based procedures require an explicit profile or project ID field plus metadata proving the field is the sole key column of a single-column primary key or unique constraint; otherwise generation refuses potentially multirow reads, updates, or deletes. The methods use only the options named above: `DataAccessNamespace`, data-access class names/paths, entity output paths, and other profile settings such as friendly/singular/plural names, description, join naming, and standard/key/subjoin field lists are not interpreted. Built-in SQL type support is explicit; user-defined and unsupported types are rejected. Fixed CHAR/NCHAR/BINARY parameters reject MAX, and Unicode parameter lengths must be even byte counts within SQL Server's 4,000-character limit.

Tables with the same name can be included when they belong to different schemas. Use `Tables[qualifier, name]` for an exact match; an unqualified lookup or removal throws when multiple tables share that name.

## SELECT statements

Add SQL expressions to `SqlCodeSelectStatement.GroupByExpressions` and `SqlCodeOrderByItem` instances to `OrderByItems` to render optional GROUP BY and ORDER BY clauses. Both synchronous and asynchronous `SqlWriter` methods support these clauses; empty collections omit them.
