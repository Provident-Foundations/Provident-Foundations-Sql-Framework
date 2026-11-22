using System.Xml;

namespace Adaptive.SqlMaker.Interfaces.IO
{
    /// <summary>
    /// Provides the signature definition for objects used to read structured XML data from an XML Document.
    /// </summary>
    public interface IAdaptiveXmlReader : IDisposable
    {
        #region Properties        
        /// <summary>
        /// Gets or sets the reference to the XML source document being read from.
        /// </summary>
        /// <value>
        /// The <see cref="XmlDocument"/> instance loaded into memory.
        /// </value>
        XmlDocument SourceDocument { get; set; }
        #endregion

        #region Methods / Functions
        /// <summary>
        /// Reads the attribute value as a boolean value.
        /// </summary>
        /// <param name="element">
        /// The <see cref="XmlElement"/> to read the attribute from.
        /// </param>
        /// <param name="attributeName">
        /// A string containing the name of the attribute whose value is to be read.
        /// </param>
        /// <returns>
        /// A <see cref="bool"/> containing the value of the attribute.
        /// </returns>
        bool ReadAttributeBoolean(XmlElement? element, string attributeName);
        /// <summary>
        /// Reads the attribute value as a date/time value.
        /// </summary>
        /// <param name="element">
        /// The <see cref="XmlElement"/> to read the attribute from.
        /// </param>
        /// <param name="attributeName">
        /// A string containing the name of the attribute whose value is to be read.
        /// </param>
        /// <returns>
        /// A <see cref="DateTime"/> containing the value of the attribute, or <b>null</b> if the value is not present or 
        /// cannot be parsed.
        /// </returns>
        DateTime? ReadAttributeDateTime(XmlElement? element, string attributeName);
        /// <summary>
        /// Reads the attribute value as a string.
        /// </summary>
        /// <param name="element">
        /// The <see cref="XmlElement"/> to read the attribute from.
        /// </param>
        /// <param name="attributeName">
        /// A string containing the name of the attribute whose value is to be read.
        /// </param>
        /// <returns>
        /// A string containing the value of the attribute.
        /// </returns>
        string? ReadAttributeString(XmlElement element, string attributeName);
        /// <summary>
        /// Reads a list of strings from the specified element.
        /// </summary>
        /// <param name="element">
        /// The <see cref="XmlElement"/> containing the child element that contains the list of strings to read.
        /// </param>
        /// <param name="elementName">
        /// A string containing the name of the XML element whose inner contents are a list of strings.
        /// </param>
        /// <returns>
        /// A <see cref="List{T}"/> of <see cref="string"/> containing the list contents.
        /// </returns>
        List<string> ReadStringList(XmlElement element, string elementName);
        /// <summary>
        /// Reads a list of strings from the specified element.
        /// </summary>
        /// <param name="element">
        /// The <see cref="XmlElement"/> whose inner contents are <see cref="XmlText"/> nodes containing the strings to be read.
        /// </param>
        /// <returns>
        /// A <see cref="List{T}"/> of <see cref="string"/> containing the list contents.
        /// </returns>
        List<string> ReadStringList(XmlElement element);
        #endregion
    }
}
