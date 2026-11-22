using Provident.Foundations.Abstractions;
using System.Collections.Specialized;

namespace Adaptive.SqlMaker
{
    /// <summary>
    /// Represents and manages the profile information for a database table that is included in the project.
    /// </summary>
    /// <seealso cref="ExceptionTrackingBase" />
    public sealed class SqlDataProjectTableProfile : ExceptionTrackingBase
    {
        #region Private Member Declarations        
        /// <summary>
        /// The data access class definition file name.
        /// </summary>
        private string? _dataAccessClassDefinition;
        /// <summary>
        /// The data access class name to create.
        /// </summary>
        private string? _dataAccessClassName;
        /// <summary>
        /// The data definition / entity class name.
        /// </summary>
        private string? _dataDefinitionClassName;
        /// <summary>
        /// The delete record  SP name.
        /// </summary>
        private string? _deleteSpName;
        /// <summary>
        /// The description.
        /// </summary>
        private string? _description;
        /// <summary>
        /// The entity class definition file name.
        /// </summary>
        private string? _entityClassDefinition;
        /// <summary>
        /// The friendly name.
        /// </summary>
        private string? _friendlyName;
        /// <summary>
        /// The get all records SP name.
        /// </summary>
        private string? _getAllSpName;
        /// <summary>
        /// The get record by ID SP name.
        /// </summary>
        private string? _getByIdSpName;
        /// <summary>
        /// The ID field name.
        /// </summary>
        private string? _idField;
        /// <summary>
        /// The insert record SP name.
        /// </summary>
        private string? _insertSpName;
        /// <summary>
        /// The key field names list.
        /// </summary>
        private StringCollection? _keyFieldNames;
        /// <summary>
        /// The date/time the profile was last updated.
        /// </summary>
        private DateTime? _lastUpdated;
        /// <summary>
        /// The no sub joins flag.
        /// </summary>
        private bool _noSubJoins;
        /// <summary>
        /// The plural name.
        /// </summary>
        private string? _pluralName;
        /// <summary>
        /// The qualified name.
        /// </summary>
        private string? _qualifiedName;
        /// <summary>
        /// The singular name.
        /// </summary>
        private string? _singularName;
        /// <summary>
        /// The soft delete flag.
        /// </summary>
        private bool _softDelete;
        /// <summary>
        /// The name of the "soft" delete field.
        /// </summary>
        private string? _softDeleteField;
        /// <summary>
        /// The standard query fields list.
        /// </summary>
        private StringCollection? _standardQueryFields;
        /// <summary>
        /// The stored procedure name prefix.
        /// </summary>
        private string? _storedProcedureNamePrefix;
        /// <summary>
        /// The stored procedure name suffix.
        /// </summary>
        private string? _storedProcedureNameSuffix;
        /// <summary>
        /// The join-based child object name prefix.
        /// </summary>
        private string? _subJoinObjectNamePrefix;
        /// <summary>
        /// The join-based child object name suffix.
        /// </summary>
        private string? _subJoinObjectNameSuffix;
        /// <summary>
        /// The sub join query fields list.
        /// </summary>
        private StringCollection? _subJoinQueryFields;
        /// <summary>
        /// The table name.
        /// </summary>
        private string? _tableName;
        /// <summary>
        /// The update record SP name.
        /// </summary>
        private string? _updateSpName;
        #endregion

        #region Constructor / Dispose Methods        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataProjectTableProfile"/> class.
        /// </summary>
        /// <remarks>
        /// This is the default constructor.
        /// </remarks>
        public SqlDataProjectTableProfile()
        {
            _standardQueryFields = new StringCollection();
            _keyFieldNames = new StringCollection();
            _subJoinQueryFields = new StringCollection();

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
                _keyFieldNames?.Clear();
                _standardQueryFields?.Clear();
                _subJoinQueryFields?.Clear();
            }

            _dataAccessClassDefinition = null;
            _dataAccessClassName = null;
            _dataDefinitionClassName = null;
            _deleteSpName = null;
            _description = null;
            _entityClassDefinition = null;
            _friendlyName = null;
            _getAllSpName = null;
            _getByIdSpName = null;
            _idField = null;
            _insertSpName = null;
            _pluralName = null;
            _qualifiedName = null;
            _singularName = null;
            _softDeleteField = null;
            _storedProcedureNamePrefix = null;
            _storedProcedureNameSuffix = null;
            _subJoinObjectNamePrefix = null;
            _subJoinObjectNameSuffix = null;
            _tableName = null;
            _updateSpName = null;

