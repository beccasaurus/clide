using System;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FluentXml;
using Clide.Extensions;

namespace Clide {

	/// <summary>Represents all of a project's MSBuild Target Imports</summary>
	public class ProjectTargetImports : IEnumerable<TargetImport> {

		/// <summary>Main constructor.  ProjectTargetImports must have a project.</summary>
		public ProjectTargetImports(Project project) {
			Project = project;
		}

		/// <summary>The Project that these project references are for</summary>
		public virtual Project Project { get; set; }

		/// <summary>Provide a generic enumerator for our Project TargetImports</summary>
		public IEnumerator<TargetImport> GetEnumerator() {
			return GetTargetImports().GetEnumerator();
		}

		/// <summary>Provide a non-generic enumerator for our Project TargetImports</summary>
		IEnumerator IEnumerable.GetEnumerator() {
			return GetTargetImports().GetEnumerator();
		}

		/// <summary>Project Reference count</summary>
		public virtual int Count { get { return GetTargetImports().Count; } }

		/// <summary></summary>
		public virtual List<TargetImport> GetTargetImports() {
			return Project.Doc.Nodes("Import").Select(node => new TargetImport(this, node)).ToList();
		}

		// /// <summary>Adds and returns a TargetImports</summary>
		// public virtual TargetImports Add(string include = null, string exclude = null) {
		// 	if (include == null && exclude == null) return null;

		// 	var compile = new TargetImports(this, TargetImportsItemGroup.NewNode("Compile"));
		// 	if (include != null) compile.Include = include;
		// 	if (exclude != null) compile.Exclude = exclude;
		// 	return compile;
		// }
	}
}
