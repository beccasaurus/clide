using System;
using System.Xml;
using System.Linq;
using FluentXml;
using Clide.Extensions;

namespace Clide {

	/// <summary>Represents a reference to another project in a Project's configuration</summary>
	public class ProjectReference : IXmlNode {

		/// <summary>ProjectReference constructor.  A ProjectReference requires an XmlNode and the ProjectProjectReferences object</summary>
		public ProjectReference(ProjectProjectReferences projectReferences, XmlNode node) {
			ProjectReferences = projectReferences;
			Node              = node;
		}

		/// <summary>The XmlNode that this ProjectReference is stored in</summary>
		public virtual XmlNode Node { get; set; }

		/// <summary>The ProjectProjectReferences that this ProjectReferences is a part of</summary>
		public virtual ProjectProjectReferences ProjectReferences { get; set; }

		/// <summary>The name of the project that this project references</summary>
		public virtual string Name {
			get { return Node.Node("Name").Text(); }
			set { Node.NodeOrNew("Name").Text(value);   }
		}

		/// <summary>The Guid ProjectId of the project that we're referencing</summary>
		public virtual Guid? ProjectId {
			get { return Node.Node("Project").Text().ToGuid(); }
			set { Node.NodeOrNew("Project").Text(value.WithCurlies()); }
		}

		/// <summary>The relative path to the project file (eg. csproj) that we're referencing</summary>
		public virtual string ProjectFile {
			get { return Node.Attr("Include"); }
			set { Node.Attr("Include", value); }
		}

		/// <summary>Remove this project reference from the Project.  Calling Project.Save() will persist this change.</summary>
		public virtual void Remove() {
			Node.ParentNode.RemoveChild(Node);
		}
	}
}
