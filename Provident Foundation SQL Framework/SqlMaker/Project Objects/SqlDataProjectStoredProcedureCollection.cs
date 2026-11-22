using Provident.Foundations.Collections;

namespace Adaptive.SqlMaker
{
    /// <summary>
    /// Contains a list of <see cref="SqlDataProjectStoredProcedure"/> instances.
    /// </summary>
    /// <seealso cref="NameIndexCollection{T}" />
    public sealed class SqlDataProjectStoredProcedureCollection : NamedIndexCollection <SqlDataProjectStoredProcedure>
    {
        /// <summary>
        /// Gets the name / key value of the specified instance.
        /// </summary>
        /// <param name="item">
        /// The <see cref="SqlDataProjectStoredProcedure"/> to be stored in the collection.</param>
        /// <returns>
        /// The name / key value of the specified instance.
        /// </returns>
        /// <remarks>
        /// This is called from several methods, including the Add() method, to identify the instance
        /// being added.
        /// </remarks>
        protected override string GetName(SqlDataProjectStoredProcedure item)
        {
            return item.Name!;
        }
    }
}
