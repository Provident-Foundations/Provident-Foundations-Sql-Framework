using Adaptive.SqlMaker.Project;
using Provident.Foundations.Abstractions;
using System.Xml;

namespace Adaptive.SqlMaker
{
    /// <summary>
    /// Performs the task of writing a SQL Data project to an XML file.
    /// </summary>
    /// <seealso cref="ExceptionTrackingBase" />
    public sealed class SqlDataProjectWriter : ExceptionTrackingBase
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
        private AdaptiveXmlWriter? _xmlHelper;
        #endregion

        #region Constructor / Dispose Methods        
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataProjectWriter"/> class.
        /// </summary>
        /// <remarks>
        /// This is the default constructor.
        /// </remarks>
        public SqlDataProjectWriter()
        {
            // Create the XML document and append the standard XML declaration.
            _document = new XmlDocument();
            XmlDeclaration declaration = _document.CreateXmlDeclaration(
                XmlConstants.XmlVersion,
                XmlConstants.XmlEncoding,
                XmlConstants.XmlStandAlone);
            _document.AppendChild(declaration);

            // Create the root element.
            _projectElement = _document.CreateElement(XmlConstants.ElementDatabaseProject);
            _document.AppendChild(_projectElement);

            // Create the helper instance.
            _xmlHelper = new AdaptiveXmlWriter(_document);
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

        #region Public Save Methods / Functions
        /// <summary>
        /// Writes the properties element to the XML document.
        /// </summary>
        /// <param name="projectName">
        /// A string containing the name of the project.
        /// </param>
        /// <param name="description">
        /// A string containing the description of the project.
        /// </param>
        /// <param name="defaultDataSource">
        /// The default data source object.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        public bool WritePropertiesElement(string projectName, string description, string defaultDataSource)
        {
            bool success = false;

            if (_document != null)
            {
                XmlElement propertiesElement = _document.CreateElement(XmlConstants.ElementProperties);
                CreateElementWithText(propertiesElement, XmlConstants.ElementProjectName, projectName);
                CreateElementWithText(propertiesElement, XmlConstants.ElementDescription, description);
                CreateElementWithText(propertiesElement, XmlConstants.ElementDefaultConnectionString, defaultDataSource);

                _projectElement?.AppendChild(propertiesElement);
                success = true;
            }
            return success;
        }
        /// <summary>
        /// Writes the project options object to the XML document.
        /// </summary>
        /// <param name="options">
        /// The <see cref="SqlDataProjectOptions"/> options instance with the properties to be written.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        public bool WriteProjectOptions(SqlDataProjectOptions options)
        {
            bool success = false;

            if (_document != null && _xmlHelper != null)
            {
                XmlElement element = _document.CreateElement(XmlConstants.ElementProjectOptions);

                _xmlHelper.AddAttribute(element, XmlConstants.AttributeStoredProcedureNamePrefix, options.StoredProcedureNamePrefix);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeStoredProcedureNameSuffix, options.StoredProcedureNameSuffix);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeSubJoinObjectNamePrefix, options.SubJoinObjectNamePrefix);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeSubJoinObjectNameSuffix, options.SubJoinObjectNameSuffix);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeEntityNamespace, options.EntityNamespace);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeDataAccessNamespace, options.DataAccessNamespace);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeDefaultIdField, options.DefaultIdField);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeDefaultSoftDelete, options.DefaultSoftDelete);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeDefaultSoftDeleteField, options.DefaultSoftDeleteField);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeUseRegions, options.UseRegions);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeGenerateJoins, options.GenerateJoins);
                _xmlHelper.AddStringList(element, XmlConstants.ElementStandardNamespaces, options.StandardNamespaces);

                _projectElement?.AppendChild(element);
                success = true;
            }
            return success;
        }
        /// <summary>
        /// Writes the list of data sources to the XML model.
        /// </summary>
        /// <param name="dataSourceList">
        /// A <see cref="SqlDataProjectDataSourceCollection"/> containing the list of data sources.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        public bool WriteDataSources(SqlDataProjectDataSourceCollection dataSourceList)
        {
            bool success = false;

            if (_document != null && _xmlHelper != null)
            {
                // Create the list container element.
                XmlElement element = _document.CreateElement(XmlConstants.ElementDataSources);

                // Create and append each data source element.
                success = true;
                foreach (SqlDataProjectDataSource ds in dataSourceList)
                {
                    if (!WriteDataSourceElement(ds, element))
                        return false;
                }
                _projectElement?.AppendChild(element);
            }
            return success;
        }
        /// <summary>
        /// Writes the list of tables to the XML model.
        /// </summary>
        /// <param name="tableList">
        /// The <see cref="SqlDataProjectTableCollection"/> instance containing the list of tables.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        public bool WriteTables(SqlDataProjectTableCollection tableList)
        {
            bool success = false;

            if (_document != null && _xmlHelper != null)
            {
                // Create the list container element.
                XmlElement element = _document.CreateElement(XmlConstants.ElementTables);

                // Create and append each table element.
                foreach (SqlDataProjectTable table in tableList)
                {
                    if (!WriteTable(table, element))
                        return false;
                }
                _projectElement?.AppendChild(element);
                success = true;

            }
            return success;
        }
        /// <summary>
        /// Writes the list of stored procedures to the XML model.
        /// </summary>
        /// <param name="procedureList">
        /// The <see cref="SqlDataProjectStoredProcedureCollection"/> instance containing the list of procedures.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        public bool WriteProcedures(SqlDataProjectStoredProcedureCollection procedureList)
        {
            bool success = false;
            if (_document != null && _xmlHelper != null)
            {
                XmlElement element = _document.CreateElement(XmlConstants.ElementStoredProcedures);

                foreach (SqlDataProjectStoredProcedure procedure in procedureList)
                {
                    WriteProcedure(procedure, element);
                }

                _projectElement?.AppendChild(element);
                success = true;
            }
            return success;
        }
        /// <summary>
        /// Writes the internal XML document to the specified file.
        /// </summary>
        /// <param name="fileName">
        /// A string containing the fully-qualified path and name of the file to be written.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        public bool WriteFile(string fileName)
        {
            if (_document == null)
                return false;

            string? temporaryFile = null;
            try
            {
                string destination = Path.GetFullPath(fileName);
                string directory = Path.GetDirectoryName(destination)!;
                temporaryFile = Path.Combine(directory, $".{Path.GetFileName(destination)}.{Guid.NewGuid():N}.tmp");
                using (FileStream stream = new FileStream(temporaryFile, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                    _document.Save(stream);
                File.Move(temporaryFile, destination, true);
                return true;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                Exceptions?.Add(ex);
                return false;
            }
            finally
            {
                if (temporaryFile != null && File.Exists(temporaryFile))
                {
                    try
                    {
                        File.Delete(temporaryFile);
                    }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                    {
                        Exceptions?.Add(ex);
                    }
                }
            }
        }
        #endregion

        #region Private Methods / Functions
        /// <summary>
        /// Creates and appends a new XML element entry to the specified parent element, and creates an inner text node value.
        /// </summary>
        /// <param name="parentElement">
        /// The reference to the parent <see cref="XmlElement"/> to append a new child element to.
        /// </param>
        /// <param name="newElementName">
        /// A string containing the name of the new element.
        /// </param>
        /// <param name="innerTextValue">
        /// A string containing the text to be contained by the new element.
        /// </param>
        private void CreateElementWithText(XmlElement parentElement, string newElementName, string innerTextValue)
        {
            if (_document != null)
            {
                XmlElement newElement = _document.CreateElement(newElementName);
                newElement.AppendChild(_document.CreateTextNode(innerTextValue));
                parentElement.AppendChild(newElement);
            }
        }
        /// <summary>
        /// Writes a DataSource XML element to the XML document.
        /// </summary>
        /// <param name="ds">
        /// The <see cref="SqlDataProjectDataSource"/> instance to be stored.
        /// </param>
        /// <param name="parentElement">
        /// The parent <see cref="XmlElement"/> to append to.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        private bool WriteDataSourceElement(SqlDataProjectDataSource ds, XmlElement parentElement)
        {
            bool success = false;

            if (_document != null && _xmlHelper != null)
            {
                // Create the element.
                XmlElement element = _document.CreateElement(XmlConstants.ElementDataSource);
                _xmlHelper?.AddAttribute(element, XmlConstants.AttributeName, ds.Name);

                // Add the inner text.
                XmlText innerText = _document.CreateTextNode(ds.ConnectionString);
                try
                {
                    element.AppendChild(innerText);
                    success = true;
                }
                catch(Exception ex)
                {
                    Exceptions?.Add(ex);
                }

                parentElement.AppendChild(element);
            }
            return success;
        }
        /// <summary>
        /// Writes the table to the XML model.
        /// </summary>
        /// <param name="table">
        /// The <see cref="SqlDataProjectTable"/> instance to append to the XML document.
        /// </param>
        /// <param name="tableListElement">
        /// The parent <see cref="XmlElement"/> to append the new element to.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        private bool WriteTable(SqlDataProjectTable table, XmlElement tableListElement)
        {
            bool success = false;

            if (_document != null && _xmlHelper != null)
            {
                // Create the list container element.
                XmlElement element = _document.CreateElement(XmlConstants.ElementTable);

                _xmlHelper.AddAttribute(element, XmlConstants.AttributeName, table.Name);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeDataSourceName, table.DataSourceName);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeQualifier, table.Qualifier);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeLastUpdated, table.LastUpdated);

                success = WriteProjectTableProfile(table.Profile, element);

                if (success)
                    tableListElement.AppendChild(element);
            }
            return success;
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
        private bool WriteProjectTableProfile(SqlDataProjectTableProfile? profile, XmlElement tableElement)
        {
            bool success = false;

            if (_document != null && _xmlHelper != null && profile != null)
            {
                XmlElement element = _document.CreateElement(XmlConstants.ElementProfile);

                _xmlHelper.AddAttribute(element, XmlConstants.AttributeDataAccessClassName, profile.DataAccessClassName);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeDataDefinitionClassName, profile.DataDefinitionClassName);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeDescription, profile.Description);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeFriendlyName, profile.FriendlyName);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeIdField, profile.IdField);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeLastUpdated, profile.LastUpdated);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributePluralName, profile.PluralName);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeQualifiedName, profile.QualifiedName);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeSingularName, profile.SingularName);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeStoredProcedureNamePrefix, profile.StoredProcedureNamePrefix);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeStoredProcedureNameSuffix, profile.StoredProcedureNameSuffix);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeSubJoinObjectNamePrefix, profile.SubJoinObjectNamePrefix);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeSubJoinObjectNameSuffix, profile.SubJoinObjectNameSuffix);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeTableName, profile.TableName);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeGetAllSpName, profile.GetAllSpName);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeGetByIdSpName, profile.GetByIdSpName);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeInsertSpName, profile.InsertSpName);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeUpdateSpName, profile.UpdateSpName);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeDeleteSpName, profile.DeleteSpName);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeSoftDeleteField, profile.SoftDeleteField);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeDataAccessClassDefinition, profile.DataAccessClassDefinition);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeEntityClassDefinition, profile.EntityClassDefinition);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeSoftDelete, profile.SoftDelete);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeNoSubJoins, profile.NoSubJoins);
                _xmlHelper.AddStringList(element, XmlConstants.ElementStandardQueryFields, profile.StandardQueryFields);
                _xmlHelper.AddStringList(element, XmlConstants.ElementKeyFieldNames, profile.KeyFieldNames);
                _xmlHelper.AddStringList(element, XmlConstants.ElementSubJoinQueryFields, profile.SubJoinQueryFields);

                tableElement.AppendChild(element);
                success = true;
            }
            return success;

        }
        /// <summary>
        /// Writes the stored procedure data to the XML model.
        /// </summary>
        /// <param name="procedure">
        /// The <see cref="SqlDataProjectStoredProcedure"/> instance to append to the XML document.
        /// </param>
        /// <param name="listElement">
        /// The parent <see cref="XmlElement"/> to append the new element to.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        private void WriteProcedure(SqlDataProjectStoredProcedure procedure, XmlElement listElement)
        {
            if (_document != null && _xmlHelper != null)
            {
                XmlElement element = _document.CreateElement(XmlConstants.ElementStoredProcedure);

                _xmlHelper.AddAttribute(element, XmlConstants.AttributeName, procedure.Name);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeRelatedTable, procedure.RelatedTable);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeProcType, (int)procedure.ProcedureType);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeSourceFile, procedure.SourceFile);
                _xmlHelper.AddAttribute(element, XmlConstants.AttributeLastUpdated, procedure.LastUpdated);

                listElement.AppendChild(element);
            }
        }
        #endregion

    }
}
