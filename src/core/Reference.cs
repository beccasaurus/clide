using System;
using System.Xml;
using System.Linq;
using FluentXml;

namespace Clide {

	/// <summary>Represents a reference to an Assembly in a Project's configuration</summary>
	public class Reference : IXmlNode {

		/// <summary>Reference constructor.  A Reference requires an XmlNode and the ProjectReferences object</summary>
		public Reference(ProjectReferences references, XmlNode node) {
			References = references;
			Node       = node;
		}

		/// <summary>The ProjectReferences that this Reference is a part of</summary>
		public ProjectReferences References { get; set; }

		/// <summary>The XmlNode that this Reference is stored in</summary>
		public XmlNode Node { get; set; }

		/// <summary>Returns this references's full assembly name</summary>
		public virtual string FullName { get { return Node.Attr("Include"); } }

		/// <summary>Returns the path, typically relative to the Project file, where this DLL can be found</summary>
		public virtual string HintPath { get { return Node.Node("HintPath").Text(); } }

		/// <summary>Returns whether or not this project requires this SpecificVersion of this assembly (I think?)</summary>
		public virtual bool SpecificVersion {
			get {
				var version = Node.Node("SpecificVersion").Text();
				return (version == null) ? false : bool.Parse(version);
			}
		}

		/// <summary>This reference's short name, eg. "System" or "MyAssembly."  This reads from FullName.</summary>
		public virtual string Name { get { return FullName.Split(',').FirstOrDefault(); } }

		/// <summary>Remove this reference from the Project.  Calling Project.Save() will persist this change.</summary>
		public virtual void Remove() {
			Node.ParentNode.RemoveChild(Node);
		}
	}
}
