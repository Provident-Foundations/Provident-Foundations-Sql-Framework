using Provident.Foundations.Abstractions;
using System.Collections.Specialized;
using System.Xml;
using Provident.Foundations.Sql.SqlMaker.Interfaces.IO.Writer;
using Provident.Foundations.Sql.SqlMaker.Constants;

namespace Provident.Foundations.SqlMaker.Project
{
    /// <summary>
    /// Provides a helper class for writing XML elements for an XML document.
    /// </summary>
    /// <seealso cref="ExceptionTrackingBase" />
    /// <seealso cref="IAdaptiveXmlWriter" />
    public sealed class AdaptiveXmlWriter : ExceptionTrackingBase, IAdaptiveXmlWriter
    {
        #region Private Member Declarations        
        /// <summary>
        /// The XML document to append the new objects to.
        /// </summary>
        private XmlDocument? _document;
        #endregion

        #region Constructor / Dispose Methods        
        /// <summary>
        /// Initializes a new instance of the <see cref="AdaptiveXmlWriter"/> class.
        /// </summary>
        /// <param name="document">
        /// The <see cref="XmlDocument"/> document to append new objects to.
        /// </param>
        public AdaptiveXmlWriter(XmlDocument document) => _document = document;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources;
        /// <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            _document = null;
            base.Dispose(disposing);
        }
        #endregion

        #region Public Properties        
        /// <summary>
        /// Gets or sets the reference to the XML source document being appended to.
        /// </summary>
        /// <value>
        /// The <see cref="XmlDocument" /> instance loaded into memory.
        /// </value>
        public XmlDocument DestinationDocument
        {
            get => _document!;
            set => _document = value;
        }
        #endregion

        #region Public Methods / Functions        
        /// <summary>
        /// Creates and appends a new attribute object to the <see cref="XmlElement" />.
        /// </summary>
        /// <param name="element">
        /// The <see cref="XmlElement" /> to append a new attribute to.
        /// </param>
        /// <param name="attributeName">
        /// A string containing the name of the attribute.
        /// </param>
        /// <param name="attributeValue">
        /// A string containing the attribute value.
        /// </param>
        public void AddAttribute(XmlElement element, string attributeName, string? attributeValue)
        {
            if (_document != null)
            {
                // Create the attribute object and set the value.
                XmlAttribute attribute = _document.CreateAttribute(attributeName);
                if (attributeValue == null)
                    attribute.Value = string.Empty;
                else
                    attribute.Value = attributeValue;

                // Append to the parent.
                element.Attributes.Append(attribute);
            }
        }
        /// <summary>
        /// Adds a new attribute to the <see cref="XmlElement" />.
        /// </summary>
        /// <param name="element">
        /// The <see cref="XmlElement" /> to append a new attribute to.
        /// </param>
        /// <param name="attributeName">
        /// A string containing the name of the attribute.
        /// </param>
        /// <param name="attributeValue">
        /// A <see cref="bool" /> containing the attribute value.
        /// </param>
        public void AddAttribute(XmlElement element, string attributeName, bool attributeValue)
        {
            string textValue;
            // Translate the boolean value to a string.
            if (attributeValue)
                textValue = "1";
            else
                textValue = "0";
            AddAttribute(element, attributeName, textValue);
        }

        public void AddAttribute(XmlElement element, string attributeName, DateTime? attributeValue)
        {
            string textValue = string.Empty;

            // Translate the Date.Time value to a string.
            if (attributeValue != null)
            {
                textValue = MakerConstants.DateToString(attributeValue.Value);
            }
            AddAttribute(element, attributeName, textValue);
        }

        public void AddAttribute(XmlElement element, string attributeName, int attributeValue)
        {
            string textValue = attributeValue.ToString();
            AddAttribute(element, attributeName, textValue);
        }
        /// <summary>
        /// Adds a new attribute to the <see cref="XmlElement" /> with an empty value string.
        /// </summary>
        /// <param name="element">The <see cref="XmlElement" /> to append a new attribute to.</param>
        /// <param name="attributeName">A string containing the name of the attribute.</param>
        public void AddAttributeWithNullValue(XmlElement element, string attributeName)
        {
            AddAttribute(element, attributeName, string.Empty);
        }
        /// <summary>
        /// Adds a list of strings as a child element to the specified XML element.
        /// </summary>
        /// <param name="element">The <see cref="XmlElement" /> to append a child element to that contains the list of strings.</param>
        /// <param name="listElementName">A string containing the name of the child list element to create.</param>
        /// <param name="stringList">An <see cref="IEnumerable{T}" /> list of <see cref="string" /> containing the entries to be added.</param>
        public void AddStringList(XmlElement element, string listElementName, IEnumerable<string>? stringList)
        {
            if (_document != null && stringList != null)
            {
                XmlElement listElement = _document.CreateElement(listElementName);
                element.AppendChild(listElement);
                foreach (string item in stringList)
                {
                    XmlElement entry = _document.CreateElement("Item");
                    entry.InnerText = item;
                    listElement.AppendChild(entry);
                }
            }
        }
        /// <summary>
        /// Adds a list of strings as a child element to the specified XML element.
        /// </summary>
        /// <param name="element">The <see cref="XmlElement" /> to append a child element to that contains the list of strings.</param>
        /// <param name="listElementName">A string containing the name of the child list element to create.</param>
        /// <param name="stringList">An <see cref="IEnumerable{T}" /> list of <see cref="string" /> containing the entries to be added.</param>
        public void AddStringList(XmlElement element, string listElementName, StringCollection? stringList)
        {
            if (_document != null && stringList != null)
            {
                XmlElement listElement = _document.CreateElement(listElementName);
                element.AppendChild(listElement);
                foreach (string? item in stringList)
                {
                    XmlElement entry = _document.CreateElement("Item");
                    entry.InnerText = item ?? string.Empty;
                    listElement.AppendChild(entry);
                }
            }
        }
        #endregion
    }
}
