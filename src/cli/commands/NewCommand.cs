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
				var global = project.Configurations.AddGlobalConfiguration();
				global["Platform"]               = "AnyCPU";
				global["ProductVersion"]         = "8.0.30703";
				global["SchemaVersion"]          = "2.0";
				global["ProjectGuid"]            = Guid.NewGuid().ToString().ToUpper().WithCurlies();
				global["OutputType"]             = "Library";   // <--- add option
				global["RootNamespace"]          = projectName; // <--- add option 
				global["AssemblyName"]           = projectName; // <--- add option 
				global["TargetFrameworkVersion"] = "4.0";       // <--- add option
				global["FileAlignment"]          = "512";
				global.GetProperty("Platform").Condition = " '$(Platform)' == '' ";

				var debug = project.Configurations.Add("Debug");
				debug["DebugSymbols"]    = "true";
				debug["DebugType"]       = "full";
				debug["Optimize"]        = "false";
				debug["OutputPath"]      = @"bin\Debug\";
				debug["DefineConstants"] = "DEBUG;TRACE'";
				debug["ErrorReport"]     = "prompt";
				debug["WarningLevel"]    = "4";

				var release = project.Configurations.Add("Release");
				release["DebugType"]       = "pdbonly";
				release["Optimize"]        = "true";
				release["OutputPath"]      = @"bin\Release\";
				release["DefineConstants"] = "TRACE";
				release["ErrorReport"]     = "prompt";
				release["WarningLevel"]    = "4";
			}

			project.Save();

			return new Response("Created new project: {0}", projectName);
		}
	}
}
