using Adaptive.SqlMaker.Interfaces;
using Provident.Foundations.Abstractions;

namespace Adaptive.SqlMaker
{
    /// <summary>
    /// Represents and contains the data for the SQL Maker Project file.
    /// </summary>
    /// <seealso cref="DisposableObjectBase" />
    public sealed class SqlDataProject : ExceptionTrackingBase, ISqlDataProjectObject
    {
        #region Private Member Declarations
        /// <summary>
        /// The file name
        /// </summary>
        private string? _fileName;
        /// <summary>
        /// The project name.
        /// </summary>
        private string? _projectName;
        /// <summary>
        /// The description text.
        /// </summary>
        private string? _description;

        /// <summary>
        /// The Default data source.
        /// </summary>
        private SqlDataProjectDataSource? _defaultDataSource;
        /// <summary>
        /// The project options container.
        /// </summary>
        private SqlDataProjectOptions? _options;
        /// <summary>
        /// The data sources list.
        /// </summary>
        private SqlDataProjectDataSourceCollection? _dataSources;
        /// <summary>
        /// The tables list.
        /// </summary>
        private SqlDataProjectTableCollection? _tables;
        /// <summary>
        /// The procedures list.
        /// </summary>
        private SqlDataProjectStoredProcedureCollection? _procedures;
        #endregion

        #region Constructor / Dispose Methods        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataProject"/> class.
        /// </summary>
        /// <remarks>
        /// This is the default constructor.
        /// </remarks>
        public SqlDataProject()
        {
            // Set defaults.
            _projectName = MakerConstants.DefaultProjectName;
            _fileName = MakerConstants.CreateDefaultFileName(Directory.GetCurrentDirectory());
            _description = MakerConstants.DefaultDescription;

            _defaultDataSource = new SqlDataProjectDataSource();
            _options = new SqlDataProjectOptions();
            _dataSources = new SqlDataProjectDataSourceCollection();
            _tables = new SqlDataProjectTableCollection();
            _procedures = new SqlDataProjectStoredProcedureCollection();
        }
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources;
        /// <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                _procedures?.Clear();
                _tables?.Clear();
                _dataSources?.Clear();
                _options?.Dispose();
                _defaultDataSource?.Dispose();
            }

