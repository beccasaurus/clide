using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using IO.Interfaces;
using NVS.Extensions;

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

		static readonly Regex _getSectionName         = new Regex(@"GlobalSection\(([^\)]+)\)");
		static readonly Regex _getStuffInQuotes       = new Regex("\"([^\"]*)\"");
		static readonly Regex _getFormatVersion       = new Regex(@"Microsoft Visual Studio Solution File, Format Version ([\d\.]+)");
		static readonly Regex _getVisualStudioVersion = new Regex(@"# Visual Studio (\d+)");

		/// <summary>The file system path to this .sln file</summary>
		public virtual string Path {
			get { return _path; }
			set {
				_projects = null;
				_sections = null;
				_path     = value;
				Parse();
			}
		}

		/// <summary>Sets the Path without resetting Projects/Sections and re-parsing the Solution</summary>
		public virtual void SetPath(string path) {
			_path = path;
		}

		/// <summary>The version of this sln's format, eg. 11.00 (for VS 2010)</summary>
		public virtual string FormatVersion { get; set; }

		/// <summary>The Visual Studio version for this sln, eg. 2010</summary>
		public virtual string VisualStudioVersion { get; set; }

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

		// 
		// Microsoft Visual Studio Solution File, Format Version 10.00
		// # Visual Studio 2008
		// Project(...
		public virtual Solution Parse() {
			if (this.DoesNotExist()) return this;

			_sections = new List<Section>();
			_projects = new List<Project>();

			foreach (var line in this.Lines())
				if (line.StartsWith("Microsoft Visual Studio Solution File"))
					FormatVersion = GetFormatVersionFromLine(line);
				else if (line.StartsWith("# Visual Studio"))
					VisualStudioVersion = GetVisualStudioVersionFromLine(line);
				else if (line.StartsWith("Project("))
					Projects.Add(ProjectFromLine(line));
				else if (line.TrimStart().StartsWith("GlobalSection("))
					Sections.Add(SectionFromLine(line));
				else if (Sections.Count > 0 && ! string.IsNullOrEmpty(line) && ! line.TrimStart().StartsWith("EndGlobal")) {
					var clean     = line.TrimStart('\t').TrimEnd('\r').TrimEnd('\n'); //Replace("\r", "").Replace("\n", "");
					var section   = Sections.Last();
					section.Text += string.IsNullOrEmpty(section.Text) ? clean : "\n" + clean;
				}

			return this;
		}

		// Project("{GUI}") = "MyApp", "MyApp\MyApp.csproj", "{GUID}"
		Project ProjectFromLine(string line) {
			var quotedStuff = GetStuffInQuotes(line);
			var type        = quotedStuff[0].ToGuid();
			var name        = quotedStuff[1];
			var path        = quotedStuff[2];
			var guid        = quotedStuff[3].ToGuid();

			return new Project { Name = name, Path = path, Id = guid, ProjectTypeId = type };
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

		string GetFormatVersionFromLine(string line) {
			return _getFormatVersion.Match(line).Groups[1].ToString();
		}

		string GetVisualStudioVersionFromLine(string line) {
			return _getVisualStudioVersion.Match(line).Groups[1].ToString();
		}
	}
}
