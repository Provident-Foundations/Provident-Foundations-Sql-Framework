using Provident.Foundations.Constants;

namespace Provident.Foundations.SqlMaker
{
    /// <summary>
    /// Provides the standard constants and static methods for the application.
    /// </summary>
    public static class MakerConstants
    {
        #region Public Constants
        /// <summary>
        /// The standard date format string to use.
        /// </summary>
        public const string DateFormatString = "MM/dd/yyyy hh:mm:ss tt";

        /// <summary>
        /// The default project description.
        /// </summary>
        public const string DefaultDescription = "Default Database Project";
        /// <summary>
        /// The default project name.
        /// </summary>
        public const string DefaultProjectName = "Database Project 1";
        /// <summary>
        /// The default project file name.
        /// </summary>
        public const string DefaultFileName = "DatabaseProject1.adbx";
        #endregion

        #region Public Static Methods / Functions        
        /// <summary>
        /// Creates the default name of the file.
        /// </summary>
        /// <param name="currentPath">
        /// A string containing the path to use.
        /// </param>
        /// <returns>
        /// A string containing the default path and name of a new database project file.
        /// </returns>
        public static string CreateDefaultFileName(string currentPath)
        {
            if (currentPath.EndsWith(CharacterConstants.Backslash))
                return currentPath + DefaultFileName;
            else
                return currentPath + CharacterConstants.Backslash + DefaultFileName;
        }
        /// <summary>
        /// Converts the specified date/time value to a string representation.
        /// </summary>
        /// <param name="date">
        /// The <see cref="DateTime"/> value to be translated.
        /// </param>
        /// <returns>
        /// A string representing the date in using the <see cref="DateFormatString"/> format.
        /// </returns>
        public static string DateToString(DateTime date)
        {
            return date.ToString("O", System.Globalization.CultureInfo.InvariantCulture);
        }
        #endregion
    }
}
