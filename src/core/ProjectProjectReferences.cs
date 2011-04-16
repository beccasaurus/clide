using System;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FluentXml;
using Clide.Extensions;

namespace Clide {

	/// <summary>Class with a very crappy name representing a Project's ProjectReferences</summary>
	public class ProjectProjectReferences : IEnumerable<ProjectReference> {

		/// <summary>Main constructor.  ProjectProjectReferences must have a project.</summary>
		public ProjectProjectReferences(Project project) {
			Project = project;
		}

		/// <summary>The Project that these project references are for</summary>
		public virtual Project Project { get; set; }

		/// <summary>Provide a generic enumerator for our Project References</summary>
		public IEnumerator<ProjectReference> GetEnumerator() {
			return GetProjectReferences().GetEnumerator();
		}

		/// <summary>Provide a non-generic enumerator for our Project References</summary>
		IEnumerator IEnumerable.GetEnumerator() {
			return GetProjectReferences().GetEnumerator();
		}

		/// <summary>Project Reference count</summary>
		public virtual int Count { get { return GetProjectReferences().Count; } }

		/// <summary></summary>
		public virtual List<ProjectReference> GetProjectReferences() {
			return Project.Doc.Nodes("ItemGroup ProjectReference").Select(node => new ProjectReference(this, node)).ToList();
		}

		/// <summary>Finds or creates an ItemGroup for our ProjectReference nodes</summary>
		/// <remarks>
		/// If a project reference exists, we return its parent node, else we make a new ItemGroup
		/// </remarks>
		public virtual XmlNode ProjectReferencesItemGroup {
			get {
				var firstReference = Project.Doc.Node("ItemGroup ProjectReference");
				if (firstReference != null)
					return firstReference.ParentNode;
				else {
					return Project.Doc.Node("Project").NewNode("ItemGroup");
				}
			}
		}

		/// <summary>Adds and returns a ProjectReference</summary>
		/// <remarks>
		/// This is dumb and will add the EXACT name, path, id that you give it!
		/// </remarks>
		public virtual ProjectReference Add(string name, string path, Guid? id) {
			var reference         = new ProjectReference(this, ProjectReferencesItemGroup.NewNode("ProjectReference"));
			reference.ProjectFile = path;
			reference.ProjectId   = id;
			reference.Name        = name;
			return reference;
		}
	}
}
