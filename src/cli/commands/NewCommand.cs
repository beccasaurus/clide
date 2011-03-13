using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Options;
using ConsoleRack;
using Clide.Extensions;

namespace Clide {

	/// <summary>clide new</summary>
	public class NewCommand {

		[Command("new", "...")]
		public static Response Invoke(Request req) { return new NewCommand(req).Invoke(); }

		public NewCommand(Request req) {
			Request = req;
		}

		public virtual Request Request { get; set; }

		/// <summary>Right now, everything is stuffed into this method ... we'll organize into properties and whatnot later ...</summary>
		public Response Invoke() {
			if (Global.Help || Request.Arguments.Length == 0)
				return new Response("Helpful information about the 'new' command");

			var bare = false;

			var options = new OptionSet {
				{ "b|bare", v => bare = true },
			};
			var extra = options.Parse(Request.Arguments);

			var projectName = Request.Arguments[0];
			var project = new Project(Path.Combine(Global.WorkingDirectory, projectName + ".csproj"));

			// Unless you specify --bare, we currently specify all of the usual default options (in code, NOT using a template)
			if (! bare) {
				project.Configurations.AddGlobalConfiguration().AddDefaultGlobalProperties(Guid.NewGuid(), "4.0", "Library", projectName, projectName);
				project.Configurations.Add("Debug").AddDefaultDebugProperties();
				project.Configurations.Add("Release").AddDefaultReleaseProperties();
			}

			project.Save();

			return new Response("Created new project: {0}", projectName);
		}
	}
}
