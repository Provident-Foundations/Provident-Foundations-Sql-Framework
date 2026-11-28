using System.Globalization;
using System.Text;
using Provident.Foundations.Sql.Schema;
using Provident.Foundations.Sql.Schema.DOM.Enumerations;
using Provident.Foundations.SqlMaker;

namespace Provident.Foundations.Sql.SqlMaker;

/// <summary>Contains source generators for explicitly configured SQL Maker table artifacts.</summary>
public static class SqlMakerCodeGenerator
{
    private static readonly HashSet<string> CSharpKeywords = new(StringComparer.Ordinal)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class", "const",
        "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event", "explicit", "extern",
        "false", "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit", "in", "int", "interface",
        "internal", "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc",
        "static", "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
        "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    };

    /// <summary>Generates a C# entity class from a table schema and its SQL Maker profile.</summary>
    /// <param name="options">Project options that control namespaces, imports, and regions.</param>
    /// <param name="profile">The profile that supplies the generated entity class name.</param>
    /// <param name="table">The SQL table schema to generate from.</param>
    /// <returns>The complete nullable-enabled C# source file.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="ArgumentException">The profile or schema contains invalid required values.</exception>
    /// <exception cref="NotSupportedException">The profile requests relationship generation or contains an unsupported SQL type.</exception>
    public static string GenerateEntityClass(SqlDataProjectOptions options, SqlDataProjectTableProfile profile, SqlTable table)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(table);

        if (options.GenerateJoins && !profile.NoSubJoins)
            throw new NotSupportedException("SQL Maker relationship and join code generation is not supported.");

        string className = RequireIdentifier(profile.DataDefinitionClassName, nameof(profile.DataDefinitionClassName));
        string? entityNamespace = FormatNamespace(options.EntityNamespace, nameof(options.EntityNamespace));
        ValidateProfileTableMatch(profile, table);
        if (table.Columns.Count == 0)
            throw new ArgumentException("The table schema must contain at least one column.", nameof(table));

        var columns = new List<(SqlColumn Column, string PropertyName, string TypeName, string OriginalName)>();
        var propertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (SqlColumn column in table.Columns)
        {
            if (string.IsNullOrEmpty(column.ColumnName))
                throw new ArgumentException("Every table column must have a name.", nameof(table));

            string propertyName = MakePropertyIdentifier(column.ColumnName);
            if (string.Equals(propertyName, className, StringComparison.Ordinal))
                throw new ArgumentException($"Column '{column.ColumnName}' generates a property with the same identifier as class '{className}'.", nameof(table));
            if (!propertyNames.Add(propertyName))
                throw new ArgumentException($"Column names produce a duplicate C# property '{propertyName}'.", nameof(table));

            columns.Add((column, EscapeIdentifier(propertyName), GetCSharpType(column), column.ColumnName));
        }

