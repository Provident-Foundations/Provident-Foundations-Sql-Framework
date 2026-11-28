using Provident.Foundations.SqlMaker.Project;
using Provident.Foundations.Abstractions;
using System.Xml;
using Provident.Foundations.Sql.SqlMaker.Constants;

namespace Provident.Foundations.SqlMaker
{
    /// <summary>
    /// Performs the task of reading a SQL Data project from an XML file.
    /// </summary>
    /// <seealso cref="ExceptionTrackingBase" />
    public sealed class SqlDataProjectReader : ExceptionTrackingBase
    {
        #region Private Member Declarations        
        /// <summary>
        /// The XML document object.
        /// </summary>
        private XmlDocument? _document;
        /// <summary>
        /// The root element.
        /// </summary>
        private XmlElement? _projectElement;
        /// <summary>
        /// The XML helper instance.
        /// </summary>
        private AdaptiveXmlReader? _xmlHelper;
        #endregion

        #region Constructor / Dispose Methods        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataProjectReader"/> class.
        /// </summary>
        /// <remarks>
        /// This is the default constructor.
        /// </remarks>
        public SqlDataProjectReader()
        {
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
                _xmlHelper?.Dispose();
                if (_document != null)
                {
                    _document.RemoveAll();
                    GC.Collect();
                }
            }

            _xmlHelper = null;
            _projectElement = null;
            _document = null;
            base.Dispose(disposing);
        }
        #endregion

