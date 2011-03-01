using System;
using System.Xml;

namespace Clide {

	/// <summary>Any class that represents an XmlNode can implement IXmlNode (to get IXmlNodeExtensions)</summary>
	public interface IXmlNode {
		XmlNode Node { get; }
	}
}
