using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Options;
using ConsoleRack;
using IO.Interfaces;
using Clide.Extensions;

namespace Clide {

	/// <summary>clide solution</summary>
	public class SolutionCommand {

		[Command("sln", "Alias for 'solution'")]
		public static Response SlnCommand(Request req){ return SolutionCommand.Invoke(req); }

		[Command("solution", "Create/Edit solution files")]
		public static Response Invoke(Request req) {
			var solutionName = (req.Arguments.Length > 0) ? req.Arguments[0] : Path.GetFileName(Path.GetFullPath(Global.WorkingDirectory));	

			var sln = new Solution(Path.Combine(Global.WorkingDirectory, solutionName + ".sln"));

			if (Global.Project != null) {
				var project = new Project(Global.Project);
				if (project.Exists())
					sln.Add(project);
			}

			sln.Save();

			return new Response("Created new solution: {0}", solutionName);
		}
	}
}
