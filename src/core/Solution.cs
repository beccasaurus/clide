using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using IO.Interfaces;
using Clide.Extensions;

namespace Clide {

	/// <summary>Represents a .sln solution file</summary>
	public class Solution : IFile {

		/// <summary>Empty constructor for creating a fresh, blank Solution</summary>
		public Solution() {
			AutoGenerateProjectConfigurationPlatforms = true;
			FormatVersion       = "11.00";
			VisualStudioVersion = "2010";
		}

		/// <summary>Create a Solution with the given Path.  If the file is found, we will parse it.</summary>
		public Solution(string path) : this() {
			Path = path;
			Parse();
		}

		string _path;
		List<Project> _projects;
		List<Section> _sections;

		static readonly Regex _getSectionName         = new Regex(@"GlobalSection\(([^\)]+)\)");
		static readonly Regex _getStuffInQuotes       = new Regex("\"([^\"]*)\"");
		static readonly Regex _getFormatVersion       = new Regex(@"Microsoft Visual Studio Solution File, Format Version ([\d\.]+)");
		static readonly Regex _getVisualStudioVersion = new Regex(@"# Visual Studio (\d+)");

		/// <summary>The file system path to this .sln file</summary>
		public virtual string Path { get; set; }

		/// <summary>The version of this sln's format, eg. 11.00 (for VS 2010)</summary>
		public virtual string FormatVersion { get; set; }

		/// <summary>The Visual Studio version for this sln, eg. 2010</summary>
		public virtual string VisualStudioVersion { get; set; }

		/// <summary>All of the Project found in this Solution</summary>
		public virtual List<Project> Projects {
			get { if (_projects == null) Parse(); return _projects; }
		}

		/// <summary>All of the GlobalSection sections found in this Solution</summary>
		public virtual List<Section> Sections {
			get { if (_sections == null) Parse(); return _sections; }
		}

		/// <summary>Adds the provided Project and returns this Solution (for a fluent interface)</summary>
		public virtual Solution Add(Project project) { Projects.Add(project); return this; }

		/// <summary>Adds the provided Section and returns this Solution (for a fluent interface)</summary>
		public virtual Solution Add(Section section) { Sections.Add(section); return this; }

		/// <summary>
		/// If set to true, we auto-genenerate the 'SolutionConfigurationPlatforms' and 'ProjectConfigurationPlatforms' sections based on this Solution's projects
		/// </summary>
		public virtual bool AutoGenerateProjectConfigurationPlatforms { get; set; }

		/// <summary>Returns the section with the provided name if it's defined, or null</summary>
		public virtual Section GetSection(string name) {
			return Sections.FirstOrDefault(section => section.Name == name);
		}

		/// <summary>Returns the SolutionConfigurationPlatforms section if it's defined, or null</summary>
		public virtual Section SolutionConfigurationPlatforms {
			get { return GetSection("SolutionConfigurationPlatforms"); }
		}

		/// <summary>Returns the ProjectConfigurationPlatforms section if it's defined, or null</summary>
		public virtual Section ProjectConfigurationPlatforms {
			get { return GetSection("ProjectConfigurationPlatforms"); }
		}

		/// <summary>This will generate and return a SolutionConfigurationPlatforms Section (based on Projects)</summary>
		public virtual Section GeneratedSolutionConfigurationPlatforms {
			get {
				return new Section {
					Name        = "SolutionConfigurationPlatforms",
					PreSolution = true,
					Text        = "Debug|Any CPU = Debug|Any CPU \n\t\tRelease|Any CPU = Release|Any CPU "
				};
			}
		}

		/// <summary>This will generate and return a ProjectConfigurationPlatforms Section (based on Projects)</summary>
		public virtual Section GeneratedProjectConfigurationPlatforms {
			get {
				var section = new Section {
					Name         = "ProjectConfigurationPlatforms",
					PostSolution = true,
				};

				var lines = new List<string>();

				foreach (var project in Projects) {
					foreach (var configuration in project.Configurations.Custom) {
						lines.Add(string.Format(
							"{0}.{1}|Any CPU.ActiveCfg = {1}|Any CPU ",
							project.Id.ToString().ToUpper().WithCurlies(),
							configuration.Name
						));
						lines.Add(string.Format(
							"{0}.{1}|Any CPU.Build.0 = {1}|Any CPU ",
							project.Id.ToString().ToUpper().WithCurlies(),
							configuration.Name
						));
					}
				}

				section.Text = string.Join("\n\t\t", lines.ToArray());

				return section;
			}
		}

