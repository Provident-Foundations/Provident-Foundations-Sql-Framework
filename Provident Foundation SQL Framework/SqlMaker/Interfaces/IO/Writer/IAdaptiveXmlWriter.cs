using System.Xml;

namespace Adaptive.SqlMaker.Interfaces.IO
{
    /// <summary>
    /// Provides the signature definition for objects used to writer structured XML data to an XML Document.
    /// </summary>
    public interface IAdaptiveXmlWriter : IDisposable 
    {
        #region Properties        
        /// <summary>
        /// Gets or sets the reference to the XML source document where new XML objects are being appended to.
        /// </summary>
        /// <value>
        /// The <see cref="XmlDocument"/> instance.
        /// </value>
        XmlDocument DestinationDocument { get; set; }
        #endregion

        #region Methods / Functions        
        /// <summary>
        /// Adds a new attribute to the <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="element">
        /// The <see cref="XmlElement"/> to append a new attribute to.
        /// </param>
        /// <param name="attributeName">
        /// A string containing the name of the attribute.
        /// </param>
        /// <param name="attributeValue">
        /// A string containing the attribute value.
        /// </param>
        void AddAttribute(XmlElement element, string attributeName, string? attributeValue);
        /// <summary>
        /// Adds a new attribute to the <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="element">
        /// The <see cref="XmlElement"/> to append a new attribute to.
        /// </param>
        /// <param name="attributeName">
        /// A string containing the name of the attribute.
        /// </param>
        /// <param name="attributeValue">
        /// A <see cref="bool"/> containing the attribute value.
        /// </param>
        void AddAttribute(XmlElement element, string attributeName, bool attributeValue);
        /// <summary>
        /// Adds a new attribute to the <see cref="XmlElement"/>.
        /// </summary>
        /// <param name="element">
        /// The <see cref="XmlElement"/> to append a new attribute to.
        /// </param>
        /// <param name="attributeName">
        /// A string containing the name of the attribute.
        /// </param>
        /// <param name="attributeValue">
        /// A <see cref="DateTime"/> containing the attribute value.
        /// </param>
        void AddAttribute(XmlElement element, string attributeName, DateTime? attributeValue);
        /// <summary>
        /// Adds a new attribute to the <see cref="XmlElement"/> with an empty value string.
        /// </summary>
        /// <param name="element">
        /// The <see cref="XmlElement"/> to append a new attribute to.
        /// </param>
        /// <param name="attributeName">
        /// A string containing the name of the attribute.
        /// </param>
        void AddAttributeWithNullValue(XmlElement element, string attributeName);
        /// <summary>
        /// Adds a list of strings as a child element to the specified XML element.
        /// </summary>
        /// <param name="element">
        /// The <see cref="XmlElement"/> to append a child element to that contains the list of strings.
        /// </param>
        /// <param name="listElementName">
        /// A string containing the name of the child list element to create.
        /// </param>
        /// <param name="stringList">
        /// An <see cref="IEnumerable{T}"/> list of <see cref="string"/> containing the entries to be added.
        /// </param>
        void AddStringList(XmlElement element, string listElementName, IEnumerable<string> stringList);
        #endregion
    }
}
