using System;
using System.Xml;
using System.Linq;
using FluentXml;
using Clide.Extensions;

namespace Clide {

	/// <summary>Represents an MSBuild Import</summary>
	public class Import : IXmlNode {

		/// <summary>Import constructor.  A Import requires an XmlNode and the ProjectImport object</summary>
		public Import(ProjectImports projectImports, XmlNode node) {
			Imports = projectImports;
			Node    = node;
		}

		/// <summary>The XmlNode that this Import is stored in</summary>
		public virtual XmlNode Node { get; set; }

		/// <summary>The ProjectImport that this Import is a part of</summary>
		public virtual ProjectImports Imports { get; set; }

		/// <summary>This Import's Project attribute</summary>
		public virtual string Project {
			get { return Node.Attr("Project"); }
			set { Node.Attr("Project", value); }
		}

		/// <summary>Remove this Import from the Project.  Calling Project.Save() will persist this change.</summary>
		public virtual void Remove() {
			Node.ParentNode.RemoveChild(Node);
		}
	}
}
