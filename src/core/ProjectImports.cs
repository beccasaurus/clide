using System;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FluentXml;
using Clide.Extensions;

namespace Clide {

	/// <summary>Represents all of a project's MSBuild Imports</summary>
	public class ProjectImports : IEnumerable<Import> {

		/// <summary>Main constructor.  ProjectImport must have a project.</summary>
		public ProjectImports(Project project) {
			Project = project;
		}

		/// <summary>The Project that these project references are for</summary>
		public virtual Project Project { get; set; }

		/// <summary>Provide a generic enumerator for our Project Import</summary>
		public IEnumerator<Import> GetEnumerator() {
			return GetImports().GetEnumerator();
		}

		/// <summary>Provide a non-generic enumerator for our Project Import</summary>
		IEnumerator IEnumerable.GetEnumerator() {
			return GetImports().GetEnumerator();
		}

		/// <summary>Project Reference count</summary>
		public virtual int Count { get { return GetImports().Count; } }

		public virtual List<Import> GetImports() {
			return Project.Doc.Nodes("Import").Select(node => new Import(this, node)).ToList();
		}

		/// <summary>Adds and returns a Import</summary>
		public virtual Import Add(string project = null) {
			var import = new Import(this, Project.Doc.Node("Project").NewNode("Import"));
			if (project != null) import.Project = project;
			return import;
		}
	}
}