		// 
		// Microsoft Visual Studio Solution File, Format Version 10.00
		// # Visual Studio 2008
		// Project(...
		//
		/// <summary>If this project's Path exists, we read and parse the file</summary>
		/// <remarks>
		/// This will reset the project's Sections and Projects.
		/// </remarks>
		public virtual Solution Parse() {
			_sections = new List<Section>();
			_projects = new List<Project>();

			if (this.DoesNotExist()) return this;

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

		/// <summary>Returns the text for this solution file.</summary>
		/// <remarks>
		/// This is generated.  It does not read from the saved file (nor does it require a saved file).
		/// </remarks>
		public virtual string ToText() {
			var builder = new StringBuilder();

			builder.AppendLine();
			builder.AppendLine("Microsoft Visual Studio Solution File, Format Version {0}", FormatVersion);
			builder.AppendLine("# Visual Studio {0}", VisualStudioVersion);

			foreach (var project in Projects)
				AppendProject(builder, project);

			builder.AppendLine("Global");
			if (AutoGenerateProjectConfigurationPlatforms && SolutionConfigurationPlatforms == null && Projects.Count > 0)
				AppendSection(builder, GeneratedSolutionConfigurationPlatforms);
			if (AutoGenerateProjectConfigurationPlatforms && ProjectConfigurationPlatforms == null && Projects.Count > 0)
				AppendSection(builder, GeneratedProjectConfigurationPlatforms);
			foreach (var section in Sections)
				AppendSection(builder, section);
			builder.AppendLine("EndGlobal");

			return builder.ToString();
		}

		/// <summary>Persists this solution's text (via ToText()) to disk (saves to Path)</summary>
		public virtual Solution Save() {
			this.Write(ToText());
			return this;
		}

		/// <summary>Helper for getting a new, blank solution (with some defaults, but no projects)</summary>
		public static Solution Blank { get { return new Solution(); }}

		/// <summary>Helper for getting a new solution from the provided path</summary>
		public static Solution FromPath(string path) { return new Solution(path); }

	// private

		// Project("{GUI}") = "MyApp", "MyApp\MyApp.csproj", "{GUID}"
		Project ProjectFromLine(string line) {
			var quotedStuff = GetStuffInQuotes(line);
			var type        = quotedStuff[0].ToGuid();
			var name        = quotedStuff[1];
			var path        = quotedStuff[2];
			var guid        = quotedStuff[3].ToGuid();
			var fullPath    = System.IO.Path.Combine(this.DirName(), path);

			if (File.Exists(fullPath))
				return new Project(fullPath);
			else
				return new Project { Name = name, RelativePath = path, Id = guid, ProjectTypeId = type };
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

		void AppendProject(StringBuilder builder, Project project) {
			if (project == null || project.Path == null) return;

			var relativeProjectPath = string.IsNullOrEmpty(this.Path)
				? project.Path
				: this.DirName().AsDir().Relative(project.Path).TrimStart('/');

			builder.AppendLine("Project({0}) = {1}, {2}, {3}", 
					project.ProjectTypeId.QuotedWithCurlies(),
					project.Name.Quoted(),
					relativeProjectPath.Quoted(),
					project.Id.QuotedWithCurlies());
			builder.AppendLine("EndProject");
		}

		void AppendSection(StringBuilder builder, Section section) {
			builder.AppendLine("\tGlobalSection({0}) = {1}", section.Name, section.PreSolution ? "preSolution" : "postSolution");
			if (! string.IsNullOrEmpty(section.Text)) builder.AppendLine("\t\t{0}", section.Text);
			builder.AppendLine("\tEndGlobalSection");
		}
	}
}
