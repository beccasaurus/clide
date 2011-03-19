using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
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

		public SolutionCommand(Request request) {
			Request = request;
		}

		string _name, _solutionPath;
		Solution _solution;

		public virtual Request Request { get; set; }

		public virtual bool MakeBlank { get; set; }

		public virtual string DirectoryName {
			get { return Path.GetFileName(Path.GetFullPath(Global.WorkingDirectory)); }
		}

		public virtual string SolutionName {
			get { return _name ?? DirectoryName; }
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
			ParseOptions();
			
			if (Solution.Exists())
				return new Response("Project already exists: {0}", SolutionName);

			if (! MakeBlank && ! string.IsNullOrEmpty(Global.Project)) {
				var project = new Project(Global.Project);
				if (project.Exists())
					Solution.Add(project);
			}

			Solution.Save();

			return new Response("Created new solution: {0}", SolutionName);
		}

		public void ParseOptions() {
			var options = new OptionSet {
				{ "b|blank", v => MakeBlank = true },
				{ "n|name=", v => SolutionName = v }
			};
			var extra = options.Parse(Request.Arguments);
			Request.Arguments = extra.ToArray();
		}
	}
}
