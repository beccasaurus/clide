using System;
using System.Xml;
using System.Linq;
using FluentXml;
using Clide.Extensions;

namespace Clide {

	/// <summary>Represents an MSBuild Target import/summary>
	public class TargetImport : IXmlNode {

		/// <summary>TargetImport constructor.  A TargetImport requires an XmlNode and the ProjectTargetImports object</summary>
		public TargetImport(ProjectTargetImports projectReferences, XmlNode node) {
			TargetImports = projectReferences;
			Node          = node;
		}

		/// <summary>The XmlNode that this TargetImport is stored in</summary>
		public virtual XmlNode Node { get; set; }

		/// <summary>The ProjectTargetImports that this TargetImports is a part of</summary>
		public virtual ProjectTargetImports TargetImports { get; set; }

		/// <summary>This Import's Project attribute</summary>
		public virtual string Project {
			get { return Node.Attr("Project"); }
			set { Node.Attr("Project", value); }
		}

		/// <summary>Remove this TargetImport from the Project.  Calling Project.Save() will persist this change.</summary>
		public virtual void Remove() {
			Node.ParentNode.RemoveChild(Node);
		}
	}
}