            _standardQueryFields = null;
            _keyFieldNames = null;
            _subJoinQueryFields = null;
            _lastUpdated = null;

            base.Dispose(disposing);
        }
        #endregion

        #region Public Properties        
        /// <summary>
        /// Gets or sets the path and file name of the generated data access class file.
        /// </summary>
        /// <value>
        /// A string containing the fully-qualified path and name of the C# code file that
        /// contains the generated and/or edited Data Access class for this table.
        /// </value>
        public string? DataAccessClassDefinition
        {
            get => _dataAccessClassDefinition; set => _dataAccessClassDefinition = value;
        }
        /// <summary>
        /// Gets or sets the name of the data access class to generate from the table definition.
        /// </summary>
        /// <value>
        /// A string containing the name of the data access class to be created when generating code.
        /// </value>
        public string? DataAccessClassName
        {
            get => _dataAccessClassName; set => _dataAccessClassName = value;
        }
        /// <summary>
        /// Gets or sets the name of the entity class to generate from the table definition.
        /// </summary>
        /// <value>
        /// A string containing the name of the data definition / entity class to be created when generating code.
        /// </value>
        public string? DataDefinitionClassName
        {
            get => _dataDefinitionClassName; set => _dataDefinitionClassName = value;
        }
        /// <summary>
        /// Gets or sets the base name of the stored procedure used to 
        /// delete a record from the table, or to mark a record as deleted.
        /// </summary>
        /// <value>
        /// A string containing the base name of the stored procedure to generate.
        /// </value>
        public string? DeleteSpName
        {
            get => _deleteSpName; set => _deleteSpName = value;
        }
        /// <summary>
        /// Gets or sets the description text.
        /// </summary>
        /// <value>
        /// A string containing the description.
        /// </value>
        public string? Description
        {
            get => _description; set => _description = value;
        }
        /// <summary>
        /// Gets or sets the path and file name of the generated data definition or entity class file.
        /// </summary>
        /// <value>
        /// A string containing the fully-qualified path and name of the C# code file that
        /// contains the generated and/or edited Data definition or entity class for this table.
        /// </value>
        public string? EntityClassDefinition
        {
            get => _entityClassDefinition; set => _entityClassDefinition = value;
        }
        /// <summary>
        /// Gets or sets a friendly name for the table.
        /// </summary>
        /// <value>
        /// A string containing the friendly name value.
        /// </value>
        public string? FriendlyName
        {
            get => _friendlyName; set => _friendlyName = value;
        }
        /// <summary>
        /// Gets or sets the base name of the stored procedure used to 
        /// read all records from the table.
        /// </summary>
        /// <value>
        /// A string containing the base name of the stored procedure to generate.
        /// </value>
        public string? GetAllSpName
        {
            get => _getAllSpName; set => _getAllSpName = value;
        }
        /// <summary>
        /// Gets or sets the base name of the stored procedure used to 
        /// read a record from the table by ID value.
        /// </summary>
        /// <value>
        /// A string containing the base name of the stored procedure to generate.
        /// </value>
        public string? GetByIdSpName
        {
            get => _getByIdSpName; set => _getByIdSpName = value;
        }
        /// <summary>
        /// Gets or sets the name of the table's identity field/column.
        /// </summary>
        /// <value>
        /// A string containing the field name.
        /// </value>
        public string? IdField
        {
            get => _idField; set => _idField = value;
        }
        /// <summary>
        /// Gets or sets the base name of the stored procedure used to 
        /// insert a record into the table.
        /// </summary>
        /// <value>
        /// A string containing the base name of the stored procedure to generate.
        /// </value>
        public string? InsertSpName
        {
            get => _insertSpName; set => _insertSpName = value;
        }

