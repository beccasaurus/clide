using System;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FluentXml;

namespace Clide {

	/// <summary>API for getting/setting a Project's references</summary>
	public class ProjectReferences : IEnumerable<Reference> {

		/// <summary>Main constructor.  ProjectReferences must have a project.</summary>
		public ProjectReferences(Project project) {
			Project = project;
		}

		/// <summary>The Project that these references are for</summary>
		public virtual Project Project { get; set; }

		/// <summary>Provide a generic enumerator for our References</summary>
		public IEnumerator<Reference> GetEnumerator() {
			return GetReferences().GetEnumerator();
		}

		/// <summary>Provide a non-generic enumerator for our References</summary>
		IEnumerator IEnumerable.GetEnumerator() {
			return GetReferences().GetEnumerator();
		}

		/// <summary>Reference count</summary>
		public virtual int Count { get { return GetReferences().Count; } }

		/// <summary>Get a reference by index</summary>
		public virtual Reference this[int index] { get { return GetReferences()[index]; } }

		/// <summary>Get a reference by name (see Get())</summary>
		public virtual Reference this[string name] { get { return Get(name); } }

		/// <summary>Actual method to go and get and return References.</summary>
		/// <remarks>
		/// Note, this is not cached!  Hence, why it's a method instead of a property.
		///
		/// We'll very likely cache this later, but I don't want to add caching before it's truly necessary.
		/// </remarks>
		public virtual List<Reference> GetReferences() {
			return Project.Doc.Nodes("ItemGroup Reference").Select(node => new Reference(this, node)).ToList();
		}

		/// <summary>Finds or creates an ItemGroup node to hold our Reference nodes</summary>
		/// <remarks>
		/// Tries to find an existing Reference.  If there is one, we use its parent ItemGroup.
		/// </remarks>
		public virtual XmlNode ReferencesItemGroup {
			get {
				var firstReference = Project.Doc.Node("ItemGroup Reference");
				if (firstReference != null)
					return firstReference.ParentNode;
				else
					return Project.Doc.Node("Project").NewNode("ItemGroup");
			}
		}

		public virtual Reference AddGacReference(string assemblyName) {
			var reference      = new Reference(this, ReferencesItemGroup.NewNode("Reference"));
			reference.FullName = assemblyName;
			return reference;
		}

		public virtual Reference AddDll(string fullName, string hintPath) {
			return AddDll(fullName, hintPath, false);
		}

		public virtual Reference AddDll(string fullName, string hintPath, bool specificVersion) {
			var reference             = new Reference(this, ReferencesItemGroup.NewNode("Reference"));
			reference.FullName        = fullName;
			reference.HintPath        = Project.NormalizePath(hintPath);
			reference.SpecificVersion = specificVersion;
			return reference;
		}

		/// <summary>Get a Reference by name (FullName or just Name)</summary>
		public virtual Reference Get(string name) {
			return GetByFullName(name) ?? GetByName(name);
		}

		/// <summary>Returns reference found with this full name (or null)</summary>
		public virtual Reference GetByFullName(string fullName) {
			return GetReferences().FirstOrDefault(reference => reference.FullName == fullName);
		}

		/// <summary>Returns reference found with this name (or null)</summary>
		public virtual Reference GetByName(string name) {
			return GetReferences().FirstOrDefault(reference => reference.Name == name);
		}

		/// <summary>Remove reference by FullName or just Name</summary>
		public virtual void Remove(string referenceName) {
			var reference = Get(referenceName);
			if (reference != null)
				reference.Remove();
		}
	}
}
