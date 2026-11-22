using Provident.Foundations.Abstractions;
using System.Collections.Specialized;

namespace Adaptive.SqlMaker
{
    /// <summary>
    /// Represents and contains the various option settings for the project.
    /// </summary>
    /// <seealso cref="ExceptionTrackingBase" />
    public sealed class SqlDataProjectOptions : ExceptionTrackingBase
    {
        #region Private Member Declarations        
        /// <summary>
        /// The data access namespace value.
        /// </summary>
        private string? _dataAccessNamespace;
        /// <summary>
        /// The default identifier field name value.
        /// </summary>
        private string? _defaultIdField;
        /// <summary>
        /// The default soft delete flag.
        /// </summary>
        private bool _defaultSoftDelete;
        /// <summary>
        /// The default soft delete field name value.
        /// </summary>
        private string? _defaultSoftDeleteField;
        /// <summary>
        /// The entity namespace value.
        /// </summary>
        private string? _entityNamespace;
        /// <summary>
        /// The list of standard namespaces to import.
        /// </summary>
        private StringCollection? _standardNamespaces;
        /// <summary>
        /// The stored procedure name prefix value.
        /// </summary>
        private string? _storedProcedureNamePrefix;
        /// <summary>
        /// The stored procedure name suffix value.
        /// </summary>
        private string? _storedProcedureNameSuffix;
        /// <summary>
        /// The sub join object name prefix value.
        /// </summary>
        private string? _subJoinObjectNamePrefix;
        /// <summary>
        /// The sub join object name suffix value.
        /// </summary>
        private string? _subJoinObjectNameSuffix;
        /// <summary>
        /// The use regions flag.
        /// </summary>
        private bool _useRegions;
        #endregion

        #region Constructor / Dispose Methods        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataProjectOptions"/> class.
        /// </summary>
        /// <remarks>
        /// This is the default constructor.
        /// </remarks>
        public SqlDataProjectOptions()
        {
            _standardNamespaces = new StringCollection();
        }
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources;
        /// <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
                _standardNamespaces?.Clear();

            _standardNamespaces = null;
            _storedProcedureNamePrefix = null;
            _storedProcedureNameSuffix = null;
            _subJoinObjectNamePrefix = null;
            _subJoinObjectNameSuffix = null;
            _entityNamespace = null;
            _dataAccessNamespace = null;
            _defaultIdField = null;
            _defaultSoftDeleteField = null;

            base.Dispose(disposing);
        }
        #endregion

        #region Public Properties        
        /// <summary>
        /// Gets or sets the namespace for the generated data access classes.
        /// </summary>
        /// <value>
        /// A string specifying the data access namespace.
        /// </value>
        public string? DataAccessNamespace { get => _dataAccessNamespace; set => _dataAccessNamespace = value; }
        /// <summary>
        /// Gets or sets the default name of the ID field for SQL tables.
        /// </summary>
        /// <value>
        /// A string containing the name of the field.
        /// </value>
        public string? DefaultIdField { get => _defaultIdField; set => _defaultIdField = value; }
        /// <summary>
        /// Gets or sets a value indicating whether soft deletes are used in the SQL Server schema.
        /// </summary>
        /// <value>
        /// <b>true</b> if records are deleted by marking them as deleted, or <b>false</b> if the SQL DELETE command is 
        /// used to delete records (hard deletes).
        /// </value>
        public bool DefaultSoftDelete { get => _defaultSoftDelete; set => _defaultSoftDelete = value; }
        /// <summary>
        /// Gets or sets the default name of the deleted field for SQL tables when using soft deletes.
        /// </summary>
        /// <value>
        /// A string containing the name of the field.
        /// </value>
        public string? DefaultSoftDeleteField { get => _defaultSoftDeleteField; set => _defaultSoftDeleteField = value; }
        /// <summary>
        /// Gets or sets the namespace for the generated entity classes.
        /// </summary>
        /// <value>
        /// A string specifying the entity classes namespace.
        /// </value>
        public string? EntityNamespace { get => _entityNamespace; set => _entityNamespace = value; }
        /// <summary>Gets or sets whether related child objects should be generated.</summary>
        public bool GenerateJoins { get; set; }
        /// <summary>
        /// Gets the reference to the list of standard namespaces to declare/import in generated classes.
        /// </summary>
        /// <value>
        /// A <see cref="StringCollection"/> containing hte list of namespaces.
        /// </value>
        public StringCollection? StandardNamespaces => _standardNamespaces;
        /// <summary>
        /// Gets or sets the value to pre-pend to stored procedure names when generating stored procedures.
        /// </summary>
        /// <value>
        /// A string containing the prefix value, or <b>null</b>, or <see cref="string.Empty"/>.
        /// </value>
        public string? StoredProcedureNamePrefix { get => _storedProcedureNamePrefix; set => _storedProcedureNamePrefix = value; }
        /// <summary>
        /// Gets or sets the value to append to stored procedure names when generating stored procedures.
        /// </summary>
        /// <value>
        /// A string containing the suffix value, or <b>null</b>, or <see cref="string.Empty"/>.
        /// </value>
        public string? StoredProcedureNameSuffix { get => _storedProcedureNameSuffix; set => _storedProcedureNameSuffix = value; }
        /// <summary>
        /// Gets or sets the value to pre-pend to object names when generating related child objects.
        /// </summary>
        /// <value>
        /// A string containing the prefix value, or <b>null</b>, or <see cref="string.Empty"/>.
        /// </value>
        public string? SubJoinObjectNamePrefix { get => _subJoinObjectNamePrefix; set => _subJoinObjectNamePrefix = value; }
        /// <summary>
        /// Gets or sets the value to pre-pend to object names when generating related child objects.
        /// </summary>
        /// <value>
        /// A string containing the suffix value, or <b>null</b>, or <see cref="string.Empty"/>.
        /// </value>
        public string? SubJoinObjectNameSuffix { get => _subJoinObjectNameSuffix; set => _subJoinObjectNameSuffix = value; }
        /// <summary>
        /// Gets or sets a value indicating whether to generate regions when generating code.
        /// </summary>
        /// <value>
        ///   <c>true</c> to generate code for region blocks; otherwise, <c>false</c>.
        /// </value>
        public bool UseRegions { get => _useRegions; set => _useRegions = value; }
        #endregion
    }
}