            _procedures = null;
            _tables = null;
            _dataSources = null;
            _options = null;
            _defaultDataSource = null;
            _projectName = null;
            _fileName = null;
            _description = null;
            base.Dispose(disposing);
        }

        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the description of the project.
        /// </summary>
        /// <value>
        /// A string containing the description text.
        /// </value>
        public string? Description
        {
            get => _description;
            set => _description = value;
        }
        /// <summary>
        /// Gets or sets the path and name of the file.
        /// </summary>
        /// <value>
        /// A string containing the fully-qualified path and name of the file.
        /// </value>
        public string? FileName
        {
            get => _fileName;
            set => _fileName = value;
        }
        /// <summary>
        /// Gets or sets the name of the project.
        /// </summary>
        /// <value>
        /// A string containing the name of the project.
        /// </value>
        public string? ProjectName
        {
            get => _projectName;
            set => _projectName = value;
        }
        /// <summary>
        /// Gets the reference to the default data source definition.
        /// </summary>
        /// <value>
        /// A <see cref="SqlDataProjectDataSource"/> instance containing the default values.
        /// </value>
        public SqlDataProjectDataSource? DefaultDataSource => _defaultDataSource;
        /// <summary>
        /// Gets the reference to the project options.
        /// </summary>
        /// <value>
        /// A <see cref="SqlDataProjectOptions"/> instance.
        /// </value>
        public SqlDataProjectOptions? Options => _options;
        /// <summary>
        /// Gets the reference to the list of data sources for the project.
        /// </summary>
        /// <value>
        /// A <see cref="SqlDataProjectDataSourceCollection"/> containing the list.
        /// </value>
        public SqlDataProjectDataSourceCollection? DataSources => _dataSources;
        /// <summary>
        /// Gets the reference to the list of tables used in the project.
        /// </summary>
        /// <value>
        /// A <see cref="SqlDataProjectTableCollection"/> containing the list.
        /// </value>
        public SqlDataProjectTableCollection? Tables => _tables;
        /// <summary>
        /// Gets the reference to the list of stored procedures for the project.
        /// </summary>
        /// <value>
        /// A <see cref="SqlDataProjectStoredProcedureCollection"/> containing the list.
        /// </value>
        public SqlDataProjectStoredProcedureCollection? Procedures => _procedures;
        #endregion

        #region Public Methods / Functions
        public bool Load()
        {
            if (string.IsNullOrWhiteSpace(_fileName))
            {
                Exceptions?.Add(new InvalidOperationException("A project file name is required."));
                return false;
            }
            using SqlDataProjectReader reader = new SqlDataProjectReader();
            bool success = reader.ReadFile(_fileName) && Load(reader);
            if (reader.Exceptions != null)
                Exceptions?.AddRange(reader.Exceptions);
            return success;
        }
        public bool Load(SqlDataProjectReader reader)
        {
            ArgumentNullException.ThrowIfNull(reader);
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            if (_options == null || _dataSources == null || _tables == null || _procedures == null)
                throw new InvalidOperationException("The project is not initialized.");
            _dataSources.Clear();
            _tables.Clear();
            _procedures.Clear();
            try
            {
                bool success = reader.ReadPropertiesElement(this) && reader.ReadProjectOptions(_options)
                    && reader.ReadDataSources(_dataSources) && reader.ReadTables(_tables) && reader.ReadProcedures(_procedures);
                if (!success)
                    Exceptions?.Add(new InvalidDataException("The SQL Maker project contains invalid or missing project information."));
                return success;
            }
            catch (ArgumentException ex)
            {
                Exceptions?.Add(new InvalidDataException("The SQL Maker project contains invalid data.", ex));
                return false;
            }
        }
        /// <summary>
        /// Saves the contents of the project to a file.
        /// </summary>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        public bool Save()
        {
            if (string.IsNullOrWhiteSpace(_fileName))
            {
                Exceptions?.Add(new InvalidOperationException("A project file name is required."));
                return false;
            }
            using SqlDataProjectWriter writer = new SqlDataProjectWriter();
            bool success = Save(writer) && writer.WriteFile(_fileName);
            if (writer.Exceptions != null)
                Exceptions?.AddRange(writer.Exceptions);
            return success;
        }
        /// <summary>
        /// Writes the contents of the object to the specified writer instance.
        /// </summary>
        /// <param name="writer">The <see cref="SqlDataProjectWriter" /> reader instance.</param>
        /// <returns>
        ///   <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        public bool Save(SqlDataProjectWriter writer)
        {
            ArgumentNullException.ThrowIfNull(writer);
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            if (_options == null || _dataSources == null || _tables == null || _procedures == null)
                throw new InvalidOperationException("The project is not initialized.");
            bool success = false;

            string connStr;
            if (_defaultDataSource == null || _defaultDataSource.ConnectionString == null)
                connStr = string.Empty;
            else
                connStr = _defaultDataSource.ConnectionString;

            if (_projectName != null)
            {
                if (_description == null)
                    _description = string.Empty;

                success = writer.WritePropertiesElement(_projectName, _description, connStr);
                if (success)
                {
                    success = writer.WriteProjectOptions(_options);
                    if (success)
                    {
                        success = writer.WriteDataSources(_dataSources);
                        if (success)
                        {
                            success = writer.WriteTables(_tables);
                            if (success)
                            {
                                success = writer.WriteProcedures(_procedures);
                            }
                        }
                    }
                }
            }
            return success;
        }
        public void SetDefaultDataSourceConnectionString(string? connectionString)
        {
            if (!string.IsNullOrEmpty(connectionString))
            {
                if (_defaultDataSource == null)
                    _defaultDataSource = new SqlDataProjectDataSource();
                _defaultDataSource.ConnectionString = connectionString;
            }
        }
        #endregion

    }
}