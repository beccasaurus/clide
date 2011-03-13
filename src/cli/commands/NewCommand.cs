using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Options;
using ConsoleRack;

namespace Clide {

	/// <summary>clide new</summary>
	public class NewCommand {

		[Command("new", "...")]
		public static Response Invoke(Request req) { return new NewCommand(req).Invoke(); }

		public NewCommand(Request req) {
			Request = req;
		}

		public virtual Request Request { get; set; }

		public Response Invoke() {
			if (Global.Help || Request.Arguments.Length == 0)
				return new Response("Helpful information about the 'new' command");

			// var bare = false;

			var projectName = Request.Arguments[0];

			var project = new Project(Path.Combine(Global.WorkingDirectory, projectName + ".csproj"));

			project.Save();

			return new Response("Created new projet: {0}", projectName);
		}

		// public void ParseOptions() {
		// 	var options = new OptionSet {
		// 		{ "b|bare",    v => ProjectTemplate = "bare"    },
		// 		{ "d|default", v => ProjectTemplate = "default" }
		// 	};
		// 	var extra = options.Parse(Request.Arguments);
		// }
	}
}