        #region Public LOad Methods / Functions
        /// <summary>
        /// Attempts to read the XML model from the specified file.
        /// </summary>
        /// <param name="fileName">
        /// A string containing the fully-qualified path and name of the file to be written.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        public bool ReadFile(string fileName)
        {
            _xmlHelper?.Dispose();
            _xmlHelper = null;
            _document = null;
            _projectElement = null;
            try
            {
                var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null };
                using var reader = XmlReader.Create(fileName, settings);
                var document = new XmlDocument { XmlResolver = null };
                document.Load(reader);
                if (document.DocumentElement?.Name != XmlConstants.ElementDatabaseProject)
                    throw new XmlException("The file is not a SQL Maker database project.");
                _document = document;
                _projectElement = document.DocumentElement;
                _xmlHelper = new AdaptiveXmlReader(document);
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or XmlException or ArgumentException)
            {
                Exceptions?.Add(ex);
                return false;
            }
        }
        /// <summary>
        /// Read the properties of the project from the Properties XML element.
        /// </summary>
        /// <param name="project">
        /// The <see cref="SqlDataProject"/> being loaded.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        public bool ReadPropertiesElement(SqlDataProject project)
        {
            bool success = false;

            if (_document != null && _projectElement != null)
            {
                // Find the properties element.
                XmlElement? propertiesElement = FindElement(_projectElement, XmlConstants.ElementProperties);
                if (propertiesElement != null)
                {
                    XmlElement? projectNameElement = FindElement(propertiesElement, XmlConstants.ElementProjectName);
                    XmlElement? descriptionElement = FindElement(propertiesElement, XmlConstants.ElementDescription);
                    XmlElement? defaultDsElement = FindElement(propertiesElement, XmlConstants.ElementDefaultConnectionString);

                    success = (projectNameElement != null && descriptionElement != null && defaultDsElement != null);
                    if (success)
                    {
                        if (projectNameElement != null)
                            project.ProjectName = projectNameElement.InnerText;

                        if (descriptionElement != null)
                            project.Description = descriptionElement.InnerText;

                        if (defaultDsElement != null)
                            project.SetDefaultDataSourceConnectionString(defaultDsElement.InnerText);
                    }
                }
            }
            return success;
        }
        /// <summary>
        /// Reads the project options from the XML model.
        /// </summary>
        /// <param name="options">
        /// The <see cref="SqlDataProjectOptions"/> options instance to be populated.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        public bool ReadProjectOptions(SqlDataProjectOptions options)
        {
            bool success = false;

            if (_document != null && _xmlHelper != null && _projectElement != null)
            {
                XmlElement? element = FindElement(_projectElement, XmlConstants.ElementProjectOptions);
                if (element != null)
                {
                    options.StoredProcedureNamePrefix = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeStoredProcedureNamePrefix);
                    options.StoredProcedureNameSuffix = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeStoredProcedureNameSuffix);
                    options.SubJoinObjectNamePrefix = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeSubJoinObjectNamePrefix);
                    options.SubJoinObjectNameSuffix = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeSubJoinObjectNameSuffix);
                    options.EntityNamespace = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeEntityNamespace);
                    options.DataAccessNamespace = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeDataAccessNamespace);
                    options.DefaultIdField = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeDefaultIdField);
                    options.DefaultSoftDelete = _xmlHelper.ReadAttributeBoolean(element, XmlConstants.AttributeDefaultSoftDelete);
                    options.DefaultSoftDeleteField = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeDefaultSoftDeleteField);
                    options.UseRegions = _xmlHelper.ReadAttributeBoolean(element, XmlConstants.AttributeUseRegions);
                    options.GenerateJoins = _xmlHelper.ReadAttributeBoolean(element, XmlConstants.AttributeGenerateJoins);
                    options.StandardNamespaces?.Clear();
                    options.StandardNamespaces?.AddRange(_xmlHelper.ReadStringList(element, XmlConstants.ElementStandardNamespaces).ToArray());
                    success = true;
                }
            }
            return success;
        }
        /// <summary>
        /// Reads the list of data sources from the XML model.
        /// </summary>
        /// <param name="dataSourceList">
        /// A <see cref="SqlDataProjectDataSourceCollection"/> to be populated.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        public bool ReadDataSources(SqlDataProjectDataSourceCollection dataSourceList)
        {
            bool success = false;

            if (_document != null && _xmlHelper != null && _projectElement != null)
            {
                // Find the data sources list element.
                XmlElement? element = FindElement(_projectElement, XmlConstants.ElementDataSources);
                if (element != null)
                {
                    // For each child element, read and translate the item.
                    foreach (XmlElement dsElement in element.ChildNodes.OfType<XmlElement>())
                    {
                        SqlDataProjectDataSource? ds = ReadDataSourceElement(dsElement);
                        if (ds != null)
                        {
                            if (string.IsNullOrEmpty(ds.Name) || dataSourceList.Contains(ds.Name))
                            {
                                Exceptions?.Add(new InvalidDataException("A data source has a missing or duplicate name."));
                                return false;
                            }
                            dataSourceList.Add(ds);
                        }
                    }
                }
                success = true;
            }
            return success;
        }
        /// <summary>
        /// Reads the list of tables from the XML model.
        /// </summary>
        /// <param name="tableList">
        /// The <see cref="SqlDataProjectTableCollection"/> instance to be populated.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        public bool ReadTables(SqlDataProjectTableCollection tableList)
        {
            bool success = false;

            if (_document != null && _xmlHelper != null && _projectElement != null)
            {
                XmlElement? element = FindElement(_projectElement, XmlConstants.ElementTables);
                IEnumerable<XmlElement> tableElements = element == null
                    ? Enumerable.Empty<XmlElement>() : element.ChildNodes.OfType<XmlElement>();
                // Older writers placed tables directly under the project root.
                tableElements = tableElements.Concat(_projectElement.ChildNodes.OfType<XmlElement>()
                    .Where(child => child.Name == XmlConstants.ElementTable));
                foreach (XmlElement tableElement in tableElements)
                {
                    SqlDataProjectTable? table = ReadTable(tableElement);
                    if (table == null)
                        continue;
                    if (string.IsNullOrEmpty(table.Name) || tableList.Contains(table.Qualifier, table.Name))
                    {
                        Exceptions?.Add(new InvalidDataException("A table has a missing name or duplicate schema-qualified name."));
                        return false;
                    }
                    tableList.Add(table);
                }
                success = true;
            }
            return success;
        }
        /// <summary>
        /// Reads the list of stored procedures from the XML model.
        /// </summary>
        /// <param name="procedureList">
        /// The <see cref="SqlDataProjectStoredProcedureCollection"/> instance to be populated.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        public bool ReadProcedures(SqlDataProjectStoredProcedureCollection procedureList)
        {
            bool success = false;

            if (_document != null && _xmlHelper != null && _projectElement != null)
            {
                // Find the data sources list element.
                XmlElement? element = FindElement(_projectElement, XmlConstants.ElementStoredProcedures);
                if (element != null)
                {
                    // For each child element, read and translate the item.
                    foreach (XmlElement dsElement in element.ChildNodes.OfType<XmlElement>())
                    {
                        SqlDataProjectStoredProcedure? proc = ReadProcedure(dsElement);
                        if (proc != null)
                        {
                            if (string.IsNullOrEmpty(proc.Name) || procedureList.Contains(proc.Name))
                            {
                                Exceptions?.Add(new InvalidDataException("A stored procedure has a missing or duplicate name."));
                                return false;
                            }
                            procedureList.Add(proc);
                        }
                    }
                }
                success = true;
            }
            return success;
        }
        #endregion

        #region Private Methods / Functions
        /// <summary>
        /// Reads the content of the inner text element of the specified XML element.
        /// </summary>
        /// <param name="parentElement">
        /// The reference to the parent <see cref="XmlElement"/> to read the inner value of.
        /// </param>
        /// <returns>
        /// A string containing the inner text value, or <b>null</b>.
        /// </returns>
        private string? ReadElementInnerText(XmlElement? parentElement)
        {
            return parentElement?.InnerText;
        }
        /// <summary>
        /// Read a DataSource instance from the XML model.
        /// </summary>
        /// <param name="dsElement">
        /// The <see cref="XmlElement"/> to be read from.
        /// </param>
        /// <returns>
        /// The <see cref="SqlDataProjectDataSource"/> instance, or <b>null</b>.
        /// </returns>
        private SqlDataProjectDataSource? ReadDataSourceElement(XmlElement dsElement)
        {
            SqlDataProjectDataSource? dataSource = null;    

            if (_document != null && _xmlHelper != null && dsElement.Name == XmlConstants.ElementDataSource)
            {
                // Create the element.
                dataSource = new SqlDataProjectDataSource();
                dataSource.Name = _xmlHelper.ReadAttributeString(dsElement, XmlConstants.AttributeName);
                dataSource.ConnectionString = ReadElementInnerText(dsElement);
            }

            return dataSource;
        }
        /// <summary>
        /// Reads the SQl table from the XML Model.
        /// </summary>
        /// <param name="tableElement">
        /// The <see cref="XmlElement"/> to extract the data from.
        /// </param>
        /// <returns>
        /// A <see cref="SqlDataProjectTable"/> instance, or <b>null</b>.
        /// </returns>
        private SqlDataProjectTable? ReadTable(XmlElement tableElement)
        {
            SqlDataProjectTable? table = null;

            if (_document != null && _xmlHelper != null && tableElement.Name == XmlConstants.ElementTable)
            {
                // Create the list container element.
                table = new SqlDataProjectTable
                {
                    Name = _xmlHelper.ReadAttributeString(tableElement, XmlConstants.AttributeName),
                    DataSourceName = _xmlHelper.ReadAttributeString(tableElement, XmlConstants.AttributeDataSourceName),
                    Qualifier = _xmlHelper.ReadAttributeString(tableElement, XmlConstants.AttributeQualifier),
                    LastUpdated = _xmlHelper.ReadAttributeDateTime(tableElement, XmlConstants.AttributeLastUpdated)
                };

                ReadTableProfile(table.Profile, tableElement);
            }
            return table;
        }
        /// <summary>
        /// Writes the table profile object to the XML model.
        /// </summary>
        /// <param name="profile">
        /// The <see cref="SqlDataProjectTableProfile"/> instance to append to the XML document.
        /// </param>
        /// <param name="tableElement">
        /// The parent <see cref="XmlElement"/> to append the new element to.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        private void ReadTableProfile(SqlDataProjectTableProfile? profile, XmlElement tableElement)
        {
            if (_document != null && _xmlHelper != null && profile != null && tableElement != null)
            {
                XmlElement? element = FindElement(tableElement, XmlConstants.ElementProfile);
                if (element != null)
                {
                    profile.DataAccessClassName = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeDataAccessClassName);
                    profile.DataDefinitionClassName = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeDataDefinitionClassName);
                    profile.Description = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeDescription);
                    profile.FriendlyName = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeFriendlyName);
                    profile.IdField = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeIdField);
                    profile.LastUpdated = _xmlHelper.ReadAttributeDateTime(element, XmlConstants.AttributeLastUpdated);
                    profile.PluralName = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributePluralName);
                    profile.QualifiedName = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeQualifiedName);
                    profile.SingularName = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeSingularName);
                    profile.StoredProcedureNamePrefix = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeStoredProcedureNamePrefix);
                    profile.StoredProcedureNameSuffix = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeStoredProcedureNameSuffix);
                    profile.SubJoinObjectNamePrefix = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeSubJoinObjectNamePrefix);
                    profile.SubJoinObjectNameSuffix = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeSubJoinObjectNameSuffix);
                    profile.TableName = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeTableName);
                    profile.GetAllSpName = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeGetAllSpName);
                    profile.GetByIdSpName = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeGetByIdSpName);
                    profile.InsertSpName = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeInsertSpName);
                    profile.UpdateSpName = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeUpdateSpName);
                    profile.DeleteSpName = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeDeleteSpName);
                    profile.SoftDeleteField = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeSoftDeleteField);
                    profile.DataAccessClassDefinition = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeDataAccessClassDefinition);
                    profile.EntityClassDefinition = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeEntityClassDefinition);
                    profile.SoftDelete = _xmlHelper.ReadAttributeBoolean(element, XmlConstants.AttributeSoftDelete);
                    profile.NoSubJoins = _xmlHelper.ReadAttributeBoolean(element, XmlConstants.AttributeNoSubJoins);
                    XmlElement? listElement = FindElement(element, XmlConstants.ElementStandardQueryFields);
                    if (listElement != null)
                        profile.StandardQueryFields?.AddRange(_xmlHelper.ReadStringList(listElement).ToArray());

                    listElement = FindElement(element, XmlConstants.ElementKeyFieldNames);
                    if (listElement != null)
                        profile.KeyFieldNames?.AddRange(_xmlHelper.ReadStringList(listElement).ToArray());

                    listElement = FindElement(element, XmlConstants.ElementSubJoinQueryFields);
                    if (listElement != null)
                        profile.SubJoinQueryFields?.AddRange(_xmlHelper.ReadStringList(listElement).ToArray());
                }
            }
        }
        /// <summary>
        /// Reads the stored procedure data from the XML model.
        /// </summary>
        /// <param name="element">
        /// The parent <see cref="XmlElement"/> to be read.
        /// </param>
        /// <returns>
        /// A <see cref="SqlDataProjectStoredProcedure"/> read from the data, or <b>null</b>.
        /// </returns>
        private SqlDataProjectStoredProcedure? ReadProcedure(XmlElement element)
        {
            SqlDataProjectStoredProcedure? procedure = null;

            if (_document != null && _xmlHelper != null && element.Name == XmlConstants.ElementStoredProcedure)
            {
                procedure = new SqlDataProjectStoredProcedure();

                procedure.Name = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeName);
                procedure.RelatedTable = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeRelatedTable);
                procedure.ProcedureType = (StoredProcedureType) _xmlHelper.ReadAttributeInt32(element, XmlConstants.AttributeProcType);
                procedure.SourceFile = _xmlHelper.ReadAttributeString(element, XmlConstants.AttributeSourceFile);
                procedure.LastUpdated = _xmlHelper.ReadAttributeDateTime(element, XmlConstants.AttributeLastUpdated);
            }
            return procedure;
        }
        /// <summary>
        /// Finds the first child element with the specified name.
        /// </summary>
        /// <param name="parentElement">
        /// The parent <see cref="XmlElement"/> instance to be checked.
        /// </param>
        /// <param name="name">
        /// A string containing the name to search for.
        /// </param>
        /// <returns>
        /// The first <see cref="XmlElement"/> matching the name, or <b>null</b>.
        /// </returns>
        private XmlElement? FindElement(XmlElement parentElement, string name)
        {
            return parentElement.ChildNodes.OfType<XmlElement>()
                .FirstOrDefault(element => string.Equals(element.Name, name, StringComparison.Ordinal));
        }
        #endregion

    }
}
