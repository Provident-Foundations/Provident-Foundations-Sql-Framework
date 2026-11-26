using Provident.Foundations.Abstractions;

namespace Provident.Foundations.SqlMaker
{
    /// <summary>
    /// Represents a database table that is a member of the project.
    /// </summary>
    /// <seealso cref="ExceptionTrackingBase" />
    public sealed class SqlDataProjectTable : ExceptionTrackingBase
    {
        #region Private Member Declarations
        /// <summary>
        /// The data source name the table definition came from.
        /// </summary>
        private string? _dataSourceName;
        /// <summary>
        /// The date/time the definition was last updated.
        /// </summary>
        private DateTime? _lastUpdated;
        /// <summary>
        /// The table name.
        /// </summary>
        private string? _name;
        /// <summary>
        /// The table profile information.
        /// </summary>
        private SqlDataProjectTableProfile? _profile;
        /// <summary>
        /// The table qualifier.
        /// </summary>
        private string? _qualifier;
        #endregion

        #region Constructor / Dispose Methods        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataProjectTable"/> class.
        /// </summary>
        /// <remarks>
        /// This is the default constructor.
        /// </remarks>
        public SqlDataProjectTable()
        {
            _profile = new SqlDataProjectTableProfile();
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources;
        /// <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
                _profile?.Dispose();

            _profile = null;
            _name = null;
            _lastUpdated = null;
            _qualifier = null;

            base.Dispose(disposing);
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Get or sets the data source name the table definition came from.
        /// </summary>
        /// <value>
        /// A string containing the name of the data source.
        /// </value>
        public string? DataSourceName
        {
            get => _dataSourceName;
            set => _dataSourceName = value;
        }
        /// <summary>
        /// Gets or sets the date/time the definition was last updated.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> specifying the date/time value.
        /// </value>
        public DateTime? LastUpdated
        {
            get => _lastUpdated;
            set => _lastUpdated = value;
        }
        /// <summary>
        /// Gets or sets the table name.
        /// </summary>
        /// <value>
        /// A string containing the name of the table.
        /// </value>
        public string? Name
        {
            get => _name;
            set => _name = value;
        }
        /// <summary>
        /// Gets the reference to the table profile information.
        /// </summary>
        /// <value>
        /// The <see cref="SqlDataProjectTableProfile"/> instance.
        /// </value>
        public SqlDataProjectTableProfile? Profile => _profile;
        /// <summary>
        /// Gets or sets the table qualifier.
        /// </summary>
        /// <value>
        /// A string containing the qualifier value.
        /// </value>
        public string? Qualifier
        {
            get => _qualifier;
            set => _qualifier = value;
        }
        #endregion
    }
}
