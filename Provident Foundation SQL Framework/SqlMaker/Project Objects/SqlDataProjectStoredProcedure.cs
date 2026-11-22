using Provident.Foundations.Abstractions;

namespace Adaptive.SqlMaker
{
    /// <summary>
    /// Represents and contains the information for a generated stored procedure.
    /// </summary>
    /// <seealso cref="ExceptionTrackingBase" />
    public sealed class SqlDataProjectStoredProcedure : ExceptionTrackingBase
    {
        #region Private Member Declarations        
        /// <summary>
        /// The name.
        /// </summary>
        private string? _name;
        /// <summary>
        /// The related table name.
        /// </summary>
        private string? _relatedTable;
        /// <summary>
        /// The type.
        /// </summary>
        private StoredProcedureType _type = StoredProcedureType.None;
        /// <summary>
        /// The source file.
        /// </summary>
        private string? _sourceFile;
        /// <summary>
        /// The last updated date/time.
        /// </summary>
        private DateTime? _lastUpdated;
        #endregion

        #region Constructor / Dispose Methods        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataProjectStoredProcedure"/> class.
        /// </summary>
        /// <remarks>
        /// This is the default constructor.
        /// </remarks>
        public SqlDataProjectStoredProcedure()
        {
        }
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources;
        /// <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            _name = null;
            _relatedTable = null;
            _lastUpdated = null;
            _sourceFile = null;
            base.Dispose(disposing);
        }
        #endregion

        #region Public Properties        
        /// <summary>
        /// Gets or sets the stored procedure name.
        /// </summary>
        /// <value>
        /// A string containing the name of the stored procedure.
        /// </value>
        public string? Name
        {
            get => _name;
            set => _name = value;
        }
        /// <summary>
        /// Gets or sets the type of stored procedure.
        /// </summary>
        /// <value>
        /// A <see cref="StoredProcedureType"/> enumerated value indicating the procedure type.
        /// </value>
        public StoredProcedureType ProcedureType
        {
            get => _type;
            set => _type = value;
        }
        /// <summary>
        /// Gets or sets the name of the table the procedure is related to / works on.
        /// </summary>
        /// <value>
        /// A string containing the name of the table.
        /// </value>
        public string? RelatedTable
        {
            get => _relatedTable;
            set => _relatedTable = value;
        }
        /// <summary>
        /// Gets or sets the path and name of the source file.
        /// </summary>
        /// <value>
        /// A string containing the path and name of the saved SQL file that contains the stored procedure.
        /// </value>
        public string? SourceFile
        {
            get => _sourceFile;
            set => _sourceFile = value;
        }
        /// <summary>
        /// Gets or sets the date/time the procedure was last updated.
        /// </summary>
        /// <value>
        /// The <see cref="DateTime"/> the procedure definition was last updated or refreshed.
        /// </value>
        public DateTime? LastUpdated
        {
            get => _lastUpdated;
            set => _lastUpdated = value;
        }
        #endregion
    }
}

