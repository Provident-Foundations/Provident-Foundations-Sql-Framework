using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Provident.Foundations.Sql.Abstractions;
using Provident.Foundations.Sql.Collections;
using Provident.Foundations.Sql.Schema.DOM;
using Provident.Foundations.Abstractions;
using Provident.Foundations.Common;
using System.Data;

namespace Provident.Foundations.Sql.Provider;

#pragma warning disable S6966 // Await-able method should be used

/// <summary>
/// Implements the data provider for SQL Server databases.
/// </summary>
/// <remarks>
/// This class encapsulates all the necessary SQL Server data access functions
/// for reading data or executing SQL Statements or stored procedures.
/// </remarks>
public class SqlDataProvider : ExceptionTrackingBase, ISqlServerDataProvider
{
    #region Private Member Declarations
    /// <summary>
    /// The SQL query to get database names.
    /// </summary>
    private const string SqlGetDbNamesQuery = "SELECT [sys].[databases].[name] " +
        "FROM [sys].[databases] " +
        "WHERE LEN([sys].[databases].[owner_sid]) > 1 AND [sys].[databases].[owner_sid] != 1 AND [sys].[databases].[state_desc] = 'ONLINE' " +
        "ORDER BY[sys].[databases].[name]";

    /// <summary>
    /// The connection string builder containing the connection string to use.
    /// </summary>
    private SqlConnectionStringBuilder? _connectionString;
    #endregion

