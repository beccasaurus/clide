using System;
using System.Xml;
using System.Linq;
using FluentXml;
using Clide.Extensions;

namespace Clide {

	/// <summary>Represents Content include/exclude element of a project</summary>
	public class Content : IXmlNode {

		/// <summary>Content constructor.  A Content requires an XmlNode and the ProjectContent object</summary>
		public Content(ProjectContent projectReferences, XmlNode node) {
			Contents = projectReferences;
			Node     = node;
		}

		/// <summary>The XmlNode that this Content is stored in</summary>
		public virtual XmlNode Node { get; set; }

		/// <summary>The ProjectContent that this Content is a part of</summary>
		public virtual ProjectContent Contents { get; set; }

		/// <summary>This Content's Include attribute</summary>
		public virtual string Include {
			get { return Node.Attr("Include"); }
			set { Node.Attr("Include", value); }
		}

		/// <summary>This Content's Exclude attribute</summary>
		public virtual string Exclude {
			get { return Node.Attr("Exclude"); }
			set { Node.Attr("Exclude", value); }
		}

		/// <summary>A file that this Content is DependentUpon</summary>
		public virtual string DependentUpon {
			get { return Node.Node("DependentUpon").Text(); }
			set { Node.NodeOrNew("DependentUpon").Text(value);   }
		}

		/// <summary>Remove this Content from the Project.  Calling Project.Save() will persist this change.</summary>
		public virtual void Remove() {
			Node.ParentNode.RemoveChild(Node);
		}
	}
}
