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
			Request         = req;
			SourcesToAdd    = new List<string>();
			ContentToAdd    = new List<string>();
			ReferencesToAdd = new List<string>();
		}

		public virtual Request Request { get; set; }

		public virtual string HelpText {
			get { return @"
Usage: clide new [ProjectName] [options]

  If the ProjectName isn't specified, the folder name of the current directory is used

  Options:
    -b, --bare       Creates a bare csproj with just a <Project> node
    -e, --exe        Sets project OutputType to exe
    -w, --winexe     Sets project OutputType to winexe
    -l, --library    Sets project OutputType to library
    -s, --source     Define source files (same as clide source add)
    -c, --content    Define content files (same as clide content add)
    -r, --reference  Define references (same as clide ref add)

COMMON".Replace("COMMON", Global.CommonOptionsText).TrimStart('\n'); }
		}

		public virtual List<string> SourcesToAdd    { get; set; }
		public virtual List<string> ContentToAdd    { get; set; }
		public virtual List<string> ReferencesToAdd { get; set; }

		/// <summary>Right now, everything is stuffed into this method ... we'll organize into properties and whatnot later ...</summary>
		public Response Invoke() {
			if (Global.Help) return new Response(HelpText);

			var bare       = false;
			var outputType = "Exe";

			var options = new OptionSet {
				{ "b|bare",       v => bare       = true      },
				{ "e|exe",        v => outputType = "Exe"     },
				{ "w|winexe",     v => outputType = "WinExe"  },
				{ "l|library",    v => outputType = "Library" },
				{ "s|source=",    v => SourcesToAdd.Add(v)    },
				{ "c|content=",   v => ContentToAdd.Add(v)    },
				{ "r|reference=", v => ReferencesToAdd.Add(v) }
			};
			var extra = options.Parse(Request.Arguments);

			var projectPath = (extra.Count > 0) ? extra.First() : Path.GetFileName(Path.GetFullPath(Global.WorkingDirectory));
			var projectName = Regex.Replace(Path.GetFileName(projectPath), @"\.\w\wproj$",  ""); // if it ends with .xxproj, get rid of that part
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

			foreach (var source    in SourcesToAdd)    project.CompilePaths.Add(include: source);
			foreach (var content   in ContentToAdd)    project.Content.Add(include: content);
			foreach (var reference in ReferencesToAdd) project.AddReference(reference);

			project.Save();

			return new Response("Created new project: {0}", projectName);
		}
	}
}
