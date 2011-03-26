using System;
using System.Xml;
using System.Linq;
using FluentXml;
using Clide.Extensions;

namespace Clide {

	/// <summary>Represents a Compile include/exclude element of a project</summary>
	public class CompilePath : IXmlNode {

		/// <summary>CompilePath constructor.  A CompilePath requires an XmlNode and the ProjectCompilePaths object</summary>
		public CompilePath(ProjectCompilePaths projectReferences, XmlNode node) {
			CompilePaths = projectReferences;
			Node         = node;
		}

		/// <summary>The XmlNode that this CompilePath is stored in</summary>
		public virtual XmlNode Node { get; set; }

		/// <summary>The ProjectCompilePaths that this CompilePaths is a part of</summary>
		public virtual ProjectCompilePaths CompilePaths { get; set; }

		/// <summary>This CompilePath's Include attribute</summary>
		public virtual string Include {
			get { return Node.Attr("Include"); }
			set { Node.Attr("Include", Project.NormalizePath(value)); }
		}

		/// <summary>This CompilePath's Exclude attribute</summary>
		public virtual string Exclude {
			get { return Node.Attr("Exclude"); }
			set { Node.Attr("Exclude", Project.NormalizePath(value)); }
		}

		/// <summary>A file that this CompilePath is DependentUpon</summary>
		public virtual string DependentUpon {
			get { return Node.Node("DependentUpon").Text(); }
			set { Node.NodeOrNew("DependentUpon").Text(Project.NormalizePath(value));   }
		}

		/// <summary>A file that this CompilePath is DependentUpon</summary>
		public virtual string Link {
			get { return Node.Node("Link").Text(); }
			set { Node.NodeOrNew("Link").Text(Project.NormalizePath(value));   }
		}

		/// <summary>Remove this CompilePath from the Project.  Calling Project.Save() will persist this change.</summary>
		public virtual void Remove() {
			Node.ParentNode.RemoveChild(Node);
		}
	}
}