        var output = new StringBuilder();
        output.AppendLine("#nullable enable");
        if (options.StandardNamespaces is not null)
        {
            foreach (string import in options.StandardNamespaces.Cast<string>().Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).Where(x => !string.Equals(x, "System", StringComparison.Ordinal)).Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal))
                output.Append("using ").Append(FormatNamespace(import, nameof(options.StandardNamespaces))).AppendLine(";");
        }

        output.AppendLine();
        if (entityNamespace is not null)
        {
            output.Append("namespace ").Append(entityNamespace).AppendLine();
            output.AppendLine("{");
        }

        string indent = entityNamespace is null ? string.Empty : "    ";
        output.Append(indent).Append("public class ").Append(EscapeIdentifier(className)).AppendLine();
        output.Append(indent).AppendLine("{");
        string memberIndent = indent + "    ";
        if (options.UseRegions)
            output.Append(memberIndent).AppendLine("#region Properties");

        foreach ((SqlColumn column, string propertyName, string typeName, string originalName) in columns)
        {
            output.Append(memberIndent).Append("[global::System.ComponentModel.DataAnnotations.Schema.Column(\"")
                .Append(EscapeCSharpString(originalName)).AppendLine("\")] ");
            output.Append(memberIndent).Append("public ").Append(typeName).Append(' ').Append(propertyName).Append(" { get; set; }");
            if (!column.IsNullable && IsReferenceType(typeName))
                output.Append(" = null!;");
            output.AppendLine();
        }

        if (options.UseRegions)
            output.Append(memberIndent).AppendLine("#endregion");
        output.Append(indent).AppendLine("}");
        if (entityNamespace is not null)
            output.AppendLine("}");
        return output.ToString();
    }

    /// <summary>Generates configured CRUD stored procedures for a table.</summary>
    /// <remarks>
    /// Only procedures with a non-empty profile name are emitted. The profile table name must match the supplied schema;
    /// key-based reads, updates, and deletes require metadata for a single-column primary key or unique constraint.
    /// Relationship queries, custom procedure bodies, and data-access classes are not generated by this method.
    /// </remarks>
    /// <param name="options">Project options that control name prefixes, suffixes, and default key/soft-delete settings.</param>
    /// <param name="profile">The per-table profile and configured procedure names.</param>
    /// <param name="table">The SQL table schema to generate from.</param>
    /// <returns>The generated procedures, in GetAll, GetById, Insert, Update, Delete order.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    /// <exception cref="ArgumentException">A required table, schema, column, or key setting is invalid.</exception>
    /// <exception cref="NotSupportedException">A requested procedure needs an unsupported SQL type or unique-key metadata is insufficient to guarantee a single-row predicate.</exception>
    public static IReadOnlyList<SqlMakerGeneratedProcedure> GenerateStoredProcedures(SqlDataProjectOptions options, SqlDataProjectTableProfile profile, SqlTable table)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(table);
        if (options.GenerateJoins && !profile.NoSubJoins)
            throw new NotSupportedException("SQL Maker relationship and join code generation is not supported.");

        (string schemaName, string tableName) = ValidateProfileTableMatch(profile, table);
        if (table.Columns.Count == 0)
            throw new ArgumentException("The table schema must contain at least one column.", nameof(table));
        string qualifiedTable = QuoteIdentifier(schemaName) + "." + QuoteIdentifier(tableName);

        var columns = table.Columns.ToList();
        if (columns.Any(column => string.IsNullOrWhiteSpace(column.ColumnName)))
            throw new ArgumentException("Every table column must have a name.", nameof(table));
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (SqlColumn column in columns)
            if (!names.Add(column.ColumnName!))
                throw new ArgumentException($"The table contains duplicate column name '{column.ColumnName}'.", nameof(table));
        if (columns.Count(column => column.IsIdentity) > 1)
            throw new ArgumentException("The table schema reports more than one identity column.", nameof(table));
        Dictionary<SqlColumn, string> parameterNames = CreateParameterNames(columns);
        string Parameter(SqlColumn column) => parameterNames[column];

        bool softDelete = profile.SoftDelete || options.DefaultSoftDelete;
        SqlColumn? softDeleteColumn = null;
        if (softDelete)
        {
            string softDeleteField = RequireName(profile.SoftDeleteField ?? options.DefaultSoftDeleteField, "soft-delete field");
            softDeleteColumn = FindColumn(columns, softDeleteField);
            if ((SqlDataTypes)softDeleteColumn.TypeId != SqlDataTypes.Bit || softDeleteColumn.IsNullable || softDeleteColumn.IsComputed || softDeleteColumn.IsIdentity)
                throw new ArgumentException("The soft-delete field must be a writable, non-nullable bit column.", "softDeleteField");
        }

        bool hasIdBasedProcedure = !string.IsNullOrWhiteSpace(profile.GetByIdSpName) ||
            !string.IsNullOrWhiteSpace(profile.UpdateSpName) || !string.IsNullOrWhiteSpace(profile.DeleteSpName);
        SqlColumn? idColumn = null;
        if (hasIdBasedProcedure)
        {
            string idField = RequireName(profile.IdField ?? options.DefaultIdField, "ID field");
            idColumn = FindColumn(columns, idField);
            if (idColumn.IsNullable)
                throw new ArgumentException("The configured ID field must be non-nullable.", "idField");
            if (!IsSingleColumnUniqueConstraint(table, idColumn))
                throw new NotSupportedException($"The configured ID field '{idField}' is not verified by a single-column primary key or unique constraint in the supplied schema metadata. Refusing to generate potentially multirow reads, updates, or deletes.");
        }

        var procedures = new List<SqlMakerGeneratedProcedure>();
        var generatedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddConfigured(profile.GetAllSpName, StoredProcedureType.GetAllRecords, "GetAllSpName");
        AddConfigured(profile.GetByIdSpName, StoredProcedureType.GetRecordById, "GetByIdSpName");
        AddConfigured(profile.InsertSpName, StoredProcedureType.Insert, "InsertSpName");
        AddConfigured(profile.UpdateSpName, StoredProcedureType.Update, "UpdateSpName");
        AddConfigured(profile.DeleteSpName, StoredProcedureType.Delete, "DeleteSpName");
        return procedures;

        void AddConfigured(string? baseName, StoredProcedureType type, string profileField)
        {
            if (string.IsNullOrWhiteSpace(baseName))
                return;

            string procedureName = (options.StoredProcedureNamePrefix ?? string.Empty) +
                (profile.StoredProcedureNamePrefix ?? string.Empty) + baseName.Trim() +
                (profile.StoredProcedureNameSuffix ?? string.Empty) + (options.StoredProcedureNameSuffix ?? string.Empty);
            if (procedureName.Length > 128)
                throw new ArgumentException("A generated stored procedure name cannot exceed 128 characters.", profileField);
            if (!generatedNames.Add(procedureName))
                throw new ArgumentException($"More than one configured procedure resolves to '{procedureName}'.", profileField);

            string sql = type switch
            {
                StoredProcedureType.GetAllRecords => GenerateGetAll(procedureName),
                StoredProcedureType.GetRecordById => GenerateGetById(procedureName, idColumn!),
                StoredProcedureType.Insert => GenerateInsert(procedureName),
                StoredProcedureType.Update => GenerateUpdate(procedureName, idColumn!),
                StoredProcedureType.Delete => GenerateDelete(procedureName, idColumn!),
                _ => throw new NotSupportedException($"Stored procedure type '{type}' is not supported.")
            };
            procedures.Add(new SqlMakerGeneratedProcedure(schemaName + "." + procedureName, sql));
        }

        string GenerateGetAll(string procedureName)
        {
            string where = softDelete ? " WHERE " + QuoteIdentifier(softDeleteColumn!.ColumnName!) + " = 0" : string.Empty;
            return BuildProcedure(procedureName, Array.Empty<SqlColumn>(), $"SELECT * FROM {qualifiedTable}{where};");
        }

        string GenerateGetById(string procedureName, SqlColumn id)
        {
            string where = QuoteIdentifier(id.ColumnName!) + " = " + Parameter(id);
            if (softDelete)
                where += " AND " + QuoteIdentifier(softDeleteColumn!.ColumnName!) + " = 0";
            return BuildProcedure(procedureName, new[] { id }, $"SELECT * FROM {qualifiedTable} WHERE {where};");
        }

        string GenerateInsert(string procedureName)
        {
            List<SqlColumn> insertColumns = columns.Where(column => !column.IsComputed && !column.IsIdentity &&
                (SqlDataTypes)column.TypeId != SqlDataTypes.TimeStamp && !ReferenceEquals(column, softDeleteColumn)).ToList();
            string statement;
            if (insertColumns.Count == 0 && !softDelete)
            {
                statement = $"INSERT INTO {qualifiedTable} DEFAULT VALUES;";
            }
            else
            {
                string targetColumns = string.Join(", ", insertColumns.Select(column => QuoteIdentifier(column.ColumnName!)));
                string values = string.Join(", ", insertColumns.Select(Parameter));
                if (softDelete)
                {
                    targetColumns = string.IsNullOrEmpty(targetColumns) ? QuoteIdentifier(softDeleteColumn!.ColumnName!) : targetColumns + ", " + QuoteIdentifier(softDeleteColumn!.ColumnName!);
                    values = string.IsNullOrEmpty(values) ? "0" : values + ", 0";
                }
                statement = $"INSERT INTO {qualifiedTable} ({targetColumns}) VALUES ({values});";
            }
            return BuildProcedure(procedureName, insertColumns, statement);
        }

        string GenerateUpdate(string procedureName, SqlColumn id)
        {
            List<SqlColumn> updateColumns = columns.Where(column => !column.IsComputed && !column.IsIdentity &&
                (SqlDataTypes)column.TypeId != SqlDataTypes.TimeStamp && !ReferenceEquals(column, id) &&
                !ReferenceEquals(column, softDeleteColumn)).ToList();
            if (updateColumns.Count == 0)
                throw new ArgumentException("The table has no writable non-key columns for the configured update procedure.", "table");
            SqlColumn[] parameters = updateColumns.Append(id).ToArray();
            string assignments = string.Join(", ", updateColumns.Select(column => QuoteIdentifier(column.ColumnName!) + " = " + Parameter(column)));
            string where = QuoteIdentifier(id.ColumnName!) + " = " + Parameter(id);
            if (softDelete)
                where += " AND " + QuoteIdentifier(softDeleteColumn!.ColumnName!) + " = 0";
            return BuildProcedure(procedureName, parameters, $"UPDATE {qualifiedTable} SET {assignments} WHERE {where};");
        }

        string GenerateDelete(string procedureName, SqlColumn id)
        {
            string where = QuoteIdentifier(id.ColumnName!) + " = " + Parameter(id);
            string statement;
            if (softDelete)
            {
                statement = $"UPDATE {qualifiedTable} SET {QuoteIdentifier(softDeleteColumn!.ColumnName!)} = 1 WHERE {where} AND {QuoteIdentifier(softDeleteColumn.ColumnName!)} = 0;";
            }
            else
            {
                statement = $"DELETE FROM {qualifiedTable} WHERE {where};";
            }
            return BuildProcedure(procedureName, new[] { id }, statement);
        }

        string BuildProcedure(string procedureName, IEnumerable<SqlColumn> parameters, string statement)
        {
            var parameterDeclarations = parameters.Select(column => Parameter(column) + " " + GetSqlType(column));
            string signature = string.Join("," + Environment.NewLine + "    ", parameterDeclarations);
            if (signature.Length > 0)
                signature = Environment.NewLine + "    " + signature;
            return $"CREATE OR ALTER PROCEDURE {QuoteIdentifier(schemaName)}.{QuoteIdentifier(procedureName)}{signature}{Environment.NewLine}AS{Environment.NewLine}BEGIN{Environment.NewLine}    SET NOCOUNT ON;{Environment.NewLine}    {statement}{Environment.NewLine}END;{Environment.NewLine}GO";
        }
    }

    private static string GetCSharpType(SqlColumn column)
    {
        SqlDataTypes sqlType = (SqlDataTypes)column.TypeId;
        string name = sqlType switch
        {
            SqlDataTypes.BigInt => "long",
            SqlDataTypes.Int => "int",
            SqlDataTypes.SmallInt => "short",
            SqlDataTypes.TinyInt => "byte",
            SqlDataTypes.Decimal or SqlDataTypes.Numeric or SqlDataTypes.Money or SqlDataTypes.SmallMoney => "decimal",
            SqlDataTypes.Real => "float",
            SqlDataTypes.Float => "double",
            SqlDataTypes.Bit => "bool",
            SqlDataTypes.Date or SqlDataTypes.DateTime or SqlDataTypes.DateTime2 or SqlDataTypes.SmallDateTime => "global::System.DateTime",
            SqlDataTypes.Time => "global::System.TimeSpan",
            SqlDataTypes.DateTimeOffset => "global::System.DateTimeOffset",
            SqlDataTypes.Char or SqlDataTypes.VarChar or SqlDataTypes.Text or SqlDataTypes.NChar or SqlDataTypes.NVarCharOrSysName or SqlDataTypes.NText or SqlDataTypes.Xml => "string",
            SqlDataTypes.Binary or SqlDataTypes.VarBinary or SqlDataTypes.Image or SqlDataTypes.TimeStamp => "byte[]",
            SqlDataTypes.UniqueIdentifier => "global::System.Guid",
            SqlDataTypes.SqlVariant => "object",
            _ => throw new NotSupportedException($"SQL type ID '{column.TypeId}' is not supported for entity generation.")
        };
        return column.IsNullable ? name + "?" : name;
    }

    private static bool IsReferenceType(string typeName) => typeName is "string" or "byte[]" or "object";

    private static string GetSqlType(SqlColumn column)
    {
        SqlDataTypes type = (SqlDataTypes)column.TypeId;
        return type switch
        {
            SqlDataTypes.BigInt => "BIGINT",
            SqlDataTypes.Int => "INT",
            SqlDataTypes.SmallInt => "SMALLINT",
            SqlDataTypes.TinyInt => "TINYINT",
            SqlDataTypes.Decimal => DecimalType("DECIMAL", column),
            SqlDataTypes.Numeric => DecimalType("NUMERIC", column),
            SqlDataTypes.Money => "MONEY",
            SqlDataTypes.SmallMoney => "SMALLMONEY",
            SqlDataTypes.Real => "REAL",
            SqlDataTypes.Float => FloatType(column),
            SqlDataTypes.Bit => "BIT",
            SqlDataTypes.Date => "DATE",
            SqlDataTypes.DateTime => "DATETIME",
            SqlDataTypes.DateTime2 => "DATETIME2(" + ValidateScale(column.Scale) + ")",
            SqlDataTypes.SmallDateTime => "SMALLDATETIME",
            SqlDataTypes.Time => "TIME(" + ValidateScale(column.Scale) + ")",
            SqlDataTypes.DateTimeOffset => "DATETIMEOFFSET(" + ValidateScale(column.Scale) + ")",
            SqlDataTypes.Char => "CHAR(" + GetLength(column.MaxLength, false, false) + ")",
            SqlDataTypes.VarChar => "VARCHAR(" + GetLength(column.MaxLength, false, true) + ")",
            SqlDataTypes.Text => "VARCHAR(MAX)",
            SqlDataTypes.NChar => "NCHAR(" + GetLength(column.MaxLength, true, false) + ")",
            SqlDataTypes.NVarCharOrSysName => "NVARCHAR(" + GetLength(column.MaxLength, true, true) + ")",
            SqlDataTypes.NText => "NVARCHAR(MAX)",
            SqlDataTypes.Binary => "BINARY(" + GetLength(column.MaxLength, false, false) + ")",
            SqlDataTypes.VarBinary => "VARBINARY(" + GetLength(column.MaxLength, false, true) + ")",
            SqlDataTypes.Image => "VARBINARY(MAX)",
            SqlDataTypes.Xml => "XML",
            SqlDataTypes.SqlVariant => "SQL_VARIANT",
            SqlDataTypes.UniqueIdentifier => "UNIQUEIDENTIFIER",
            _ => throw new NotSupportedException($"SQL type ID '{column.TypeId}' cannot be used as a generated stored procedure parameter.")
        };
    }

    private static string FloatType(SqlColumn column)
    {
        if (column.Precision is < 1 or > 53)
            throw new ArgumentException($"The precision for column '{column.ColumnName}' is invalid.", nameof(column));
        return "FLOAT(" + column.Precision.ToString(CultureInfo.InvariantCulture) + ")";
    }

    private static string DecimalType(string name, SqlColumn column)
    {
        if (column.Precision is < 1 or > 38 || column.Scale > column.Precision)
            throw new ArgumentException($"The precision and scale for column '{column.ColumnName}' are invalid.", nameof(column));
        return name + "(" + column.Precision.ToString(CultureInfo.InvariantCulture) + "," + column.Scale.ToString(CultureInfo.InvariantCulture) + ")";
    }

    private static int ValidateScale(byte scale)
    {
        if (scale > 7)
            throw new ArgumentException("SQL time and date-time scale must be between 0 and 7.", nameof(scale));
        return scale;
    }

    private static string GetLength(short maxLength, bool unicode, bool allowMax)
    {
        if (maxLength == -1)
        {
            if (allowMax)
                return "MAX";
            throw new ArgumentException("SQL Server does not support MAX for fixed-width parameter types.", nameof(maxLength));
        }
        if (maxLength <= 0 || (unicode && maxLength % 2 != 0))
            throw new ArgumentException("The SQL column length is invalid or does not contain a whole number of Unicode characters.", nameof(maxLength));
        int length = unicode ? maxLength / 2 : maxLength;
        int maximumLength = unicode ? 4000 : 8000;
        if (length > maximumLength)
            throw new ArgumentException($"The SQL column length exceeds the supported maximum of {maximumLength} characters.", nameof(maxLength));
        return length.ToString(CultureInfo.InvariantCulture);
    }

    private static SqlColumn FindColumn(IReadOnlyCollection<SqlColumn> columns, string name) =>
        columns.FirstOrDefault(column => string.Equals(column.ColumnName, name, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException($"The configured column '{name}' does not exist in the table schema.", nameof(name));

    private static bool IsSingleColumnUniqueConstraint(SqlTable table, SqlColumn column)
    {
        foreach (SqlIndex index in table.Indexes.Where(index => index.IsUnique && (index.IsPrimaryKey || index.IsUniqueConstraint)))
        {
            if ((!string.IsNullOrEmpty(index.TableName) && !string.Equals(index.TableName, table.TableName, StringComparison.Ordinal)) ||
                (table.TableObjectId > 0 && index.TableObjectId > 0 && table.TableObjectId != index.TableObjectId))
                continue;

            SqlIndexColumn[] keyColumns = (index.Columns ?? new SqlIndexColumnCollection())
                .Where(indexColumn => !indexColumn.IsIncluded && indexColumn.KeyOrdinal > 0)
                .ToArray();
            if (keyColumns.Length != 1)
                continue;

            SqlIndexColumn key = keyColumns[0];
            if (ReferenceEquals(key.TableColumn, column) || (column.ColumnId > 0 && key.TableColumnId == column.ColumnId))
                return true;
        }
        return false;
    }

    private static (string Schema, string Table) ValidateProfileTableMatch(SqlDataProjectTableProfile profile, SqlTable table)
    {
        string tableName = RequireName(table.TableName, nameof(table.TableName));
        string schemaName = RequireName(table.Schema, nameof(table.Schema));
        string profileTableName = RequireName(profile.TableName, nameof(profile.TableName));
        (string? profileSchema, string profileName) = ParseTableIdentifier(profileTableName, nameof(profile.TableName));
        if (!string.Equals(profileName, tableName, StringComparison.Ordinal))
            throw new ArgumentException("The table profile name does not match the supplied schema.", nameof(profile));
        if (profileSchema is not null && !string.Equals(profileSchema, schemaName, StringComparison.Ordinal))
            throw new ArgumentException("The table profile schema does not match the supplied SQL table schema.", nameof(profile));

        if (!string.IsNullOrWhiteSpace(profile.QualifiedName))
        {
            (string? qualifiedSchema, string qualifiedTable) = ParseTableIdentifier(profile.QualifiedName, nameof(profile.QualifiedName));
            if (!string.Equals(qualifiedTable, tableName, StringComparison.Ordinal))
                throw new ArgumentException("The profile qualified name does not match the supplied SQL table name.", nameof(profile));
            if (qualifiedSchema is not null && !string.Equals(qualifiedSchema, schemaName, StringComparison.Ordinal))
                throw new ArgumentException("The profile qualified schema does not match the supplied SQL table schema.", nameof(profile));
        }
        return (schemaName, tableName);
    }

    private static (string? Schema, string Table) ParseTableIdentifier(string value, string parameterName)
    {
        var parts = new List<string>(2);
        int position = 0;
        while (position < value.Length)
        {
            while (position < value.Length && char.IsWhiteSpace(value[position]))
                position++;
            if (position == value.Length)
                break;

            var part = new StringBuilder();
            if (value[position] == '[')
            {
                position++;
                bool closed = false;
                while (position < value.Length)
                {
                    char current = value[position++];
                    if (current == ']')
                    {
                        if (position < value.Length && value[position] == ']')
                        {
                            part.Append(']');
                            position++;
                        }
                        else
                        {
                            closed = true;
                            break;
                        }
                    }
                    else
                    {
                        part.Append(current);
                    }
                }
                if (!closed)
                    throw new ArgumentException($"'{value}' is not a valid table identifier.", parameterName);
                while (position < value.Length && char.IsWhiteSpace(value[position]))
                    position++;
                if (position < value.Length && value[position] != '.')
                    throw new ArgumentException($"'{value}' is not a valid table identifier.", parameterName);
            }
            else
            {
                while (position < value.Length && value[position] != '.')
                    part.Append(value[position++]);
                string unquoted = part.ToString().Trim();
                part.Clear().Append(unquoted);
            }

            if (part.Length == 0)
                throw new ArgumentException($"'{value}' is not a valid table identifier.", parameterName);
            parts.Add(part.ToString());
            if (parts.Count > 2)
                throw new ArgumentException($"'{value}' must contain at most a schema and table name.", parameterName);
            if (position < value.Length)
            {
                position++;
                if (position == value.Length)
                    throw new ArgumentException($"'{value}' is not a valid table identifier.", parameterName);
            }
        }

        if (parts.Count == 1)
            return (null, parts[0]);
        if (parts.Count == 2)
            return (parts[0], parts[1]);
        throw new ArgumentException($"'{value}' is not a valid table identifier.", parameterName);
    }

    private static Dictionary<SqlColumn, string> CreateParameterNames(IReadOnlyList<SqlColumn> columns)
    {
        var names = new Dictionary<SqlColumn, string>();
        var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < columns.Count; index++)
        {
            string columnName = columns[index].ColumnName!;
            bool valid = columnName.Length <= 127 && (char.IsLetter(columnName[0]) || columnName[0] == '_') &&
                columnName.All(character => char.IsLetterOrDigit(character) || character == '_');
            string parameter = "@" + (valid ? columnName : "p" + (index + 1).ToString(CultureInfo.InvariantCulture));
            if (!usedNames.Add(parameter))
            {
                int suffix = 0;
                do
                {
                    parameter = "@p_" + (index + 1).ToString(CultureInfo.InvariantCulture) + "_" + (++suffix).ToString(CultureInfo.InvariantCulture);
                }
                while (!usedNames.Add(parameter));
            }
            names.Add(columns[index], parameter);
        }
        return names;
    }

    private static string MakePropertyIdentifier(string name)
    {
        var output = new StringBuilder(name.Length);
        bool capitalizeNext = false;
        foreach (char character in name)
        {
            if (char.IsLetterOrDigit(character) || character == '_')
            {
                if (output.Length == 0 && char.IsDigit(character))
                    output.Append('_');
                output.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);
                capitalizeNext = false;
            }
            else
            {
                capitalizeNext = output.Length > 0;
            }
        }
        if (output.Length == 0)
            throw new ArgumentException($"SQL column name '{name}' cannot be converted to a C# property name.", nameof(name));
        return output.ToString();
    }

    private static string RequireIdentifier(string? identifier, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("An explicit C# identifier is required.", parameterName);
        string value = identifier.Trim();
        if (value.StartsWith('@'))
            value = value[1..];
        if (!IsIdentifier(value))
            throw new ArgumentException($"'{identifier}' is not a valid C# identifier.", parameterName);
        return value;
    }

    private static string? FormatNamespace(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        string[] parts = value.Split('.');
        if (parts.Any(part => !IsIdentifier(part.StartsWith('@') ? part[1..] : part)))
            throw new ArgumentException($"'{value}' is not a valid C# namespace.", parameterName);
        return string.Join('.', parts.Select(part => EscapeIdentifier(part.StartsWith('@') ? part[1..] : part)));
    }

    private static bool IsIdentifier(string value) => value.Length > 0 &&
        (char.IsLetter(value[0]) || value[0] == '_') && value.Skip(1).All(character => char.IsLetterOrDigit(character) || character == '_');

    private static string EscapeIdentifier(string value) => CSharpKeywords.Contains(value) ? "@" + value : value;

    private static string EscapeCSharpString(string value)
    {
        var output = new StringBuilder(value.Length);
        foreach (char character in value)
        {
            output.Append(character switch
            {
                '\\' => "\\\\",
                '"' => "\\\"",
                '\0' => "\\0",
                '\a' => "\\a",
                '\b' => "\\b",
                '\f' => "\\f",
                '\n' => "\\n",
                '\r' => "\\r",
                '\t' => "\\t",
                '\v' => "\\v",
                _ when char.IsControl(character) => "\\u" + ((int)character).ToString("X4", CultureInfo.InvariantCulture),
                _ => character.ToString()
            });
        }
        return output.ToString();
    }

    private static string RequireName(string? value, string parameterName) => !string.IsNullOrWhiteSpace(value)
        ? value.Trim()
        : throw new ArgumentException("A non-empty name is required.", parameterName);

    private static string QuoteIdentifier(string value) => "[" + value.Replace("]", "]]", StringComparison.Ordinal) + "]";
}
