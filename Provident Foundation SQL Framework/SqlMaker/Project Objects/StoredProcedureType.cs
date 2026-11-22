namespace Adaptive.SqlMaker
{
    /// <summary>
    /// Lists the types of stored procedures that are currently supported.
    /// </summary>
    public enum StoredProcedureType
    {
        /// <summary>
        /// Indicates no stored procedure type specified.
        /// </summary>
        None = 0,
        /// <summary>
        /// Indicates a stored procedure to get all the records from a table.
        /// </summary>
        GetAllRecords = 1,
        /// <summary>
        /// Indicates a stored procedure to get a record by ID value.
        /// </summary>
        GetRecordById = 2,
        /// <summary>
        /// Indicates a stored procedure to insert a record.
        /// </summary>
        Insert = 3,
        /// <summary>
        /// Indicates a stored procedure to update a record.
        /// </summary>
        Update = 4,
        /// <summary>
        /// Indicates a stored procedure to delete a record.
        /// </summary>
        Delete = 5,
        /// <summary>
        /// Indicates a stored procedure that is created by the user.
        /// </summary>
        UserCustom = 100
    }
}