    #region Constructor / Dispose Methods		
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlDataProvider"/> class.
    /// </summary>
    /// <remarks>
    /// This is the default constructor.
    /// </remarks>
    public SqlDataProvider()
    {
        _connectionString = new SqlConnectionStringBuilder();
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlDataProvider"/> class.
    /// </summary>
    /// <param name="connectionString">
    /// The connection string value.
    /// </param>
    public SqlDataProvider(string connectionString)
    {
        _connectionString = new SqlConnectionStringBuilder(connectionString);
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlDataProvider"/> class.
    /// </summary>
    /// <param name="connectionString">
    /// The <see cref="SqlConnectionStringBuilder"/> containing the connection string value to use.
    /// </param>
    public SqlDataProvider(SqlConnectionStringBuilder connectionString)
    {
        _connectionString = new SqlConnectionStringBuilder(connectionString.ToString());
    }
    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (!IsDisposed && disposing)
        {
            _connectionString?.Clear();
        }

        _connectionString = null;
        base.Dispose(disposing);
    }
    #endregion

    #region Public Properties
    /// <summary>
    /// Gets the connection string value.
    /// </summary>
    /// <value>
    /// A string containing the connection data.
    /// </value>
    public string? ConnectionString
    {
        get
        {
            if (_connectionString == null)
            {
                return null;
            }
            else
            {
                return _connectionString.ToString();
            }
        }
        set => _connectionString = new SqlConnectionStringBuilder(value);
    }
    #endregion

    #region Public Methods / Functions

    #region SQL Connection Object Methods
    /// <summary>
    /// Creates a disconnected version of the data source connection object to use.
    /// </summary>
    /// <returns>
    /// A disconnected <see cref="SqlConnection"/> instance, or <b>null</b> on failure.
    /// </returns>
    public SqlConnection? CreateConnectionInstance()
    {
        SqlConnection? newConnection;
        try
        {
            newConnection = new SqlConnection(ConnectionString);
        }
        catch (Exception ex)
        {
            newConnection = null;
            AddException(ex);
        }

        return newConnection;
    }
    /// <summary>
    /// Attempts to connect the specified connection object to the
    /// data source.
    /// </summary>
    /// <param name="connection">
    /// The <see cref="SqlConnection"/> instance to use.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="SqlConnection"/> containing the result of the operation.
    /// </returns>
    public virtual IOperationResult<SqlConnection> ConnectInstance(SqlConnection? connection)
    {
        IOperationResult<SqlConnection> result;

        if (connection == null)
        {
            result = new OperationResult<SqlConnection>(false);
        }
        else
        {
            try
            {
                connection.Open();
                result = new OperationResult<SqlConnection>(true);
            }
            catch (Exception ex)
            {
                connection.Dispose();
                result = new OperationResult<SqlConnection>(ex);
            }
        }

        // Ensure the connection is referenced.
        result.DataContent = connection;

        return result;
    }
    /// <summary>
    /// Attempts to connect the specified connection object to the
    /// data source.
    /// </summary>
    /// <param name="connection">
    /// The <see cref="SqlConnection"/> instance to use.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="SqlConnection"/> containing the result of the operation.
    /// </returns>
    public virtual async Task<IOperationResult<SqlConnection>> ConnectInstanceAsync(SqlConnection? connection)
    {
        IOperationResult<SqlConnection> result;

        if (connection == null)
        {
            result = new OperationResult<SqlConnection>(false);
        }
        else
        {
            try
            {
                await connection.OpenAsync().ConfigureAwait(false);
                result = new OperationResult<SqlConnection>(true);
            }
            catch (Exception ex)
            {
                await connection.DisposeAsync().ConfigureAwait(false);
                result = new OperationResult<SqlConnection>(ex);
            }
        }

        // Ensure the connection is referenced.
        result.DataContent = connection;

        return result;
    }
    /// <summary>
    /// Attempts to create a connected version of the SQL connection object to use.
    /// </summary>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the connected
    /// <see cref="SqlConnection"/> instance to use or the exception
    /// if the operation fails.
    /// </returns>
    public IOperationResult<SqlConnection> CreateAndOpenConnection()
    {
        IOperationResult<SqlConnection>? result;

        // Try to create the object instance.
        SqlConnection? connection = CreateConnectionInstance();
        if (connection != null && connection.State == ConnectionState.Open)
        {
            result = new OperationResult<SqlConnection>(connection);
        }

        else if (connection != null)
        {
            // Attempt to establish the connection.
            result = ConnectInstance(connection);
        }
        else
        {
            result = new OperationResult<SqlConnection>(false);
        }
        return result;
    }
    /// <summary>
    /// Attempts to create a connected version of the SQL connection object to use.
    /// </summary>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the connected
    /// <see cref="SqlConnection"/> instance to use or the exception
    /// if the operation fails.
    /// </returns>
    public virtual async Task<IOperationResult<SqlConnection>> CreateAndOpenConnectionAsync()
    {
        IOperationResult<SqlConnection>? result;

        // Try to create the object instance.
        SqlConnection? connection = CreateConnectionInstance();
        if (connection != null)
        {
            // Attempt to establish the connection.
            result = await ConnectInstanceAsync(connection).ConfigureAwait(false);
        }
        else
        {
            result = new OperationResult<SqlConnection>(false);
        }
        return result;
    }
    #endregion

    #region SQL Command Object Methods
    /// <summary>
    /// Creates the SQL command instance for the provided query,
    /// </summary>
    /// <param name="query">
    /// A string containing the SQL query to be executed.
    /// </param>
    /// <returns>
    /// A <see cref="IOperationResult{T}"/> containing the <see cref="SqlCommand"/>
    /// instance.
    /// </returns>
    public IOperationResult<SqlCommand> CreateCommandInstance(string query)
    {
        OperationResult<SqlCommand> result;

        try
        {
            SqlCommand command = new SqlCommand(query);
            result = new OperationResult<SqlCommand>(command);
        }
        catch (Exception ex)
        {
            result = new OperationResult<SqlCommand>(ex);
        }

        return result;
    }
    /// <summary>
    /// Creates the SQL command instance.
    /// </summary>
    /// <param name="query">
    /// A string containing the SQL query to be executed.
    /// </param>
    /// <param name="parameterList">
    /// An <see cref="IEnumerable{T}"/> of <see cref="SqlParameter"/> instances
    /// to add to the <see cref="SqlCommand"/>.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the <see cref="SqlCommand"/>
    /// instance if successful; otherwise, contains the error message.
    /// </returns>
    public IOperationResult<SqlCommand> CreateCommandInstance(string query, IEnumerable<SqlParameter> parameterList)
    {
        IOperationResult<SqlCommand> result = new OperationResult<SqlCommand>();

        SqlCommand? command;
        try
        {
            command = new SqlCommand(query);
            result.DataContent = command;
            result.Success = true;
        }
        catch (Exception ex)
        {
            command = null;
            result.AddException(ex);
        }

        if (command != null)
        {
            try
            {
                command.Parameters.AddRange(parameterList.ToArray());
            }
            catch (Exception ex)
            {
                command.Dispose();
                result.Success = false;
                result.DataContent = null;
                result.AddException(ex);
            }
        }
        return result;
    }
    /// <summary>
    /// Creates the command instance.
    /// </summary>
    /// <param name="query">
    /// A string containing the SQL query to be executed.
    /// </param>
    /// <param name="createConnection">
    /// <b>true</b> to create the SQL connection instance.
    /// </param>
    /// <param name="connectionString">The connection string.</param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the <see cref="SqlCommand"/> if successful;
    /// otherwise, contains the error message.
    /// </returns>
    public async Task<IOperationResult<SqlCommand>> CreateCommandInstanceAsync(string query, bool createConnection, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(query);
        OperationResult<SqlCommand> result;

        if (createConnection)
        {
            ArgumentNullException.ThrowIfNull(connectionString);
            IOperationResult<SqlConnection> connectionResult;
            try
            {
                SqlConnection connection = new(connectionString);
                connectionResult = await ConnectInstanceAsync(connection).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                result = new OperationResult<SqlCommand>(ex);
                return result;
            }

            if (!connectionResult.Success)
            {
                // Store the error and exit.
                result = new OperationResult<SqlCommand>(false);
                result.AddExceptions(connectionResult.Exceptions);
            }
            else
            {
                // Create the command object using the new SQL Server connection.
                SqlConnection connection = connectionResult.DataContent!;
                try
                {
                    SqlCommand command = new SqlCommand(query);
                    command.Connection = connection;
                    result = new OperationResult<SqlCommand>(command);
                }
                catch (Exception ex)
                {
                    result = new OperationResult<SqlCommand>(ex);

                    // Dispose and Close if the operation somehow fails.
                    await connection.DisposeAsync().ConfigureAwait(false);
                    await connection.CloseAsync().ConfigureAwait(false);
                }
            }
        }
        else
        {
            result = (OperationResult<SqlCommand>)CreateCommandInstance(query);
        }

        return result;
    }
    /// <summary>
    /// Creates the SQL command instance for the provided query to execute and the provided SQL connection instance.
    /// </summary>
    /// <param name="query">
    /// A string containing the SQL query to be executed.
    /// </param>
    /// <param name="connection">
    /// The <see cref="SqlConnection"/> instance to attach to the command.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{SqlCommand}"/> The constructed <see cref="SqlCommand" /> instance with the
    /// maximum command timeout value, if successful; otherwise, contains the error message.
    /// </returns>
    public IOperationResult<SqlCommand> CreateCommandInstance(string? query, SqlConnection? connection)
    {
        OperationResult<SqlCommand> result = new OperationResult<SqlCommand>();

        if (!string.IsNullOrEmpty(query) && connection != null)
        {
            SqlCommand? command = null;
            try
            {
                command = new SqlCommand(query, connection) { CommandTimeout = int.MaxValue };
                result.DataContent = command;
                result.Success = true;
            }
            catch (Exception ex)
            {
                command?.Dispose();
                result.Success = false;
                result.AddException(ex);
            }
        }
        else
        {
            result.Success = false;
        }

        return result;
    }
    /// <summary>
    /// Executes the specified batch of statements within a transaction.
    /// </summary>
    /// <remarks>
    /// If all the statements are executed successfully, the transaction is committed;
    /// otherwise, the transactions are rolled back.
    /// </remarks>
    /// <param name="sqlStatements">
    /// An array of strings containing the SQL statements to be executed.
    /// </param>
    /// <returns>
    /// A <see cref="IOperationResult{T}"/> containing the number of rows affected, if successful;
    /// otherwise, contains the error message.
    /// </returns>
    public IOperationResult<int> ExecuteBatch(IEnumerable<string> sqlStatements)
    {
        IOperationResult<int> result = new OperationResult<int>(false);
        int totalRowsAffected = 0;

        // Do nothing if no queries were provided.
        string[] statements = sqlStatements.ToArray();
        if (statements.Length > 0)
        {
            // Attempt to connect.
            IOperationResult<SqlConnection> connectResult = CreateAndOpenConnection();
            if (connectResult.Success)
            {
                // Create the transaction to execute the statements in.
                SqlConnection connection = connectResult.DataContent!;
                SqlTransaction? transaction = CreateTransactionInstance(connection, result);
                if (transaction != null)
                {
                    // Create the generic SQL command object to use for all the operations.
                    SqlCommand? command = CreateEmptySqlCommandInstance(connection, result);
                    if (command != null)
                    {
                        // Execute each statement in sequence.
                        bool failed = false;
                        int length = statements.Length;
                        int count = 0;
                        do
                        {
                            command.CommandText = statements[count];
                            try
                            {
                                int rowsAffected = command.ExecuteNonQuery();
                                totalRowsAffected += rowsAffected;
                            }
                            catch (Exception ex)
                            {
                                // Time to roll back.
                                failed = true;
                                result.AddException(ex);
                                result.Success = false;
                            }

                            count++;
                        } while (count < length && !failed);

                        // Dispose.
                        Array.Clear(statements);
                        command.Dispose();

                        // If an operation failed, rollback the transaction, otherwise,
                        // commit.
                        if (failed)
                        {
                            transaction.Rollback();
                        }
                        else
                        {
                            result.Success = true;
                            result.DataContent = totalRowsAffected;
                            transaction.Commit();
                        }
                    }

                    // Dispose.
                    transaction.Dispose();
                }

                // Close the connection.
                connection.Close();
                connection.Dispose();
            }
            else
            {
                result.Success = false;
                result.AddExceptions(connectResult.Exceptions);
            }

            // Dispose.
            connectResult.Dispose();
        }
        return result;
    }
    /// <summary>
    /// Executes the specified batch of statements within a transaction.
    /// </summary>
    /// <remarks>
    /// If all the statements are executed successfully, the transaction is committed;
    /// otherwise, the transactions are rolled back.
    /// </remarks>
    /// <param name="sqlStatements">
    /// An array of strings containing the SQL statements to be executed.
    /// </param>
    /// <returns>
    /// A <see cref="IOperationResult{T}"/> containing the number of rows affected, if successful;
    /// otherwise, contains the error message.
    /// </returns>
    public async Task<IOperationResult<int>> ExecuteBatchAsync(IEnumerable<string>? sqlStatements)
    {
        IOperationResult<int> result = new OperationResult<int>(false);
        int totalRowsAffected = 0;

        if (sqlStatements != null)
        {
            // Do nothing if no queries were provided.
            string[] statements = sqlStatements.ToArray();
            if (statements.Length > 0)
            {
                // Attempt to connect.
                IOperationResult<SqlConnection> connectResult = await CreateAndOpenConnectionAsync().ConfigureAwait(false);
                if (connectResult.Success)
                {
                    // Create the transaction to execute the statements in.
                    SqlConnection connection = connectResult.DataContent!;
                    SqlTransaction? transaction = CreateTransactionInstance(connection, result);
                    if (transaction != null)
                    {
                        // Create the generic SQL command object to use for all the operations.
                        SqlCommand? command = CreateEmptySqlCommandInstance(connection, result);
                        if (command != null)
                        {
                            // Execute each statement in sequence.
                            bool failed = false;
                            int length = statements.Length;
                            int count = 0;
                            do
                            {
                                command.CommandText = statements[count];
                                try
                                {
                                    int rowsAffected = await command.ExecuteNonQueryAsync();
                                    totalRowsAffected += rowsAffected;
                                }
                                catch (Exception ex)
                                {
                                    // Time to roll back.
                                    failed = true;
                                    result.AddException(ex);
                                    result.Success = false;
                                }

                                count++;
                            } while (count < length && !failed);

                            // Dispose.
                            Array.Clear(statements);
                            command.Dispose();

                            // If an operation failed, rollback the transaction, otherwise,
                            // commit.
                            if (failed)
                            {
                                transaction.Rollback();
                            }
                            else
                            {
                                result.Success = true;
                                result.DataContent = totalRowsAffected;
                                transaction.Commit();
                            }
                        }

                        // Dispose.
                        transaction.Dispose();
                    }

                    // Close the connection.
                    connection.Close();
                    connection.Dispose();
                }
                else
                {
                    result.Success = false;
                    result.AddExceptions(connectResult.Exceptions);
                }

                // Dispose.
                connectResult.Dispose();
            }
        }
        return result;
    }
    /// <summary>
    /// Attempts to execute the list of SQL statements as a batch operation.
    /// </summary>
    /// <remarks>
    /// Implementations of this method should wrap these executions within a transaction.
    /// </remarks>
    /// <param name="commands">
    /// An <see cref="IEnumerable{T}"/> list of <see cref="SqlCommand"/> instances containing the SQL Statements to be executed.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="int"/> containing the result of the operation.
    /// If successful, the result returns the number of rows affected.
    /// </returns>
    public IOperationResult<int> ExecuteBatch(IEnumerable<SqlCommand>? commands)
    {
        IOperationResult<int> result = new OperationResult<int>(false);
        int totalRowsAffected = 0;

        // Do nothing if no queries were provided.
        if (commands != null)
        {
            // Attempt to connect.
            IOperationResult<SqlConnection> connectResult = CreateAndOpenConnection();
            if (connectResult.Success)
            {
                // Create the transaction to execute the statements in.
                SqlConnection connection = connectResult.DataContent!;
                SqlTransaction? transaction = CreateTransactionInstance(connection, result);
                if (transaction != null)
                {
                    bool failed = false;
                    foreach (SqlCommand command in commands)
                    {
                        command.Connection = connection;
                        command.Transaction = transaction;

                        try
                        {
                            int rowsAffected = command.ExecuteNonQuery();
                            totalRowsAffected += rowsAffected;
                        }
                        catch (Exception ex)
                        {
                            // Time to roll back.
                            failed = true;
                            result.AddException(ex);
                            result.Success = false;
                        }
                        if (failed)
                        {
                            break;
                        }
                    }

                    // If an operation failed, rollback the transaction, otherwise,
                    // commit.
                    if (failed)
                    {
                        transaction.Rollback();
                    }
                    else
                    {
                        result.Success = true;
                        result.DataContent = totalRowsAffected;
                        transaction.Commit();
                    }

                    // Dispose.
                    transaction.Dispose();
                }

                // Close the connection.
                connection.Close();
                connection.Dispose();
            }
            else
            {
                result.Success = false;
                result.AddExceptions(connectResult.Exceptions);
            }

            // Dispose.
            connectResult.Dispose();
        }

        return result;
    }
    /// <summary>
    /// Attempts to execute the list of SQL statements as a batch operation.
    /// </summary>
    /// <remarks>
    /// Implementations of this method should wrap these executions within a transaction.
    /// </remarks>
    /// <param name="commands">
    /// An <see cref="IEnumerable{T}"/> list of <see cref="SqlCommand"/> instances containing the SQL Statements to be executed.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="int"/> containing the result of the operation.
    /// If successful, the result returns the number of rows affected.
    /// </returns>
    public async Task<IOperationResult<int>> ExecuteBatchAsync(IEnumerable<SqlCommand>? commands)
    {
        IOperationResult<int> result = new OperationResult<int>(false);
        int totalRowsAffected = 0;

        // Do nothing if no queries were provided.
        if (commands != null)
        {
            // Attempt to connect.
            IOperationResult<SqlConnection> connectResult = await CreateAndOpenConnectionAsync().ConfigureAwait(false);
            if (connectResult.Success)
            {
                // Create the transaction to execute the statements in.
                SqlConnection connection = connectResult.DataContent!;
                SqlTransaction? transaction = CreateTransactionInstance(connection, result);
                if (transaction != null)
                {
                    bool failed = false;
                    foreach (SqlCommand command in commands)
                    {
                        command.Connection = connection;
                        command.Transaction = transaction;

                        try
                        {
                            int rowsAffected = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                            totalRowsAffected += rowsAffected;
                        }
                        catch (Exception ex)
                        {
                            // Time to roll back.
                            failed = true;
                            result.AddException(ex);
                            result.Success = false;
                        }
                        if (failed)
                        {
                            break;
                        }
                    }

                    // If an operation failed, rollback the transaction, otherwise,
                    // commit.
                    if (failed)
                    {
                        await transaction.RollbackAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        result.Success = true;
                        result.DataContent = totalRowsAffected;
                        transaction.Commit();
                    }

                    // Dispose.
                    transaction.Dispose();
                }

                // Close the connection.
                connection.Close();
                connection.Dispose();
            }
            else
            {
                result.Success = false;
                result.AddExceptions(connectResult.Exceptions);
            }

            // Dispose.
            connectResult.Dispose();
        }

        return result;
    }

    #endregion

    #region SQL Adapter Object Methods
    /// <summary>
    /// Creates the adapter instance.
    /// </summary>
    /// <returns>
    /// The <see cref="SqlDataAdapter"/> instance.
    /// </returns>
    public IOperationResult<SqlDataAdapter> CreateAdapterInstance(SqlCommand? command)
    {
        OperationResult<SqlDataAdapter> result;
        if (command != null)
        {
            try
            {
                SqlDataAdapter adapter = new SqlDataAdapter(command);
                result = new OperationResult<SqlDataAdapter>(adapter);
            }
            catch (Exception ex)
            {
                result = new OperationResult<SqlDataAdapter>(false);
                result.AddException(ex);
            }
        }
        else
        {
            result = new OperationResult<SqlDataAdapter>(false);
        }

        return result;
    }
    #endregion

    #region SQL Data Reader Object Methods
    /// <summary>
    /// Executes the supplied SQL Query and returns the resulting SQL data reader.
    /// </summary>
    /// <param name="command">
    /// The <see cref="SqlCommand"/> to be executed.
    /// </param>
    /// <returns>
    /// A <see cref="IOperationResult{T}"/> containing the <see cref="SqlDataReader"/>
    /// resulting from the query, or the <see cref="Exception"/> if the operation
    /// failed.
    /// </returns>
    public virtual async Task<IOperationResult<SqlDataReader>> ExecuteDataReaderAsync(SqlCommand? command)
    {
        OperationResult<SqlDataReader> result;

        if (command != null)
        {
            try
            {
                SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                result = new OperationResult<SqlDataReader>(reader);
            }
            catch (Exception ex)
            {
                result = new OperationResult<SqlDataReader>(ex);
            }
        }
        else
        {
            result = new OperationResult<SqlDataReader>(false);
        }

        return result;
    }
    /// <summary>
    /// Attempts to execute the supplied SQL statement.
    /// </summary>
    /// <param name="sql">
    /// A string containing the SQL statement to be executed.
    /// </param>
    /// <returns>
    /// A <see cref="SqlQueryErrorCollection"/> containing the list of SQL Errors, if present.
    /// If the query is successful, this function returns <b>null</b>.
    /// </returns>
    public async Task<SqlQueryErrorCollection?> ExecuteUserSqlAsync(string sql)
    {
        SqlQueryErrorCollection? errorList = null;
        if (!string.IsNullOrEmpty(sql))
        {
            IOperationResult<SqlConnection> result = await CreateAndOpenConnectionAsync().ConfigureAwait(false);
            if (result.Success && result.DataContent != null)
            {
                SqlConnection connection = result.DataContent;
                IOperationResult<SqlCommand> cmdResult = CreateCommandInstance(sql, connection);
                if (cmdResult.Success && cmdResult.DataContent != null)
                {
                    SqlCommand command = cmdResult.DataContent;

                    try
                    {
                        await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                    }
                    catch (SqlException sqlEx)
                    {
                        AddException(sqlEx);
                        errorList = new SqlQueryErrorCollection();
                        foreach (SqlError error in sqlEx.Errors)
                        {
                            SqlQueryError queryError = new SqlQueryError
                            {
                                Class = error.Class,
                                LineNumber = error.LineNumber,
                                Message = error.Message,
                                Number = error.Number,
                                Procedure = error.Procedure,
                                Server = error.Server,
                                Source = error.Source,
                                State = error.State
                            };
                            errorList.Add(queryError);
                        }
                    }
                    catch (Exception ex)
                    {
                        AddException(ex);
                    }

                    command.Dispose();
                }
                cmdResult.Dispose();
                connection.Close();
                connection.Dispose();
            }
            result.Dispose();
        }
        return errorList;
    }
    /// <summary>
    /// Executes the supplied SQL Query and returns the resulting content in a data table.
    /// </summary>
    /// <param name="command">
    /// The <see cref="SqlCommand"/> to be executed.
    /// </param>
    /// <returns>
    /// A <see cref="IOperationResult{T}"/> containing the <see cref="DataTable"/>
    /// resulting from the query, or the <see cref="Exception"/> if the operation
    /// failed.
    /// </returns>
    public IOperationResult<DataTable> FillDataTable(SqlCommand? command)
    {
        OperationResult<DataTable> result;

        if (command != null)
        {
            IOperationResult<SqlDataAdapter> adapterResult = CreateAdapterInstance(command);
            if (adapterResult.Success)
            {
                SqlDataAdapter adapter = adapterResult.DataContent!;
                DataTable table = new DataTable();
                try
                {
                    adapter.Fill(table);
                    result = new OperationResult<DataTable>(table);
                }
                catch (Exception ex)
                {
                    result = new OperationResult<DataTable>(ex);
                }
                adapter.Dispose();
            }
            else
            {
                result = new OperationResult<DataTable>();
                adapterResult.CopyTo(result);
                adapterResult.Dispose();
            }
        }
        else
        {
            result = new OperationResult<DataTable>(false);
        }

        return result;
    }
    /// <summary>
    /// Executes the supplied SQL Query and returns the resulting content in a data table.
    /// </summary>
    /// <param name="sqlQuery">
    /// The text of the SQL to be executed to read data.
    /// </param>
    /// <returns>
    /// A <see cref="IOperationResult{T}"/> containing the <see cref="DataTable"/>
    /// resulting from the query, or the <see cref="Exception"/> if the operation
    /// failed.
    /// </returns>
    public async Task<IOperationResult<DataTable>> FillDataTableAsync(string sqlQuery)
    {
        OperationResult<DataTable> result;

        IOperationResult<SqlConnection> connectResult = await CreateAndOpenConnectionAsync().ConfigureAwait(false);
        if (connectResult.Success)
        {
            SqlConnection? connection = connectResult.DataContent;
            SqlCommand command = new SqlCommand(sqlQuery, connection);

            IOperationResult<SqlDataAdapter> adapterResult = CreateAdapterInstance(command);
            if (adapterResult.Success)
            {
                SqlDataAdapter adapter = adapterResult.DataContent!;
                DataTable table = new DataTable();
                try
                {
                    adapter.Fill(table);
                    result = new OperationResult<DataTable>(table);
                }
                catch (Exception ex)
                {
                    result = new OperationResult<DataTable>(ex);
                }
                adapter.Dispose();
            }
            else
            {
                result = new OperationResult<DataTable>();
                adapterResult.CopyTo(result);
                adapterResult.Dispose();
            }
            command.Dispose();
            connection?.Close();
            connection?.Dispose();
        }
        else
        {
            result = new OperationResult<DataTable>(false);
        }

        connectResult.Dispose();
        return result;
    }
    #endregion

    #region SQL Query Execution Methods
    /// <summary>
    /// Executes the specified SQL command object.
    /// </summary>
    /// <param name="command">
    /// The <see cref="SqlCommand"/> instance to be executed.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="int"/> containing the result of the operation.
    /// If successful, the result returns the number of rows affected.
    /// </returns>
    public IOperationResult<int> ExecuteCommand(SqlCommand command)
    {
        OperationResult<int> result;
        try
        {
            int rowsAffected = command.ExecuteNonQuery();
            result = new OperationResult<int>(rowsAffected);
        }
        catch (Exception ex)
        {
            result = new OperationResult<int>(ex);
        }
        return result;
    }
    /// <summary>
    /// Executes the provided SQL Command instance and returns the result of the operation.
    /// </summary>
    /// <param name="command">
    /// A <see cref="SqlCommand"/> instance containing a non-SELECT SQL query to
    /// be executed.
    /// </param>
    /// <returns>
    /// A <see cref="IOperationResult{T}"/> of <see cref="int"/> containing the number of rows affected if
    /// successful; otherwise, contains the error message.
    /// </returns>
    public async Task<IOperationResult<int>> ExecuteCommandAsync(SqlCommand command)
    {
        OperationResult<int> result;
        try
        {
            int rowsAffected = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            result = new OperationResult<int>(rowsAffected);
        }
        catch (Exception ex)
        {
            result = new OperationResult<int>(ex);
        }
        return result;
    }
    /// <summary>
    /// Executes the list of SQL commands within the specified transaction.
    /// </summary>
    /// <param name="transaction">
    /// The <see cref="SqlTransaction"/> instance to be used.
    /// </param>
    /// <param name="commandList">
    /// The <see cref="IEnumerable{T}"/> list of <see cref="SqlCommand"/> instances to be executed.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult"/> containing the result of the operation.  If the operation fails,
    /// the transaction is rolled back.
    /// </returns>
    public IOperationResult ExecuteInTransaction(SqlTransaction transaction, IEnumerable<SqlCommand> commandList)
    {
        OperationResult? result = null;

        bool failed = false;
        foreach (SqlCommand command in commandList)
        {
            command.Transaction = transaction;
            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                result = new OperationResult(ex);
                failed = true;
            }
            if (failed)
            {
                break;
            }
        }

        if (failed)
        {
            if (result == null)
            {
                result = new OperationResult(false);
            }

            transaction.Rollback();
        }
        else
        {
            transaction.Commit();
            result = new OperationResult(true);
        }

        return result;
    }
    /// <summary>
    /// Executes the list of SQL commands within the specified transaction.
    /// </summary>
    /// <param name="transaction">
    /// The <see cref="SqlTransaction"/> instance to be used.
    /// </param>
    /// <param name="statementList">
    /// The <see cref="IEnumerable{T}"/> list of <see cref="string"/> containing the SQL Statements to be executed.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult"/> containing the result of the operation.  If the operation fails,
    /// the transaction is rolled back.
    /// </returns>
    public IOperationResult ExecuteInTransaction(SqlTransaction transaction, IEnumerable<string> statementList)
    {
        // Convert the string statements to SQL command objects.
        List<SqlCommand> commandList = new List<SqlCommand>();
        foreach (string statement in statementList)
        {
            commandList.Add(new SqlCommand(statement, transaction.Connection));
        }

        // Execute.
        IOperationResult result = ExecuteInTransaction(transaction, commandList);

        // Clear memory.
        foreach (SqlCommand command in commandList)
        {
            command.Dispose();
        }

        commandList.Clear();

        return result;
    }
    /// <summary>
    /// Executes the list of SQL commands within the specified transaction.
    /// </summary>
    /// <param name="transaction">
    /// The <see cref="SqlTransaction"/> instance to be used.
    /// </param>
    /// <param name="commandList">
    /// The <see cref="IEnumerable{T}"/> list of <see cref="SqlCommand"/> instances to be executed.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult"/> containing the result of the operation.  If the operation fails,
    /// the transaction is rolled back.
    /// </returns>
    public async Task<IOperationResult> ExecuteInTransactionAsync(SqlTransaction transaction, IEnumerable<SqlCommand> commandList)
    {
        OperationResult? result = null;

        bool failed = false;
        foreach (SqlCommand command in commandList)
        {
            command.Transaction = transaction;
            try
            {
                await command.ExecuteNonQueryAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                result = new OperationResult(ex);
                failed = true;
            }
            if (failed)
            {
                break;
            }
        }

        if (failed)
        {
            if (result == null)
            {
                result = new OperationResult(false);
            }

            await transaction.RollbackAsync().ConfigureAwait(false);
        }
        else
        {
            await transaction.CommitAsync().ConfigureAwait(false);
            result = new OperationResult(true);
        }

        return result;

    }
    /// <summary>
    /// Executes the list of SQL commands within the specified transaction.
    /// </summary>
    /// <param name="transaction">
    /// The <see cref="SqlTransaction"/> instance to be used.
    /// </param>
    /// <param name="statementList">
    /// The <see cref="IEnumerable{T}"/> list of <see cref="string"/> containing the SQL Statements to be executed.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult"/> containing the result of the operation.  If the operation fails,
    /// the transaction is rolled back.
    /// </returns>
    public async Task<IOperationResult> ExecuteInTransactionAsync(SqlTransaction transaction, IEnumerable<string> statementList)
    {
        // Convert the string statements to SQL command objects.
        List<SqlCommand> commandList = new List<SqlCommand>();
        foreach (string statement in statementList)
        {
            commandList.Add(new SqlCommand(statement, transaction.Connection));
        }

        // Execute.
        IOperationResult result = await ExecuteInTransactionAsync(transaction, commandList).ConfigureAwait(false);

        // Clear memory.
        foreach (SqlCommand command in commandList)
        {
            command.Dispose();
        }

        commandList.Clear();

        return result;
    }
    /// <summary>
    /// Attempts to execute the specified SQL command text with the supplied
    /// parameters.
    /// </summary>
    /// <param name="sqlText">
    /// A string containing the SQL to be executed.
    /// </param>
    /// <param name="parameterList">
    /// An <see cref="IEnumerable{T}"/> of <see cref="SqlParameter"/> instances to send to the command.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult"/> instance containing the result of the operation.
    /// </returns>
    public IOperationResult<int> ExecuteParameterizedSql(string? sqlText, IEnumerable<SqlParameter>? parameterList)
    {
        OperationResult<int> result = new OperationResult<int>();

        if (!string.IsNullOrEmpty(sqlText))
        {
            // Connect to SQL Server.
            IOperationResult<SqlConnection> connectResult = CreateAndOpenConnection();
            if (connectResult.Success)
            {
                // Create the command object.
                SqlConnection connection = connectResult.DataContent!;
                IOperationResult<SqlCommand> commandResult = CreateCommandInstance(sqlText, connection);
                if (commandResult.Success)
                {
                    SqlCommand command = commandResult.DataContent!;
                    command.CommandType = CommandType.Text;

                    // Add the parameters to the command, if present.
                    if (parameterList != null)
                    {
                        command.Parameters.AddRange(parameterList.ToArray());
                    }

                    // Execute.
                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        result.DataContent = rowsAffected;
                        result.Success = true;
                    }
                    catch (Exception ex)
                    {
                        AddException(ex);
                        result.AddException(ex);
                        result.Success = false;
                    }
                    command.Dispose();
                }
                else
                {
                    commandResult.CopyTo(result);
                }

                commandResult.Dispose();
                TryClose(connection);
                connection.Dispose();
            }
            else
            {
                connectResult.CopyTo(result);
            }

            connectResult.Dispose();
        }
        return result;
    }
    /// <summary>
    /// Attempts to execute the specified SQL command text with the supplied
    /// parameters.
    /// </summary>
    /// <param name="sqlText">
    /// A string containing the SQL to be executed.
    /// </param>
    /// <param name="parameterList">
    /// An <see cref="IEnumerable{T}"/> of <see cref="SqlParameter"/> instances to send to the command.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult"/> instance containing the result of the operation.
    /// </returns>
    public async Task<IOperationResult<int>> ExecuteParameterizedSqlAsync(string sqlText, IEnumerable<SqlParameter>? parameterList)
    {
        OperationResult<int> result = new OperationResult<int>();

        if (!string.IsNullOrEmpty(sqlText))
        {
            // Connect to SQL Server.
            IOperationResult<SqlConnection> connectResult = await CreateAndOpenConnectionAsync().ConfigureAwait(false);
            if (connectResult.Success)
            {
                // Create the command object.
                SqlConnection connection = connectResult.DataContent!;
                IOperationResult<SqlCommand> commandResult = CreateCommandInstance(sqlText, connection);
                if (commandResult.Success)
                {
                    SqlCommand command = commandResult.DataContent!;
                    command.CommandType = CommandType.Text;

                    // Add the parameters to the command, if present.
                    if (parameterList != null)
                    {
                        command.Parameters.AddRange(parameterList.ToArray());
                    }

                    // Execute.
                    try
                    {
                        int rowsAffected = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        result.DataContent = rowsAffected;
                        result.Success = true;
                    }
                    catch (Exception ex)
                    {
                        AddException(ex);
                        result.AddException(ex);
                        result.Success = false;
                    }
                    command.Dispose();
                }
                else
                {
                    commandResult.CopyTo(result);
                }

                commandResult.Dispose();
                TryClose(connection);
                connection.Dispose();
            }
            else
            {
                connectResult.CopyTo(result);
            }

            connectResult.Dispose();
        }
        return result;
    }
    /// <summary>
    /// Attempts to execute the supplied SQL statement.
    /// </summary>
    /// <param name="sql">
    /// A string containing the SQL statement to be executed.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="int"/> containing the result of the operation.
    /// If successful, the result contains an integer indicating the number of affected rows.
    /// </returns>
    public IOperationResult<int> ExecuteSql(string? sql)
    {
        OperationResult<int> result = new OperationResult<int>(false);

        if (!string.IsNullOrEmpty(sql))
        {
            // Connect to SQL Server.
            IOperationResult<SqlConnection> connectResult = CreateAndOpenConnection();
            if (connectResult.Success)
            {
                // Create the command instance.
                SqlConnection connection = connectResult.DataContent!;
                IOperationResult<SqlCommand> commandResult = CreateCommandInstance(sql, connection);
                if (commandResult.Success)
                {
                    SqlCommand command = commandResult.DataContent!;
                    try
                    {
                        int rowsAffected = command.ExecuteNonQuery();
                        result.Success = true;
                        result.DataContent = rowsAffected;
                    }
                    catch (Exception ex)
                    {
                        AddException(ex);
                        result.AddException(ex);
                    }
                    command.Dispose();
                }
                else
                {
                    commandResult.CopyTo(result);
                }

                commandResult.Dispose();
                TryClose(connection);
                connection.Dispose();
            }
            else
            {
                connectResult.CopyTo(result);
            }
        }
        return result;

    }
    /// <summary>
    /// Attempts to execute the supplied SQL statement.
    /// </summary>
    /// <param name="sql">
    /// A string containing the SQL statement to be executed.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="int"/> containing the result of the operation.
    /// If successful, the result contains an integer indicating the number of affected rows.
    /// </returns>
    public async Task<IOperationResult<int>> ExecuteSqlAsync(string sql)
    {
        OperationResult<int> result = new OperationResult<int>(false);

        if (!string.IsNullOrEmpty(sql))
        {
            // Connect to SQL Server.
            IOperationResult<SqlConnection> connectResult = await CreateAndOpenConnectionAsync().ConfigureAwait(false);
            if (connectResult.Success)
            {
                // Create the command instance.
                SqlConnection connection = connectResult.DataContent!;
                IOperationResult<SqlCommand> commandResult = CreateCommandInstance(sql, connection);
                if (commandResult.Success)
                {
                    SqlCommand command = commandResult.DataContent!;
                    try
                    {
                        int rowsAffected = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        result.Success = true;
                        result.DataContent = rowsAffected;
                    }
                    catch (Exception ex)
                    {
                        AddException(ex);
                        result.AddException(ex);
                    }
                    command.Dispose();
                }
                else
                {
                    commandResult.CopyTo(result);
                }

                commandResult.Dispose();
                TryClose(connection);
                connection.Dispose();
            }
            else
            {
                connectResult.CopyTo(result);
            }
        }
        return result;
    }
    /// <summary>
    /// Attempts to execute the specified stored procedure with the specified
    /// parameters.
    /// </summary>
    /// <param name="storedProcedure">
    /// A string containing the name of the SQL stored procedure to be executed.
    /// </param>
    /// <param name="parameterList">
    /// An <see cref="IEnumerable{T}"/> of <see cref="SqlParameter"/> instances to send to the command.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="int"/> containing the result of the operation.
    /// If successful, the result contains an integer indicating the number of affected rows.
    /// </returns>
    public IOperationResult<int> ExecuteStoredProcedure(string storedProcedure, IEnumerable<SqlParameter>? parameterList)
    {
        OperationResult<int> result = new OperationResult<int>(false);

        if (!string.IsNullOrEmpty(storedProcedure))
        {
            // Connect to SQL Server.
            IOperationResult<SqlConnection> connectResult = CreateAndOpenConnection();
            if (connectResult.Success)
            {
                // Create the command object.
                SqlConnection connection = connectResult.DataContent!;
                IOperationResult<SqlCommand> commandResult = CreateCommandInstance(storedProcedure, connection);
                if (commandResult.Success)
                {
                    SqlCommand command = commandResult.DataContent!;
                    command.CommandType = CommandType.StoredProcedure;

                    // Add the parameters to the command, if present.
                    if (parameterList != null)
                    {
                        command.Parameters.AddRange(parameterList.ToArray());
                    }

                    // Execute.
                    try
                    {
                        int rows = command.ExecuteNonQuery();
                        result.DataContent = rows;
                        result.Success = true;
                    }
                    catch (Exception ex)
                    {
                        AddException(ex);
                        result.AddException(ex);
                        result.Success = false;
                    }
                    command.Dispose();
                }
                else
                {
                    commandResult.CopyTo(result);
                }

                commandResult.Dispose();
                TryClose(connection);
                connection.Dispose();
            }
            else
            {
                connectResult.CopyTo(result);
            }

            connectResult.Dispose();
        }
        return result;
    }
    /// <summary>
    /// Attempts to execute the specified stored procedure with the specified
    /// parameters.
    /// </summary>
    /// <param name="storedProcedure">
    /// A string containing the name of the SQL stored procedure to be executed.
    /// </param>
    /// <param name="parameterList">
    /// An <see cref="IEnumerable{T}"/> of <see cref="SqlParameter"/> instances to send to the command.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="int"/> containing the result of the operation.
    /// If successful, the result contains an integer indicating the number of affected rows.
    /// </returns>
    public async Task<IOperationResult<int>> ExecuteStoredProcedureAsync(string storedProcedure, IEnumerable<SqlParameter>? parameterList)
    {
        OperationResult<int> result = new OperationResult<int>(false);

        if (!string.IsNullOrEmpty(storedProcedure))
        {
            // Connect to SQL Server.
            IOperationResult<SqlConnection> connectResult = await CreateAndOpenConnectionAsync().ConfigureAwait(false);
            if (connectResult.Success)
            {
                // Create the command object.
                SqlConnection connection = connectResult.DataContent!;
                IOperationResult<SqlCommand> commandResult = CreateCommandInstance(storedProcedure, connection);
                if (commandResult.Success)
                {
                    SqlCommand command = commandResult.DataContent!;
                    command.CommandType = CommandType.StoredProcedure;

                    // Add the parameters to the command, if present.
                    if (parameterList != null)
                    {
                        command.Parameters.AddRange(parameterList.ToArray());
                    }

                    // Execute.
                    try
                    {
                        int rows = await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                        result.DataContent = rows;
                        result.Success = true;
                    }
                    catch (Exception ex)
                    {
                        AddException(ex);
                        result.AddException(ex);
                        result.Success = false;
                    }
                    command.Dispose();
                }
                else
                {
                    commandResult.CopyTo(result);
                }

                commandResult.Dispose();
                TryClose(connection);
                connection.Dispose();
            }
            else
            {
                connectResult.CopyTo(result);
            }

            connectResult.Dispose();
        }
        return result;
    }
    #endregion

    #region Data Reader Methods / Functions
    /// <summary>
    /// Executes the supplied SQL Query and returns the resulting SQL data reader.
    /// </summary>
    /// <param name="sqlCommand">
    /// A string containing the SQL to be executed.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="SqlDataReader"/> containing the result of the operation.
    /// If successful, the result returns a the <see cref="SqlDataReader"/> resulting from the query.
    /// </returns>
    public IOperationResult<ISafeSqlDataReader> GetDataReader(string sqlCommand)
    {
        OperationResult<ISafeSqlDataReader> result = new OperationResult<ISafeSqlDataReader>(false);

        //
        // Do not dispose of the command or connection objects as their ownership will be passed to the returned
        // reader instance - if successful; otherwise, both are disposed of in the exception handler.
        //

        // Connect to SQL Server.
        IOperationResult<SqlConnection> connectResult = CreateAndOpenConnection();
        if (connectResult.Success)
        {
            // Create the command instance.
            SqlConnection connection = connectResult.DataContent!;
            IOperationResult<SqlCommand> commandResult = CreateCommandInstance(sqlCommand, connection);
            if (commandResult.Success)
            {
                SqlCommand command = commandResult.DataContent!;
                result = (OperationResult<ISafeSqlDataReader>)GetDataReader(command);
            }
            commandResult.Dispose();
        }
        connectResult.Dispose();

        return result;
    }
    /// <summary>
    /// Executes the supplied SQL Query and returns the resulting SQL data reader.
    /// </summary>
    /// <param name="sqlCommand">
    /// A string containing the SQL to be executed.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="SqlDataReader"/> containing the result of the operation.
    /// If successful, the result returns a the <see cref="SqlDataReader"/> resulting from the query.
    /// </returns>
    public async Task<IOperationResult<ISafeSqlDataReader>> GetDataReaderAsync(string sqlCommand)
    {
        OperationResult<ISafeSqlDataReader> result = new OperationResult<ISafeSqlDataReader>(false);

        //
        // Do not dispose of the command or connection objects as their ownership will be passed to the returned
        // reader instance - if successful; otherwise, both are disposed of in the exception handler.
        //

        // Connect to SQL Server.
        IOperationResult<SqlConnection> connectResult = await CreateAndOpenConnectionAsync().ConfigureAwait(false);
        if (connectResult.Success)
        {
            // Create the command instance.
            SqlConnection connection = connectResult.DataContent!;
            IOperationResult<SqlCommand> commandResult = CreateCommandInstance(sqlCommand, connection);
            if (commandResult.Success)
            {
                SqlCommand command = commandResult.DataContent!;
                result = (OperationResult<ISafeSqlDataReader>)await GetDataReaderAsync(command).ConfigureAwait(false);
            }
            commandResult.Dispose();
        }
        connectResult.Dispose();

        return result;
    }
    /// <summary>
    /// Executes the supplied SQL Query and returns the resulting SQL data reader.
    /// </summary>
    /// <param name="command">
    /// The <see cref="SqlCommand"/> to be executed.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="SqlDataReader"/> containing the result of the operation.
    /// If successful, the result returns a the <see cref="SqlDataReader"/> resulting from the query.
    /// </returns>
    public IOperationResult<ISafeSqlDataReader> GetDataReader(SqlCommand command)
    {
        OperationResult<ISafeSqlDataReader> result = new OperationResult<ISafeSqlDataReader>(false);

        try
        {
            SqlDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult);
            result.Success = true;
            result.DataContent = new SafeSqlDataReader(reader, command);
        }
        catch (Exception ex)
        {
            AddException(ex);
            result.AddException(ex);
        }

        return result;
    }
    /// <summary>
    /// Executes the supplied SQL Query and returns the resulting SQL data reader.
    /// </summary>
    /// <param name="command">
    /// The <see cref="SqlCommand"/> to be executed.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="SqlDataReader"/> containing the result of the operation.
    /// If successful, the result returns a the <see cref="SqlDataReader"/> resulting from the query.
    /// </returns>
    public async Task<IOperationResult<ISafeSqlDataReader>> GetDataReaderAsync(SqlCommand command)
    {
        OperationResult<ISafeSqlDataReader> result = new OperationResult<ISafeSqlDataReader>(false);

        try
        {
            SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess)
                .ConfigureAwait(false);
            result.Success = true;
            result.DataContent = new SafeSqlDataReader(reader, command);
        }
        catch (Exception ex)
        {
            AddException(ex);
            result.AddException(ex);
        }

        return result;
    }
    /// <summary>
    /// Attempts to execute the specified query and return an <see cref="ISafeSqlDataReader"/>
    /// instance containing zero or one rows in the results.
    /// </summary>
    /// <param name="sqlToExecute">
    /// A string containing the SQL query to execute.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the result of the operation.
    /// If successful, the result contains an <see cref="ISafeSqlDataReader"/> containing the results for reading.
    /// </returns>
    public IOperationResult<ISafeSqlDataReader?> GetOneRowReaderForQuery(string sqlToExecute)
    {
        OperationResult<ISafeSqlDataReader?> result = new OperationResult<ISafeSqlDataReader?>(false);

        //
        // Do not dispose of the command or connection objects as their ownership will be passed to the returned
        // reader instance - if successful; otherwise, both are disposed of in the exception handler.
        //

        // Connect to SQL Server.
        IOperationResult<SqlConnection> connectResult = CreateAndOpenConnection();
        if (connectResult.Success)
        {
            // Create the command instance.
            SqlConnection connection = connectResult.DataContent!;
            IOperationResult<SqlCommand> commandResult = CreateCommandInstance(sqlToExecute, connection);
            if (commandResult.Success)
            {
                SqlCommand command = commandResult.DataContent!;
                try
                {
                    SqlDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess | CommandBehavior.SingleResult);
                    result.DataContent = new SafeSqlDataReader(reader, command);
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    command.Dispose();
                    connection.Dispose();
                    AddException(ex);
                    result.AddException(ex);
                }
            }
            else
            {
                commandResult.CopyTo(result);
            }
        }
        else
        {
            connectResult.CopyTo(result);
        }

        connectResult.Dispose();

        return result;
    }
    /// <summary>
    /// Attempts to execute the specified query and return an <see cref="ISafeSqlDataReader"/>
    /// instance containing zero or one rows in the results.
    /// </summary>
    /// <param name="sqlToExecute">
    /// A string containing the SQL query to execute.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the result of the operation.
    /// If successful, the result contains an <see cref="ISafeSqlDataReader"/> containing the results for reading.
    /// </returns>
    public async Task<IOperationResult<ISafeSqlDataReader?>> GetOneRowReaderForQueryAsync(string sqlToExecute)
    {
        OperationResult<ISafeSqlDataReader?> result = new OperationResult<ISafeSqlDataReader?>(false);

        //
        // Do not dispose of the command or connection objects as their ownership will be passed to the returned
        // reader instance - if successful; otherwise, both are disposed of in the exception handler.
        //

        // Connect to SQL Server.
        IOperationResult<SqlConnection> connectResult = await CreateAndOpenConnectionAsync().ConfigureAwait(false);
        if (connectResult.Success)
        {
            // Create the command instance.
            SqlConnection connection = connectResult.DataContent!;
            IOperationResult<SqlCommand> commandResult = CreateCommandInstance(sqlToExecute, connection);
            if (commandResult.Success)
            {
                SqlCommand command = commandResult.DataContent!;
                try
                {
                    SqlDataReader reader = await command.ExecuteReaderAsync(
                        CommandBehavior.SequentialAccess | CommandBehavior.SingleResult)
                        .ConfigureAwait(false);

                    result.DataContent = new SafeSqlDataReader(reader, command);
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    command.Dispose();
                    connection.Dispose();
                    AddException(ex);
                    result.AddException(ex);
                }
            }
            else
            {
                commandResult.CopyTo(result);
            }
        }
        else
        {
            connectResult.CopyTo(result);
        }

        connectResult.Dispose();

        return result;
    }
    /// <summary>
    /// Attempts to execute the specified query and return an <see cref="ISafeSqlDataReader"/>
    /// instance containing zero or one rows in the results.
    /// </summary>
    /// <param name="command">
    /// The <see cref="SqlCommand"/> to execute to retrieve the data.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the result of the operation.
    /// If successful, the result contains an <see cref="ISafeSqlDataReader"/> containing the results for reading.
    /// </returns>
    public IOperationResult<ISafeSqlDataReader?> GetOneRowReaderForQuery(SqlCommand command)
    {
        OperationResult<ISafeSqlDataReader?> result = new OperationResult<ISafeSqlDataReader?>(false);

        try
        {
            SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SingleRow);
            result.DataContent = new SafeSqlDataReader(reader, command);
            result.Success = true;
        }
        catch (Exception ex)
        {
            AddException(ex);
            result.AddException(ex);
        }

        return result;
    }
    /// <summary>
    /// Attempts to execute the specified query and return an <see cref="ISafeSqlDataReader"/>
    /// instance containing zero or one rows in the results.
    /// </summary>
    /// <param name="command">
    /// The <see cref="SqlCommand"/> to execute to retrieve the data.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the result of the operation.
    /// If successful, the result contains an <see cref="ISafeSqlDataReader"/> containing the results for reading.
    /// </returns>
    public async Task<IOperationResult<ISafeSqlDataReader?>> GetOneRowReaderForQueryAsync(SqlCommand command)
    {
        OperationResult<ISafeSqlDataReader?> result = new OperationResult<ISafeSqlDataReader?>(false);

        try
        {
            SqlDataReader reader = await command.ExecuteReaderAsync(
                CommandBehavior.SingleResult | CommandBehavior.SingleRow)
                .ConfigureAwait(false);
            result.DataContent = new SafeSqlDataReader(reader, command);
            result.Success = true;
        }
        catch (Exception ex)
        {
            AddException(ex);
            result.AddException(ex);
        }

        return result;
    }
    /// <summary>
    /// Attempts to execute the specified query and return an <see cref="ISafeSqlDataReader"/>
    /// instance containing the results.
    /// </summary>
    /// <param name="sqlToExecute">
    /// A string containing the SQL query to execute.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the result of the operation.
    /// If successful, the result contains an <see cref="ISafeSqlDataReader"/> containing the results for reading.
    /// </returns>
    public IOperationResult<ISafeSqlDataReader?> GetReaderForMultiResultQuery(string sqlToExecute)
    {
        OperationResult<ISafeSqlDataReader?> result = new OperationResult<ISafeSqlDataReader?>(false);

        //
        // Do not dispose of the command or connection objects as their ownership will be passed to the returned
        // reader instance - if successful; otherwise, both are disposed of in the exception handler.
        //

        // Connect to SQL Server.
        IOperationResult<SqlConnection> connectResult = CreateAndOpenConnection();
        if (connectResult.Success)
        {
            // Create the command instance.
            SqlConnection connection = connectResult.DataContent!;
            IOperationResult<SqlCommand> commandResult = CreateCommandInstance(sqlToExecute, connection);
            if (commandResult.Success)
            {
                SqlCommand? command = commandResult.DataContent;
                if (command != null)
                {
                    IOperationResult<ISafeSqlDataReader?> execResult = GetReaderForMultiResultQuery(command);
                    execResult.CopyTo(result);
                    execResult.Dispose();
                }
            }
            else
            {
                commandResult.CopyTo(result);
            }

            commandResult.Dispose();
        }
        else
        {
            connectResult.CopyTo(result);
        }

        connectResult.Dispose();
        return result;
    }
    /// <summary>
    /// Attempts to execute the specified query and return an <see cref="ISafeSqlDataReader"/>
    /// instance containing the results.
    /// </summary>
    /// <param name="command">
    /// The <see cref="SqlCommand"/> to execute to retrieve the data.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the result of the operation.
    /// If successful, the result contains an <see cref="ISafeSqlDataReader"/> containing the results for reading.
    /// </returns>
    public IOperationResult<ISafeSqlDataReader?> GetReaderForMultiResultQuery(SqlCommand command)
    {
        OperationResult<ISafeSqlDataReader?> result = new OperationResult<ISafeSqlDataReader?>(false);

        //
        // Do not dispose of the command or connection objects as their ownership will be passed to the returned
        // reader instance - if successful; otherwise, both are disposed of in the exception handler.
        //
        try
        {
            SqlDataReader reader = command.ExecuteReader();
            result.DataContent = new SafeSqlDataReader(reader, command);
            result.Success = true;
        }
        catch (Exception ex)
        {
            AddException(ex);
            result.AddException(ex);
            result.Success = false;
            command.Connection?.Close();
            command.Connection?.Dispose();
            command.Dispose();
        }

        return result;
    }
    /// <summary>
    /// Attempts to execute the specified query and return an <see cref="ISafeSqlDataReader"/>
    /// instance containing the results.
    /// </summary>
    /// <param name="sqlToExecute">
    /// A string containing the SQL query to execute.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the result of the operation.
    /// If successful, the result contains an <see cref="ISafeSqlDataReader"/> containing the results for reading.
    /// </returns>
    public async Task<IOperationResult<ISafeSqlDataReader?>> GetReaderForMultiResultQueryAsync(string sqlToExecute)
    {
        OperationResult<ISafeSqlDataReader?> result = new OperationResult<ISafeSqlDataReader?>(false);

        //
        // Do not dispose of the command or connection objects as their ownership will be passed to the returned
        // reader instance - if successful; otherwise, both are disposed of in the exception handler.
        //

        // Connect to SQL Server.
        IOperationResult<SqlConnection> connectResult = await CreateAndOpenConnectionAsync().ConfigureAwait(false);
        if (connectResult.Success)
        {
            // Create the command instance.
            SqlConnection connection = connectResult.DataContent!;
            IOperationResult<SqlCommand> commandResult = CreateCommandInstance(sqlToExecute, connection);
            if (commandResult.Success)
            {
                SqlCommand? command = commandResult.DataContent;
                result = (OperationResult<ISafeSqlDataReader?>)await GetReaderForMultiResultQueryAsync(command).ConfigureAwait(false);
            }
            else
            {
                commandResult.CopyTo(result);
            }

            commandResult.Dispose();
        }
        else
        {
            connectResult.CopyTo(result);
        }

        connectResult.Dispose();
        return result;
    }
    /// <summary>
    /// Attempts to execute the specified query and return an <see cref="ISafeSqlDataReader"/>
    /// instance containing the results.
    /// </summary>
    /// <param name="command">
    /// The <see cref="SqlCommand"/> to execute to retrieve the data.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the result of the operation.
    /// If successful, the result contains an <see cref="ISafeSqlDataReader"/> containing the results for reading.
    /// </returns>
    public async Task<IOperationResult<ISafeSqlDataReader?>> GetReaderForMultiResultQueryAsync(SqlCommand? command)
    {
        OperationResult<ISafeSqlDataReader?> result = new OperationResult<ISafeSqlDataReader?>(false);

        if (command != null)
        {
            //
            // Do not dispose of the command or connection objects as their ownership will be passed to the returned
            // reader instance - if successful; otherwise, both are disposed of in the exception handler.
            //
            try
            {
                SqlDataReader reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
                result.DataContent = new SafeSqlDataReader(reader, command);
                result.Success = true;
            }
            catch (Exception ex)
            {
                AddException(ex);
                result.AddException(ex);
                result.Success = false;
                command.Connection?.Close();
                command.Connection?.Dispose();
                command.Dispose();
            }
        }
        return result;
    }
    /// <summary>
    /// Attempts to execute the specified SQL command and return an <see cref="ISafeSqlDataReader"/>
    /// instance containing the results.
    /// </summary>
    /// <param name="command">
    /// A <see cref="SqlCommand"/> instance containing the command definition.
    /// </param>
    /// <param name="sqlParamsList">
    /// A <see cref="List{T}"/> of <see cref="SqlParameter"/> instances defined
    /// and populated for the stored procedure.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the result of the operation.
    /// If successful, the result contains an <see cref="ISafeSqlDataReader"/> containing the results for reading.
    /// </returns>
    public IOperationResult<ISafeSqlDataReader?> GetReaderForParameterizedCommand(SqlCommand command,
        IEnumerable<SqlParameter>? sqlParamsList)
    {
        OperationResult<ISafeSqlDataReader?> result = new OperationResult<ISafeSqlDataReader?>(false);

        IOperationResult<SqlConnection> connectResult = CreateAndOpenConnection();
        if (connectResult.Success)
        {
            SqlConnection connection = connectResult.DataContent!;

            // Configure the command.
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = int.MaxValue;

            // Add the parameters.
            if (sqlParamsList != null)
            {
                SqlParameter[] list = sqlParamsList.ToArray();
                if (list.Length > 0)
                {
                    command.Parameters.AddRange(list);
                }
            }

            try
            {
                SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
                result.DataContent = new SafeSqlDataReader(reader, command);
                result.Success = true;
            }
            catch (Exception ex)
            {
                AddException(ex);
                result.Success = false;
                result.AddException(ex);
                connection.Dispose();
            }
        }
        else
        {
            connectResult.CopyTo(result);
        }

        connectResult.Dispose();

        return result;
    }
    /// <summary>
    /// Attempts to execute the specified SQL command and return an <see cref="ISafeSqlDataReader"/>
    /// instance containing the results.
    /// </summary>
    /// <param name="command">
    /// A <see cref="SqlCommand"/> instance containing the command definition.
    /// </param>
    /// <param name="sqlParamsList">
    /// A <see cref="List{T}"/> of <see cref="SqlParameter"/> instances defined
    /// and populated for the stored procedure.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the result of the operation.
    /// If successful, the result contains an <see cref="ISafeSqlDataReader"/> containing the results for reading.
    /// </returns>
    public async Task<IOperationResult<ISafeSqlDataReader?>> GetReaderForParameterizedCommandAsync(SqlCommand? command, IEnumerable<SqlParameter>? sqlParamsList)
    {
        OperationResult<ISafeSqlDataReader?> result = new OperationResult<ISafeSqlDataReader?>(false);

        if (command != null)
        {
            IOperationResult<SqlConnection> connectResult = await CreateAndOpenConnectionAsync().ConfigureAwait(false);
            if (connectResult.Success)
            {
                SqlConnection connection = connectResult.DataContent!;

                // Configure the command.
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = int.MaxValue;

                // Add the parameters.
                if (sqlParamsList != null)
                {
                    SqlParameter[] list = sqlParamsList.ToArray();
                    if (list.Length > 0)
                    {
                        command.Parameters.AddRange(list);
                    }
                }

                try
                {
                    SqlDataReader reader = await command.ExecuteReaderAsync(
                        CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)
                        .ConfigureAwait(false);
                    result.DataContent = new SafeSqlDataReader(reader, command);
                    result.Success = true;
                }
                catch (Exception ex)
                {
                    AddException(ex);
                    result.Success = false;
                    result.AddException(ex);
                    connection.Dispose();
                }
            }
            else
            {
                connectResult.CopyTo(result);
            }

            connectResult.Dispose();
        }
        return result;
    }
    /// <summary>
    /// Attempts to execute the specified SQL command as a stored procedure and return an <see cref="ISafeSqlDataReader"/>
    /// instance containing the results.
    /// </summary>
    /// <param name="storedProcedure">
    /// A <see cref="SqlCommand"/> instance containing the name of the stored procedure to execute.
    /// </param>
    /// <param name="sqlParamsList">
    /// A <see cref="List{T}"/> of <see cref="SqlParameter"/> instances defined
    /// and populated for the stored procedure.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the result of the operation.
    /// If successful, the result contains an <see cref="ISafeSqlDataReader"/> containing the results for reading.
    /// </returns>
    public IOperationResult<ISafeSqlDataReader?> GetReaderForStoredProcedure(SqlCommand storedProcedure, IEnumerable<SqlParameter> sqlParamsList)
    {
        OperationResult<ISafeSqlDataReader?> result = new OperationResult<ISafeSqlDataReader?>(false);

        IOperationResult<SqlConnection> connectResult = CreateAndOpenConnection();
        if (connectResult.Success)
        {
            SqlConnection connection = connectResult.DataContent!;

            // Configure the command.
            storedProcedure.Connection = connection;
            storedProcedure.CommandType = CommandType.StoredProcedure;
            storedProcedure.CommandTimeout = int.MaxValue;

            // Add the parameters.
            SqlParameter[] list = sqlParamsList.ToArray();
            if (list.Length > 0)
            {
                storedProcedure.Parameters.AddRange(list);
            }

            try
            {
                SqlDataReader reader = storedProcedure.ExecuteReader(CommandBehavior.SingleResult | CommandBehavior.SequentialAccess);
                result.DataContent = new SafeSqlDataReader(reader, storedProcedure);
                result.Success = true;
            }
            catch (Exception ex)
            {
                AddException(ex);
                result.Success = false;
                result.AddException(ex);
                connection.Dispose();
            }
        }
        else
        {
            connectResult.CopyTo(result);
        }

        connectResult.Dispose();

        return result;
    }
    /// <summary>
    /// Attempts to execute the specified SQL command as a stored procedure and return an <see cref="ISafeSqlDataReader"/>
    /// instance containing the results.
    /// </summary>
    /// <param name="storedProcedure">
    /// A <see cref="SqlCommand"/> instance containing the name of the stored procedure to execute.
    /// </param>
    /// <param name="sqlParamsList">
    /// A <see cref="List{T}"/> of <see cref="SqlParameter"/> instances defined
    /// and populated for the stored procedure.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the result of the operation.
    /// If successful, the result contains an <see cref="ISafeSqlDataReader"/> containing the results for reading.
    /// </returns>
    public async Task<IOperationResult<ISafeSqlDataReader?>> GetReaderForStoredProcedureAsync(SqlCommand storedProcedure, IEnumerable<SqlParameter>? sqlParamsList)
    {
        OperationResult<ISafeSqlDataReader?> result = new OperationResult<ISafeSqlDataReader?>(false);

        IOperationResult<SqlConnection> connectResult = await CreateAndOpenConnectionAsync().ConfigureAwait(false);
        if (connectResult.Success)
        {
            SqlConnection connection = connectResult.DataContent!;

            // Configure the command.
            storedProcedure.Connection = connection;
            storedProcedure.CommandType = CommandType.StoredProcedure;
            storedProcedure.CommandTimeout = int.MaxValue;

            // Add the parameters.
            if (sqlParamsList != null)
            {
                SqlParameter[] list = sqlParamsList.ToArray();
                if (list.Length > 0)
                {
                    storedProcedure.Parameters.AddRange(list);
                }
            }

            try
            {
                SqlDataReader reader = await storedProcedure.ExecuteReaderAsync(CommandBehavior.SingleResult | CommandBehavior.SequentialAccess)
                    .ConfigureAwait(false);
                result.DataContent = new SafeSqlDataReader(reader, storedProcedure);
                result.Success = true;
            }
            catch (Exception ex)
            {
                AddException(ex);
                result.Success = false;
                result.AddException(ex);
                connection.Dispose();
            }
        }
        else
        {
            connectResult.CopyTo(result);
        }

        connectResult.Dispose();

        return result;
    }


    /// <summary>
    /// Attempts to execute the specified stored procedure and return an <see cref="ISafeSqlDataReader"/>
    /// instance containing the results.
    /// </summary>
    /// <param name="command">
    /// A <see cref="SqlCommand"/> instance containing the command definition.
    /// </param>
    /// <param name="sqlParamsList">
    /// A <see cref="List{T}"/> of <see cref="SqlParameter"/> instances defined
    /// and populated for the stored procedure.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the result of the operation.
    /// If successful, the result contains an <see cref="ISafeSqlDataReader"/> containing the results for reading.
    /// </returns>
    public IOperationResult<ISafeSqlDataReader?> GetMultiResultSetReaderForParameterizedCommand(SqlCommand command, IEnumerable<SqlParameter> sqlParamsList)
    {
        OperationResult<ISafeSqlDataReader?> result = new OperationResult<ISafeSqlDataReader?>(false);

        IOperationResult<SqlConnection> connectResult = CreateAndOpenConnection();
        if (connectResult.Success)
        {
            SqlConnection connection = connectResult.DataContent!;

            // Configure the command.
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = int.MaxValue;

            // Add the parameters.
            SqlParameter[] list = sqlParamsList.ToArray();
            if (list.Length > 0)
            {
                command.Parameters.AddRange(list);
            }

            try
            {
                SqlDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess);
                result.DataContent = new SafeSqlDataReader(reader, command);
                result.Success = true;
            }
            catch (Exception ex)
            {
                AddException(ex);
                result.Success = false;
                result.AddException(ex);
                connection.Dispose();
            }
        }
        else
        {
            connectResult.CopyTo(result);
        }

        connectResult.Dispose();

        return result;
    }
    /// <summary>
    /// Attempts to execute the specified stored procedure and return an <see cref="ISafeSqlDataReader"/>
    /// instance containing the results.
    /// </summary>
    /// <param name="command">
    /// A <see cref="SqlCommand"/> instance containing the command definition.
    /// </param>
    /// <param name="sqlParamsList">
    /// A <see cref="List{T}"/> of <see cref="SqlParameter"/> instances defined
    /// and populated for the stored procedure.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> containing the result of the operation.
    /// If successful, the result contains an <see cref="ISafeSqlDataReader"/> containing the results for reading.
    /// </returns>
    public async Task<IOperationResult<ISafeSqlDataReader?>> GetMultiResultSetReaderForParameterizedCommandAsync(SqlCommand command, IEnumerable<SqlParameter> sqlParamsList)
    {
        OperationResult<ISafeSqlDataReader?> result = new OperationResult<ISafeSqlDataReader?>(false);

        IOperationResult<SqlConnection> connectResult = await CreateAndOpenConnectionAsync().ConfigureAwait(false);
        if (connectResult.Success)
        {
            SqlConnection connection = connectResult.DataContent!;

            // Configure the command.
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            command.CommandTimeout = int.MaxValue;

            // Add the parameters.
            SqlParameter[] list = sqlParamsList.ToArray();
            if (list.Length > 0)
            {
                command.Parameters.AddRange(list);
            }

            try
            {
                SqlDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess)
                    .ConfigureAwait(false);
                result.DataContent = new SafeSqlDataReader(reader, command);
                result.Success = true;
            }
            catch (Exception ex)
            {
                AddException(ex);
                result.Success = false;
                result.AddException(ex);
                connection.Dispose();
            }
        }
        else
        {
            connectResult.CopyTo(result);
        }

        connectResult.Dispose();

        return result;
    }
    #endregion

    #region General Utility Methods
    /// <summary>
    /// Gets the names of the database for the SQL Server instance currently connected to.
    /// </summary>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="List{T}"/> of <see cref="string"/> containing the result of the
    /// operation.  If successful, the result contains the <see cref="List{T}"/> of <see cref="string"/> values containing the
    /// names of the databases on the server.
    /// </returns>
    public IOperationResult<List<string>> GetDatabaseNames()
    {
        OperationResult<List<string>> result = new OperationResult<List<string>>(false);

        IOperationResult<ISafeSqlDataReader> readResult = GetDataReader(SqlGetDbNamesQuery);
        if (readResult.Success)
        {
            ISafeSqlDataReader reader = readResult.DataContent!;
            List<string> dbList = new List<string>(10);
            while (reader.Read())
            {
                string? name = reader.GetString(0);
                if (!string.IsNullOrEmpty(name))
                {
                    dbList.Add(name);
                }
            }
            reader.Dispose();
            result.Success = true;
            result.DataContent = dbList;
        }
        readResult.Dispose();

        return result;
    }
    /// <summary>
    /// Gets the names of the database for the SQL Server instance currently connected to.
    /// </summary>
    /// <returns>
    /// An <see cref="IOperationResult{T}"/> of <see cref="List{T}"/> of <see cref="string"/> containing the result of the
    /// operation.  If successful, the result contains the <see cref="List{T}"/> of <see cref="string"/> values containing the
    /// names of the databases on the server.
    /// </returns>
    public async Task<IOperationResult<List<string>>> GetDatabaseNamesAsync()
    {
        OperationResult<List<string>> result = new OperationResult<List<string>>(false);

        IOperationResult<ISafeSqlDataReader> readResult = await GetDataReaderAsync(SqlGetDbNamesQuery).ConfigureAwait(false);
        if (readResult.Success)
        {
            ISafeSqlDataReader reader = readResult.DataContent!;
            List<string> dbList = new List<string>(10);

            while (reader.Read())
            {
                string? name = reader.GetString(0);
                if (!string.IsNullOrEmpty(name))
                {
                    dbList.Add(name);
                }
            }

            reader.Dispose();
            result.Success = true;
            result.DataContent = dbList;
        }
        readResult.Dispose();

        return result;
    }
    /// <summary>
    /// Gets the list of SQL statements from the provided query string.
    /// </summary>
    /// <param name="sqlQuery">
    /// A string containing the SQL query code.
    /// </param>
    /// <returns>
    /// A <see cref="List{T}"/> of <see cref="TSqlStatement"/> instances if successful. 
    /// Otherwise, returns <b>null</b>. 
    /// </returns>
    public static List<TSqlStatement>? GetStatements(string? sqlQuery)
    {
        if (sqlQuery == null)
        {
            return null;
        }
        else
        {
            return NativeTSqlCodeDom.ParseStatements(sqlQuery);
        }
    }
    /// <summary>
    /// Determines whether the specified SQL query has a select statement.
    /// </summary>
    /// <param name="sql">
    /// A string containing the SQL statement to examine.</param>
    /// <returns>
    ///   <c>true</c> if the specified SQL has select; otherwise, <c>false</c>.
    /// </returns>
    public static bool HasSelect(string? sql)
    {
        if (sql == null)
        {
            return false;
        }

        return NativeTSqlCodeDom.HasSelectStatement(sql);
    }
    /// <summary>
    /// Parses the supplied SQL text and returns any syntax errors.
    /// </summary>
    /// <param name="sql">The SQL text to parse.</param>
    /// <returns>
    /// <see langword="null"/> when parsing succeeds or the input is <see langword="null"/>;
    /// otherwise, a collection containing the parse errors.
    /// </returns>
    public static SqlQueryErrorCollection? ParseSqlString(string? sql)
    {
        if (sql == null)
        {
            return null;
        }

        return NativeTSqlCodeDom.ParseSql(sql);
    }
    /// <summary>
    /// Asynchronously parses the supplied SQL text and returns any syntax errors.
    /// </summary>
    /// <param name="sql">The SQL text to parse.</param>
    /// <returns>
    /// A task that produces <see langword="null"/> when parsing succeeds or the input is
    /// <see langword="null"/>; otherwise, a collection containing the parse errors.
    /// </returns>
    public async Task<SqlQueryErrorCollection?> ParseSqlAsync(string? sql)
    {
        if (sql != null)
        {
            await Task.Yield();
            return ParseSqlString(sql);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Sets the name of the current database to be read from.
    /// </summary>
    /// <param name="databaseName">
    /// A string containing the name of the database.
    /// </param>
    public void SetDatabase(string databaseName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        if (_connectionString != null)
        {
            _connectionString.InitialCatalog = databaseName;
        }
    }
    /// <summary>
    /// Tests the ability to connect to SQL Server asynchronously.
    /// </summary>
    /// <returns>
    /// An <see cref="IOperationResult"/> containing the result of the operation.
    /// </returns>
    public IOperationResult TestConnection()
    {
        IOperationResult result = new OperationResult(false);

        IOperationResult<SqlConnection> connectResult = CreateAndOpenConnection();
        if (connectResult.Success)
        {
            result.Success = true;
            connectResult.DataContent!.Dispose();
        }
        else
        {
            connectResult.CopyTo(result);
        }

        connectResult.Dispose();
        return result;
    }
    /// <summary>
    /// Tests the ability to connect to SQL Server asynchronously.
    /// </summary>
    /// <returns>
    /// A task that produces an <see cref="IOperationResult"/> indicating whether the connection succeeded.
    /// </returns>
    public async Task<IOperationResult> TestConnectionAsync()
    {
        IOperationResult result = new OperationResult(false);

        IOperationResult<SqlConnection> connectResult = await CreateAndOpenConnectionAsync().ConfigureAwait(false);
        if (connectResult.Success)
        {
            result.Success = true;
            connectResult.DataContent!.Dispose();
        }
        else
        {
            connectResult.CopyTo(result);
        }

        connectResult.Dispose();
        return result;
    }
    /// <summary>
    /// Tests the ability to connect to SQL Server asynchronously.
    /// </summary>
    /// <param name="connectionString">
    /// A <see cref="SqlConnectionStringBuilder"/> containing the data needed to connect to a SQL Server.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult"/> containing the result of the operation.
    /// </returns>
    public IOperationResult TestConnection(SqlConnectionStringBuilder connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = new SqlConnectionStringBuilder(connectionString.ToString());
        return TestConnection();
    }
    /// <summary>
    /// Tests the ability to connect to SQL Server asynchronously.
    /// </summary>
    /// <param name="connectionString">
    /// A <see cref="SqlConnectionStringBuilder"/> containing the data needed to connect to a SQL Server.
    /// </param>
    /// <returns>
    /// An <see cref="IOperationResult"/> containing the result of the operation.
    /// </returns>
    public Task<IOperationResult> TestConnectionAsync(SqlConnectionStringBuilder connectionString)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        _connectionString = new SqlConnectionStringBuilder(connectionString.ToString());
        return TestConnectionAsync();
    }
    #endregion

    #endregion

    #region Private Methods / Functions
    /// <summary>
    /// Creates the SQL Transaction instance.
    /// </summary>
    /// <param name="connection">
    /// The <see cref="SqlConnection"/> instance to use.
    /// </param>
    /// <param name="result">
    /// The parent <see cref="IOperationResult"/> instance being used by the caller.
    /// </param>
    /// <returns>
    /// A <see cref="SqlTransaction"/> instance if successful; otherwise, returns <b>null</b>.
    /// </returns>
    private SqlTransaction? CreateTransactionInstance(SqlConnection connection, IOperationResult result)
    {
        // Attempt to create the SQL transaction instance.
        SqlTransaction? transaction;
        try
        {
            transaction = connection.BeginTransaction();
        }
        catch (Exception ex)
        {
            transaction = null;
            result.Success = false;
            result.AddException(ex);
        }
        return transaction;
    }
    /// <summary>
    /// Creates an empty SQL command instance for the provided SQL connection.
    /// </summary>
    /// <param name="connection">
    /// The <see cref="SqlConnection"/> instance to use.
    /// </param>
    /// <param name="result">
    /// The parent <see cref="IOperationResult"/> instance being used by the caller.
    /// </param>
    /// <returns>
    /// A <see cref="SqlCommand"/> instance if successful; otherwise, returns <b>null</b>.
    /// </returns>
    private SqlCommand? CreateEmptySqlCommandInstance(SqlConnection connection, IOperationResult result)
    {
        SqlCommand? command;
        try
        {
            command = new SqlCommand(string.Empty, connection);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.AddException(ex);
            command = null;
        }

        return command;
    }
    /// <summary>
    /// Closes the SQL Server connection.
    /// </summary>
    /// <param name="connection">The <see cref="SqlConnection"/> instance to be closed.
    /// </param>
    private void TryClose(SqlConnection? connection)
    {
        if (connection != null)
        {
            try
            {
                connection.Close();
            }
            catch (SqlException ex)
            {
                AddException(ex);
            }
        }
    }
    #endregion
}