        /// <summary>
        /// Gets the reference to the list of standard key field/column names.
        /// </summary>
        /// <value>
        /// A <see cref="StringCollection"/> containing the list of standard key fields/columns.
        /// </value>
        public StringCollection? KeyFieldNames => _keyFieldNames;
        /// <summary>
        /// Gets or sets the date/time this table profile was last updated / refreshed.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> specifying the last date/time the item was updated.
        /// </value>
        public DateTime? LastUpdated
        {
            get => _lastUpdated; set => _lastUpdated = value;
        }
        /// <summary>
        /// Gets or sets a value indicating whether to ignore relationships and joins in generating child objects.
        /// </summary>
        /// <value>
        ///   <c>true</c> to generate no child-objects from relationships and joins; 
        ///   otherwise, <b>false</b>.
        /// </value>
        public bool NoSubJoins
        {
            get => _noSubJoins; set => _noSubJoins = value;
        }
        /// <summary>
        /// Gets or sets the plural objects name for the table.
        /// </summary>
        /// <value>
        /// A string containing the table name when referred to as multiple objects.
        /// Example: A "Person" table would be "Persons"; A "Customers" table would be "Customers", 
        ///          a "History" table would be "Histories", etc.
        /// </value>
        public string? PluralName
        {
            get => _pluralName; set => _pluralName = value;
        }
        /// <summary>
        /// Gets or sets the qualified table name.
        /// </summary>
        /// <value>
        /// A string containing the qualified name of the table (such as "dbo.TableName");
        /// </value>
        public string? QualifiedName
        {
            get => _qualifiedName; set => _qualifiedName = value;
        }
        /// <summary>
        /// Gets or sets the single object name for the table.
        /// </summary>
        /// <value>
        /// A string containing the table name when referred to as a single object.
        /// Example: A "Person" table would be "Person"; A "Customers" table would be "Customer", 
        ///          a "...History" table would be "...History", etc.
        /// </value>
        public string? SingularName
        {
            get => _singularName; set => _singularName = value;
        }
        /// <summary>
        /// Gets or sets a value indicating whether soft deletes are used in the database.
        /// </summary>
        /// <remarks>
        /// This value has a direct effect on the code that is generated.
        /// </remarks>
        /// <value>
        ///   <b>true</b> if soft deletes are used on this table; otherwise, <b>false</b>.
        /// </value>
        public bool SoftDelete
        {
            get => _softDelete; set => _softDelete = value;
        }
        /// <summary>
        /// Gets or sets the name of the soft delete field/column.
        /// </summary>
        /// <value>
        /// When using soft deletes, this is the name of the boolean field that marks a record
        /// as deleted or non-deleted.
        /// </value>
        public string? SoftDeleteField
        {
            get => _softDeleteField; set => _softDeleteField = value;
        }
        /// <summary>
        /// Gets the reference to the list of standard query fields.
        /// </summary>
        /// <value>
        /// A <see cref="StringCollection"/> containing the list of standard query fields/columns.
        /// </value>
        public StringCollection? StandardQueryFields => _standardQueryFields;
        /// <summary>
        /// Gets or sets the stored procedure name prefix.
        /// </summary>
        /// <value>
        /// A string to pre-pend to stored procedure names when generated.
        /// </value>
        public string? StoredProcedureNamePrefix
        {
            get => _storedProcedureNamePrefix; set => _storedProcedureNamePrefix = value;
        }
        /// <summary>
        /// Gets or sets the stored procedure name suffix.
        /// </summary>
        /// <value>
        /// A string to append to stored procedure names when generated.
        /// </value>
        public string? StoredProcedureNameSuffix
        {
            get => _storedProcedureNameSuffix; set => _storedProcedureNameSuffix = value;
        }
        /// <summary>
        /// Gets or sets the sub-join child object name prefix.
        /// </summary>
        /// <value>
        /// When generating child objects from relationships on the table, the class names will
        /// be pre-pended with this value.
        /// </value>
        public string? SubJoinObjectNamePrefix
        {
            get => _subJoinObjectNamePrefix; set => _subJoinObjectNamePrefix = value;
        }
        /// <summary>
        /// Gets or sets the sub-join child object name suffix.
        /// </summary>
        /// <value>
        /// When generating child objects from relationships on the table, the class names will
        /// be appended with this value.
        /// </value>
        public string? SubJoinObjectNameSuffix
        {
            get => _subJoinObjectNameSuffix; set => _subJoinObjectNameSuffix = value;
        }
        /// <summary>
        /// Gets the reference to the list of standard relational field/column names used in JOINs.
        /// </summary>
        /// <value>
        /// A <see cref="StringCollection"/> containing the list of standard relational field/column names used in JOINs.
        /// </value>
        public StringCollection? SubJoinQueryFields => _subJoinQueryFields;
        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// a string containing the name of the table.
        /// </value>
        public string? TableName
        {
            get => _tableName; set => _tableName = value;
        }
        /// <summary>
        /// Gets or sets the base name of the stored procedure used to 
        /// update a record in the table.
        /// </summary>
        /// <value>
        /// A string containing the base name of the stored procedure to generate.
        /// </value>
        public string? UpdateSpName
        {
            get => _updateSpName; set => _updateSpName = value;
        }
        #endregion
    }
}
