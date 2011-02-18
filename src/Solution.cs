using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using IO.Interfaces;

namespace NVS {

	/// <summary>Represents a .sln solution file</summary>
	public class Solution : IFile {

		public Solution() {}
		public Solution(string path) {
			Path = path;
		}

		string _path;
		List<Project> _projects;
		List<Section> _sections;

		/// <summary>The file system path to this .sln file</summary>
		public virtual string Path {
			get { return _path; }
			set {
				_projects = null;
				_sections = null;
				_path     = value;
			}
		}

		/// <summary>All of the Project found in this Solution</summary>
		public virtual List<Project> Projects {
			get {
				if (_projects == null) Parse();
				return _projects;
			}
		}

		/// <summary>All of the GlobalSection sections found in this Solution</summary>
		public virtual List<Section> Sections {
			get {
				if (_sections == null) Parse();
				return _sections;
			}
		}

		void Parse() {
			if (this.DoesNotExist()) return;

			_sections = new List<Section>();
			_projects = new List<Project>();

			foreach (var line in this.Lines())
				if (line.StartsWith("Project("))
					Projects.Add(ProjectFromLine(line));
				else if (line.TrimStart().StartsWith("GlobalSection("))
					Sections.Add(SectionFromLine(line));
				else if (Sections.Count > 0 && ! line.TrimStart().StartsWith("EndGlobal"))
					Sections.Last().Text += "\n" + line;
		}

		// Project("{GUI}") = "MyApp", "MyApp\MyApp.csproj", "{GUID}"
		Project ProjectFromLine(string line) {
			return new Project();
		}

		// GlobalSection(ProjectConfigurationPlatforms) = postSolution
		Section SectionFromLine(string line) {
			return new Section();
		}
	}
}
