using Microsoft.Data.SqlClient;
using Provident.Foundations.Abstractions;

namespace Provident.Foundations.SqlMaker
{
    /// <summary>
    /// Represents and manages the data for a SQL Server data source connection.
    /// </summary>
    /// <seealso cref="ExceptionTrackingBase" />
    public sealed class SqlDataProjectDataSource : ExceptionTrackingBase
    {
        #region Private Member Declarations        
        /// <summary>
        /// The name given to the instance.
        /// </summary>
        private string? _name;
        /// <summary>
        /// The connection string container.
        /// </summary>
        private SqlConnectionStringBuilder? _connectionString;
        #endregion

        #region Constructor / Dispose Methods        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataProjectDataSource"/> class.
        /// </summary>
        /// <remarks>
        /// This is the default constructor.
        /// </remarks>
        public SqlDataProjectDataSource()
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataProjectDataSource"/> class.
        /// </summary>
        /// <param name="name">
        /// A string containing the name for the data source.
        /// </param>
        public SqlDataProjectDataSource(string name)
        {
            _name = name;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataProjectDataSource"/> class.
        /// </summary>
        /// <param name="name">
        /// A string containing the name for the data source.
        /// </param>
        /// <param name="connectionString">
        /// A string containing the connection string information.
        /// </param>
        public SqlDataProjectDataSource(string name, string connectionString)
        {
            _name = name;
            _connectionString = new SqlConnectionStringBuilder(connectionString);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataProjectDataSource"/> class.
        /// </summary>
        /// <param name="name">
        /// A string containing the name for the data source.
        /// </param>
        /// <param name="connectionString">
        /// A string containing the connection string information.
        /// </param>
        public SqlDataProjectDataSource(string name, SqlConnectionStringBuilder connectionString)
        {
            _name = name;
            _connectionString = new SqlConnectionStringBuilder(connectionString.ToString());
        }
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources;
        /// <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
                _connectionString?.Clear();

            _connectionString = null;
            _name = null;
            base.Dispose(disposing);
        }
        #endregion

        #region Public Properties        
        /// <summary>
        /// Gets or sets the connection string used to connect to SQL Server.
        /// </summary>
        /// <value>
        /// A string containing the connection information.
        /// </value>
        public string? ConnectionString
        {
            get
            {
                if (_connectionString == null)
                    return null;
                else
                    return _connectionString.ToString();
            }
            set
            {
                _connectionString?.Clear();
                _connectionString = new SqlConnectionStringBuilder(value);
            }
        }
        /// <summary>
        /// Gets or sets the name of the data source.
        /// </summary>
        /// <value>
        /// A string containing the data source name value.
        /// </value>
        public string? Name
        {
            get => _name; set => _name = value;
        }
        #endregion
    }
}
