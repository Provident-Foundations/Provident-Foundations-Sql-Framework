namespace Adaptive.SqlMaker
{
    /// <summary>
    /// Provides the standard constants for reading/writing the Database Project XML file(s).
    /// </summary>
    public static class XmlConstants
    {
        #region Attribute Names        
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeDataAccessClassDefinition = "dataAccessClassDefinition";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeDataAccessClassName = "dataAccessClassName";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeDataAccessNamespace = "dataAccessNamespace";
        /// <summary>The generate related objects option.</summary>
        public const string AttributeGenerateJoins = "generateJoins";
        /// <summary>The standard namespaces list element.</summary>
        public const string ElementStandardNamespaces = "StandardNamespaces";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeDataDefinitionClassName = "dataDefinitionClassName";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeDataSourceName = "dataSourceName";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeDefaultIdField = "defaultIdField";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeDefaultSoftDelete = "defaultSoftDelete";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeDefaultSoftDeleteField = "defaultSoftDeleteField";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeDeleteSpName = "deleteSpName";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeDescription = "description";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeEntityClassDefinition = "entityClassDefinition";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeEntityNamespace = "entityNamespace";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeFriendlyName = "friendlyName";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeGetAllSpName = "getAllSpName";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeGetByIdSpName = "getByIdSpName";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeIdField = "idField";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeInsertSpName = "insertSpName";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeLastUpdated = "lastUpdated";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeName = "name";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeNoSubJoins = "noSubJoins";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributePluralName = "pluralName";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeProcType = "procType";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeQualifiedName = "qualifiedName";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeQualifier = "qualifier";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeRelatedTable = "relatedTable";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeSingularName = "singularName";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeSoftDelete = "softDelete";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeSoftDeleteField = "softDeleteField";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeSourceFile = "sourceFile";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeStoredProcedureNamePrefix = "storedProcedureNamePrefix";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeStoredProcedureNameSuffix = "storedProcedureNameSuffix";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeSubJoinObjectNamePrefix = "AttributeSubJoinObjectNamePrefix";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeSubJoinObjectNameSuffix = "AttributeSubJoinObjectNameSuffix";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeTableName = "tableName";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeUpdateSpName = "updateSpName";
        /// <summary>
        /// Attribute name.
        /// </summary>
        public const string AttributeUseRegions = "useRegions";
        #endregion

        #region Element Names        
        /// <summary>
        /// The DatabaseProject element name.
        /// </summary>
        public const string ElementDatabaseProject = "DatabaseProject";
        /// <summary>
        /// The DataSource element name.
        /// </summary>
        public const string ElementDataSource = "DataSource";
        /// <summary>
        /// The DataSources element name.
        /// </summary>
        public const string ElementDataSources = "DataSources";
        /// <summary>
        /// The DefaultConnectionString element name.
        /// </summary>
        public const string ElementDefaultConnectionString = "DefaultConnectionString";
        /// <summary>
        /// The Description element name.
        /// </summary>
        public const string ElementDescription = "Description";
        /// <summary>
        /// The KeyFieldNames element name.
        /// </summary>
        public const string ElementKeyFieldNames = "KeyFieldNames";
        /// <summary>
        /// The Profile element name.
        /// </summary>
        public const string ElementProfile = "Profile";
        /// <summary>
        /// The ProjectName element name.
        /// </summary>
        public const string ElementProjectName = "ProjectName";
        /// <summary>
        /// The ProjectOptions element name.
        /// </summary>
        public const string ElementProjectOptions = "ProjectOptions";
        /// <summary>
        /// The Properties element name.
        /// </summary>
        public const string ElementProperties = "Properties";
        /// <summary>
        /// The StandardQueryFields element name.
        /// </summary>
        public const string ElementStandardQueryFields = "StandardQueryFields";
        /// <summary>
        /// The StoredProcedures element name.
        /// </summary>
        public const string ElementStoredProcedures = "StoredProcedures";
        /// <summary>
        /// The StoredProcedure element name.
        /// </summary>
        public const string ElementStoredProcedure = "StoredProcedure";
        /// <summary>
        /// The SubJoinQueryFields element name.
        /// </summary>
        public const string ElementSubJoinQueryFields = "SubJoinQueryFields";
        /// <summary>
        /// The Table element name.
        /// </summary>
        public const string ElementTable = "Table";
        /// <summary>
        /// The Tables element name.
        /// </summary>
        public const string ElementTables = "Tables";
        #endregion

        #region Standard XML Values
        /// <summary>
        /// The value for the Encoding component in the XML declaration.
        /// </summary>
        public const string XmlEncoding = "utf-8";
        /// <summary>
        /// The value for the Standalone component in the XML declaration.
        /// </summary>
        public const string XmlStandAlone = "yes";
        /// <summary>
        /// The value for the Version component in the XML declaration.
        /// </summary>
        public const string XmlVersion = "1.0";
        #endregion
    }
}
