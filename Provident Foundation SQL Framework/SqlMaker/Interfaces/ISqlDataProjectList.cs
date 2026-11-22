using System.Collections;

namespace Adaptive.SqlMaker.Interfaces
{
    /// <summary>
    /// Provides the signature definition for a list container for objects that constitute a SQL Maker database project.
    /// </summary>
    /// <seealso cref="IDisposable" />
    public interface ISqlDataProjectList : IList
    {
        #region Methods / Functions        
        /// <summary>
        /// Loads the contents of the object from the specified reader instance.
        /// </summary>
        /// <param name="reader">
        /// The <see cref="SqlDataProjectReader"/> reader instance.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        bool Load(SqlDataProjectReader reader);
        /// <summary>
        /// Writes the contents of the object to the specified writer instance.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="SqlDataProjectWriter"/> reader instance.
        /// </param>
        /// <returns>
        /// <b>true</b> if the operation is successful; otherwise, returns <b>false</b>.
        /// </returns>
        bool Save(SqlDataProjectWriter writer);
        #endregion
    }
}