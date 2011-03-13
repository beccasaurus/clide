using System;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FluentXml;
using Clide.Extensions;

namespace Clide {

	/// <summary>Represents all of a project's Content include/exclude paths</summary>
	public class ProjectContent : IEnumerable<Content> {

		/// <summary>Main constructor.  ProjectContent must have a project.</summary>
		public ProjectContent(Project project) {
			Project = project;
		}

		/// <summary>The Project that these project references are for</summary>
		public virtual Project Project { get; set; }

		/// <summary>Provide a generic enumerator for our Project Content</summary>
		public IEnumerator<Content> GetEnumerator() {
			return GetContent().GetEnumerator();
		}

		/// <summary>Provide a non-generic enumerator for our Project Content</summary>
		IEnumerator IEnumerable.GetEnumerator() {
			return GetContent().GetEnumerator();
		}

		/// <summary>Project Reference count</summary>
		public virtual int Count { get { return GetContent().Count; } }

		/// <summary></summary>
		public virtual List<Content> GetContent() {
			return Project.Doc.Nodes("ItemGroup Content").Select(node => new Content(this, node)).ToList();
		}

		/// <summary>Finds or creates an ItemGroup for our Content nodes</summary>
		/// <remarks>
		/// If a project reference exists, we return its parent node, else we make a new ItemGroup
		/// </remarks>
		public virtual XmlNode ContentItemGroup {
			get {
				var firstReference = Project.Doc.Node("ItemGroup Content");
				if (firstReference != null)
					return firstReference.ParentNode;
				else {
					return Project.Doc.Node("Project").NewNode("ItemGroup");
				}
			}
		}

		/// <summary>Adds and returns a Content</summary>
		public virtual Content Add(string include = null, string exclude = null) {
			if (include == null && exclude == null) return null;

			var compile = new Content(this, ContentItemGroup.NewNode("Content"));
			if (include != null) compile.Include = include;
			if (exclude != null) compile.Exclude = exclude;
			return compile;
		}
	}
}
