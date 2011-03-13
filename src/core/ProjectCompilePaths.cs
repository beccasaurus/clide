using System;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FluentXml;
using Clide.Extensions;

namespace Clide {

	/// <summary>Represents all of a project's Compile include/exclude paths</summary>
	public class ProjectCompilePaths : IEnumerable<CompilePath> {

		/// <summary>Main constructor.  ProjectCompilePaths must have a project.</summary>
		public ProjectCompilePaths(Project project) {
			Project = project;
		}

		/// <summary>The Project that these project references are for</summary>
		public virtual Project Project { get; set; }

		/// <summary>Provide a generic enumerator for our Project CompilePaths</summary>
		public IEnumerator<CompilePath> GetEnumerator() {
			return GetCompilePaths().GetEnumerator();
		}

		/// <summary>Provide a non-generic enumerator for our Project CompilePaths</summary>
		IEnumerator IEnumerable.GetEnumerator() {
			return GetCompilePaths().GetEnumerator();
		}

		/// <summary>Project Reference count</summary>
		public virtual int Count { get { return GetCompilePaths().Count; } }

		/// <summary></summary>
		public virtual List<CompilePath> GetCompilePaths() {
			return Project.Doc.Nodes("ItemGroup Compile").Select(node => new CompilePath(this, node)).ToList();
		}

		/// <summary>Finds or creates an ItemGroup for our CompilePath nodes</summary>
		/// <remarks>
		/// If a project reference exists, we return its parent node, else we make a new ItemGroup
		/// </remarks>
		public virtual XmlNode CompilePathsItemGroup {
			get {
				var firstReference = Project.Doc.Node("ItemGroup Compile");
				if (firstReference != null)
					return firstReference.ParentNode;
				else {
					return Project.Doc.Node("Project").NewNode("ItemGroup");
				}
			}
		}

		/// <summary>Adds and returns a CompilePath</summary>
		public virtual CompilePath Add(string include = null, string exclude = null) {
			if (include == null && exclude == null) return null;

			var compile = new CompilePath(this, CompilePathsItemGroup.NewNode("Compile"));
			if (include != null) compile.Include = include;
			if (exclude != null) compile.Exclude = exclude;
			return compile;
		}
	}
}
