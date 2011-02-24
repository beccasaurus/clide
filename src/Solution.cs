using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

		readonly Regex _getSectionName   = new Regex(@"GlobalSection\(([^\)]+)\)");
		readonly Regex _getStuffInQuotes = new Regex("\"([^\"]*)\"");

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
				else if (Sections.Count > 0 && ! string.IsNullOrEmpty(line) && ! line.TrimStart().StartsWith("EndGlobal")) {
					var clean     = line.TrimStart('\t').TrimEnd('\r').TrimEnd('\n'); //Replace("\r", "").Replace("\n", "");
					var section   = Sections.Last();
					section.Text += string.IsNullOrEmpty(section.Text) ? clean : "\n" + clean;
				}
		}

		// Project("{GUI}") = "MyApp", "MyApp\MyApp.csproj", "{GUID}"
		Project ProjectFromLine(string line) {
			var stuffInQuotes = GetStuffInQuotes(line);
			var name          = stuffInQuotes[1];
			var path          = stuffInQuotes[2];
			var guid          = new Guid(stuffInQuotes[3].TrimStart('{').TrimEnd('}'));

			return new Project { Name = name, Path = path, Id = guid };
		}

		// GlobalSection(ProjectConfigurationPlatforms) = postSolution
		Section SectionFromLine(string line) {
			var name = _getSectionName.Match(line).Groups[1].ToString();
			var pre  = line.Contains("= preSolution");

			return new Section { Name = name, PreSolution = pre };
		}

		// Given ... "Foo", "Bar" ... This would return new List<string>{ "Foo", "Bar" }
		List<string> GetStuffInQuotes(string text) {
			var stuff = new List<string>();
			foreach (Match match in _getStuffInQuotes.Matches(text))
				stuff.Add(match.Groups[1].ToString()); // get the Regex capture for this match
			return stuff;	
		}
	}
}
