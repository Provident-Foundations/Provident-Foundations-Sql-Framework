using Provident.Foundations.Collections;

namespace Provident.Foundations.SqlMaker
{
    /// <summary>
    /// Contains a list of data sources used by the project.
    /// </summary>
    /// <seealso cref="NameIndexCollection{T}" />
    public sealed class SqlDataProjectDataSourceCollection : NamedIndexCollection<SqlDataProjectDataSource>
    {
        #region Protected Method Overrides        
        /// <summary>
        /// Gets the name / key value of the specified instance.
        /// </summary>
        /// <param name="item">
        /// The <see cref="SqlDataProjectDataSource"/>  to be stored in the collection.</param>
        /// <returns>
        /// The name / key value of the specified instance.
        /// </returns>
        /// <remarks>
        /// This is called from several methods, including the Add() method, to identify the instance
        /// being added.
        /// </remarks>
        protected override string GetName(SqlDataProjectDataSource item)
        {
            return item.Name!;
        }
        #endregion
    }
}
