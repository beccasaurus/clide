using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mono.Options;
using ConsoleRack;
using Clide.Extensions;

namespace Clide {

	/// <summary>clide new</summary>
	public class NewCommand {

		[Command("new", "Create a new project")]
		public static Response Invoke(Request req) { return new NewCommand(req).Invoke(); }

		public NewCommand(Request req) {
			Request = req;
		}

		public virtual Request Request { get; set; }

		public virtual string HelpText {
			get { return @"
Usage: clide new [ProjectName] [options]

  If the ProjectName isn't specified, the folder name of the current directory is used

  Options:
    -b, --bare       Creates a bare csproj with just a <Project> node
        --exe        Sets project OutputType to exe
        --winexe     Sets project OutputType to winexe
        --library    Sets project OutputType to library

COMMON".Replace("COMMON", Global.CommonOptionsText).TrimStart('\n'); }
		}

		/// <summary>Right now, everything is stuffed into this method ... we'll organize into properties and whatnot later ...</summary>
		public Response Invoke() {
			if (Global.Help) return new Response(HelpText);

			var bare       = false;
			var outputType = "Exe";

			var options = new OptionSet {
				{ "b|bare",  v => bare       = true      },
				{ "exe",     v => outputType = "Exe"     },
				{ "winexe",  v => outputType = "WinExe"  },
				{ "library", v => outputType = "Library" }
			};
			var extra = options.Parse(Request.Arguments);

			var projectPath = (extra.Count > 0) ? extra.First() : Path.GetFileName(Path.GetFullPath(Global.WorkingDirectory));
			var projectName = Path.GetFileNameWithoutExtension(projectPath); // take the last *.csproj part of the path and use * as the name
			var project     = new Project(Path.Combine(Global.WorkingDirectory, Regex.Replace(projectPath, @"\.csproj$", "") + ".csproj"));

			// Unless you specify --bare, we currently specify all of the usual default options (in code, NOT using a template)
			if (! bare) {
				project.SetDefaultProjectAttributes();
				project.Configurations.AddGlobalConfiguration().AddDefaultGlobalProperties(
					root:     projectName,
					assembly: projectName,
					type:     outputType		
				);
				project.Configurations.Add("Debug").AddDefaultDebugProperties();
				project.Configurations.Add("Release").AddDefaultReleaseProperties();
				project.AddDefaultCSharpImport();
			}

			project.Save();

			return new Response("Created new project: {0}", projectName);
		}
	}
}
