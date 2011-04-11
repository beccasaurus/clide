using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mono.Options;
using ConsoleRack;
using IO.Interfaces;
using Clide.Extensions;

namespace Clide {

	// TODO extract stuff out of here into a base ClideCommand class (for parsing, etc)
	/// <summary>clide solution</summary>
	public class SolutionCommand {

		[Command("sln", "Alias for 'solution'")]
		public static Response SlnCmd(Request req){ return new SolutionCommand(req).Invoke(); }

		[Command("solution", "Create/Edit solution files")]
		public static Response SolutionCmd(Request req) { return new SolutionCommand(req).Invoke(); }

		public virtual string HelpText {
			get { return @"
Usage: clide sln [add|rm|SolutionName] [options]

  Examples:
    clide sln                  Creates a new solution (using the current directory name)
    clide sln Foo              Creates a new Foo.sln (or displays it, if it already exists)
    clide sln add Foo.csproj   Adds the Foo.csproj project to the current solution
    clide sln rm Foo.csproj    Removes the Foo.csproj project from the current solution

  Options:
    -b, --blank       Creates a bare sln file (won't automatically add the current project)
    -n, --name        Explicitly set the name of the solution file to create (clide sln -n Foo)
	-p, --project     Specify project(s) that you want to automatically be added (when creating)

COMMON".Replace("COMMON", Global.CommonOptionsText).TrimStart('\n'); }
		}

		public SolutionCommand(Request request) {
			Request       = request;
			ProjectsToAdd = new List<string>();
		}

		string _name, _solutionPath;
		Solution _solution;

		public virtual Request Request { get; set; }

		public virtual List<string> ProjectsToAdd { get; set; }

		public virtual bool MakeBlank { get; set; }

		public virtual string DirectoryName {
			get { return Path.GetFileName(Path.GetFullPath(Global.WorkingDirectory)); }
		}

		public virtual string SolutionName {
			get {
				string name = null;
				if (! string.IsNullOrEmpty(_name)) // we explicitly set the name
					name = _name;
				else if (! string.IsNullOrEmpty(Global.Solution)) // there's a solution set
					name = Path.GetFileName(Global.Solution);
				else
					name = DirectoryName; // fall back to the name of the directory
				return Regex.Replace(name, @"\.sln$", "");
			}
			set { _name = value; }
		}

		public virtual string SolutionPath {
			get { return _solutionPath ?? Path.Combine(Global.WorkingDirectory, SolutionName + ".sln"); }
			set { _solutionPath = value; }
		}

		public virtual Solution Solution {
			get {
				if (_solution == null) {
					_solution = new Solution(SolutionPath);
					_solution.AutoGenerateProjectConfigurationPlatforms = ! MakeBlank;
				}
				return _solution;
			}
			set { _solution = value; }
		}

		public virtual Response Invoke() {
			if (Global.Help) return new Response(HelpText);
			ParseOptions();

			if (Request.Arguments.Length == 0)
				return CreateNewSolution();

			var args          = Request.Arguments.ToList();
			var subCommand    = args.First(); args.RemoveAt(0);
			Request.Arguments = args.ToArray();

			switch (subCommand.ToLower()) {
				case "add":  return AddProject();
				case "rm":   return RemoveProject();
				case "info": return PrintInfo();
				default:
					SolutionName = subCommand;
					return CreateNewSolution(); // clide sln Foo <-- try to make a Foo solution
			}
		}

		public static bool IsAbsolutePath(string path) {
			return Path.GetFullPath(path) == path;
		}

		/// <summary>Helper to get a Project from an argument, eg. 'Foo' or 'src/Hi.csproj' or 'src'</summary>
		public Project GetProjectOnFileSystem(string projectName) {
			if (IsAbsolutePath(projectName) && File.Exists(projectName))
				return new Project(projectName);

			var path = Path.Combine(Path.GetFullPath(Global.WorkingDirectory), projectName);
			if (File.Exists(path))
				return new Project(path);

			path += ".csproj";
			if (File.Exists(path))
				return new Project(path);

			return Solution.Projects.FirstOrDefault(p => p.SolutionName == projectName);
		}

		public Project GetProjectFromSolution(string projectName) {
			// First, try the normal file system approach ... if it works, try to find a matching project in the solution
			var project = GetProjectOnFileSystem(projectName);
			if (project != null && project.Exists())
				project = Solution.Projects.FirstOrDefault(p => Path.GetFullPath(p.Path) == Path.GetFullPath(project.Path));
			if (project != null) return project;

			// Try using the project name (via solution or AssemblyName in the options)
			project = Solution.Projects.FirstOrDefault(p => p.Name == projectName);
			if (project != null) return project;

			// Try using the project name specified in the solution
			project = Solution.Projects.FirstOrDefault(p => p.SolutionName == projectName);
			if (project != null) return project;

			// Maybe the relative path matches?
			project = Solution.Projects.FirstOrDefault(p => p.RelativePath == Project.NormalizePath(projectName));
			if (project != null) return project;

			return null;
		}

		public Response AddProject() {
			if (Request.Arguments.Length != 1)
				return new Response("add expects 1 argument (project)");

			var project = GetProjectOnFileSystem(Request.Arguments.First());
			if (project == null || project.DoesNotExist())
				return new Response("Project not found: {0}", Request.Arguments.First());

			Solution.Add(project);
			Solution.Save();

			return new Response("Added {0} to Solution", project.Name);
		}

		public Response RemoveProject() {
			if (Request.Arguments.Length != 1)
				return new Response("add expects 1 argument (project)");

			var project = GetProjectFromSolution(Request.Arguments.First());
			if (project == null || project.DoesNotExist())
				return new Response("Project not found: {0}", Request.Arguments.First());

			Solution.Remove(project);
			Solution.Save();

			return new Response("Removed {0} from Solution", project.Name);
		}

		public Response PrintInfo(Solution sln = null) {
			if (sln == null && Global.Solution != null) sln = new Solution(Global.Solution);
			var response = new Response();

			if (sln.Projects.Count == 0)
				response.Append("No projects in this Solution\n");
			else
				foreach (var project in sln.Projects)
					response.Append("{0} [{1}]\n", project.SolutionName, project.RelativePath);

			return response;
		}

		public Response CreateNewSolution() {
			if (Solution.Exists()) {
				var response = PrintInfo(Solution);
				response.PrependToOutput("Project already exists: {0}\n\n", SolutionName);
				return response;
			}

			if (! MakeBlank && ! string.IsNullOrEmpty(Global.Project)) {
				var project = new Project(Global.Project);
				if (project.Exists())
					Solution.Add(project);
			}

			foreach (var project in ProjectsToAdd)
				if (File.Exists(project))
					Solution.Projects.Add(new Project(project));

			Solution.Save();

			return new Response("Created new solution: {0}", SolutionName);
		}

		public void ParseOptions() {
			var options = new OptionSet {
				{ "b|blank",    v => MakeBlank    = true  },
				{ "n|name=",    v => SolutionName = v     },
				{ "p|project=", v => ProjectsToAdd.Add(v) }
			};
			var extra = options.Parse(Request.Arguments);
			Request.Arguments = extra.ToArray();
		}
	}
}
