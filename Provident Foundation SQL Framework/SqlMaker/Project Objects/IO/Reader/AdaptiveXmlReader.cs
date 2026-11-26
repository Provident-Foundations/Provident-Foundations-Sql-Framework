using Provident.Foundations.SqlMaker.Interfaces.IO;
using Provident.Foundations.Abstractions;
using System.Xml;

namespace Provident.Foundations.SqlMaker.Project
{
    /// <summary>
    /// Provides a helper class for reading XML elements from an XML document.
    /// </summary>
    /// <seealso cref="ExceptionTrackingBase" />
    /// <seealso cref="IAdaptiveXmlReader" />
    /// <remarks>
    /// Initializes a new instance of the <see cref="AdaptiveXmlReader"/> class.
    /// </remarks>
    /// <param name="document">
    /// The <see cref="XmlDocument"/> document to read objects from.
    /// </param>
    public sealed class AdaptiveXmlReader(XmlDocument document) : ExceptionTrackingBase, IAdaptiveXmlReader
    {
        #region Private Member Declarations        
        /// <summary>
        /// The XML document to append the new objects to.
        /// </summary>
        private XmlDocument? _document = document;

        #endregion

        #region Constructor / Dispose Methods        
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

        #region Puiblic Properties        
        /// <summary>
        /// Gets or sets the reference to the XML source document being read from.
        /// </summary>
        /// <value>
        /// The <see cref="XmlDocument" /> instance loaded into memory.
        /// </value>
        public XmlDocument SourceDocument
        {
            get => _document!;
            set => _document = value;
        }
        #endregion

        #region Public Methods / Functions        
        /// <summary>
        /// Reads the attribute value as a boolean value.
        /// </summary>
        /// <param name="element">The <see cref="XmlElement" /> to read the attribute from.</param>
        /// <param name="attributeName">A string containing the name of the attribute whose value is to be read.</param>
        /// <returns>
        /// A <see cref="bool" /> containing the value of the attribute.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public bool ReadAttributeBoolean(XmlElement? element, string attributeName)
        {
            bool value = false;

            if (element != null)
            {
                string? rawValue = ReadAttributeString(element, attributeName);
                // "1" = true; anything else at all is false.
                value = (!string.IsNullOrEmpty(rawValue) && rawValue == "1");
            }
            return value;
        }
        /// <summary>
        /// Reads the attribute value as a date/time value.
        /// </summary>
        /// <param name="element">The <see cref="XmlElement" /> to read the attribute from.</param>
        /// <param name="attributeName">A string containing the name of the attribute whose value is to be read.</param>
        /// <returns>
        /// A <see cref="DateTime" /> containing the value of the attribute.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public DateTime? ReadAttributeDateTime(XmlElement? element, string attributeName)
        {
            DateTime? value = null;

            if (element != null)
            {
                string? rawValue = ReadAttributeString(element, attributeName);
                if (rawValue != null)
                {
                    bool success = DateTime.TryParse(rawValue, System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.RoundtripKind, out DateTime parsedValue);
                    if (!success)
                        success = DateTime.TryParse(rawValue, out parsedValue);
                    if (success)
                        value = parsedValue;
                }
            }
            return value;
        }
        /// <summary>
        /// Reads the attribute value as a date/time value.
        /// </summary>
        /// <param name="element">The <see cref="XmlElement" /> to read the attribute from.</param>
        /// <param name="attributeName">A string containing the name of the attribute whose value is to be read.</param>
        /// <returns>
        /// A <see cref="DateTime" /> containing the value of the attribute.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public int ReadAttributeInt32(XmlElement? element, string attributeName)
        {
            int value = 0;

            if (element != null)
            {
                string? rawValue = ReadAttributeString(element, attributeName);
                if (rawValue != null)
                {
                    bool success = int.TryParse(rawValue, out int parsedValue);
                    if (success)
                        value = parsedValue;
                }
            }
            return value;
        }
        /// <summary>
        /// Reads the attribute value as a string.
        /// </summary>
        /// <param name="element">The <see cref="XmlElement" /> to read the attribute from.</param>
        /// <param name="attributeName">A string containing the name of the attribute whose value is to be read.</param>
        /// <returns>
        /// A string containing the value of the attribute.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string? ReadAttributeString(XmlElement element, string attributeName)
        {
            string? value = null;
            if (!string.IsNullOrEmpty(attributeName))
            {
                try
                {
                    XmlAttribute? attribute = element.Attributes[attributeName];
                    if (attribute != null)
                        value = attribute.Value;
                }
                catch (Exception ex)
                {
                    Exceptions?.Add(ex);
                }
            }
            return value;
        }
        /// <summary>
        /// Reads a list of strings from the specified element.
        /// </summary>
        /// <param name="element">The <see cref="XmlElement" /> containing the child element that contains the list of strings to read.</param>
        /// <param name="elementName">A string containing the name of the XML element whose inner contents are a list of strings.</param>
        /// <returns>
        /// A <see cref="List{T}" /> of <see cref="string" /> containing the list contents.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public List<string> ReadStringList(XmlElement element, string elementName)
        {
            List<string> returnList = [];

            if (!string.IsNullOrEmpty(elementName))
            {
                XmlNodeList nodeList = element.GetElementsByTagName(elementName);
                if (nodeList.Count > 0)
                {
                    if (nodeList[0] is XmlElement listElement)
                        returnList = ReadStringList(listElement);
                }
            }
            return returnList;
        }
        /// <summary>
        /// Reads a list of strings from the specified element.
        /// </summary>
        /// <param name="element">The <see cref="XmlElement" /> whose inner contents are <see cref="XmlText" /> nodes containing the strings to be read.</param>
        /// <returns>
        /// A <see cref="List{T}" /> of <see cref="string" /> containing the list contents.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public List<string> ReadStringList(XmlElement element)
        {
            List<string> stringList = [];
            foreach (XmlNode item in element.ChildNodes)
            {
                if (item is XmlElement entry && entry.Name == "Item")
                    stringList.Add(entry.InnerText);
                else if (item is XmlText text)
                    stringList.Add(text.Value ?? string.Empty);
            }
            return stringList;
        }
        #endregion
    }
}
  
