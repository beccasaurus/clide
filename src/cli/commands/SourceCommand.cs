using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mono.Options;
using ConsoleRack;
using IO.Interfaces;
using Clide.Extensions;

namespace Clide {

	// TODO extract stuff out of here into a base ClideCommand class (for parsing, etc)
	/// <summary>clide source</summary>
	public class SourceCommand {

		[Command("source", "Manage a Project's code source files")]
		public static Response SourceCmd(Request req) { return new SourceCommand(req).Invoke(); }

		public SourceCommand(Request request) {
			Request = request;
		}

		public virtual string HelpText {
			get { return @"
Usage: clide source add|rm file1.cs file2.cs

  Examples:
    clide source add file.cs          Adds file.cs as a file to compile
    clide source add src/*.cs         Adds all .cs files under src (adds each individually)
    clide source add 'src/*.cs'       Adds all .cs files under src (adds wildcard matcher to csproj)
    clide source rm file.cs           Removes file.cs from files to compile
    clide source rm src/*.cs          Removes each of the .cs files under src, individually
    clide source rm 'src/*.cs'        Removes this exact wildcard matcher from csproj

COMMON".Replace("COMMON", Global.CommonOptionsText).TrimStart('\n'); }
		}

		public virtual Request Request { get; set; }

		public virtual Response Invoke() {
			if (Global.Help) return new Response(HelpText);
			ParseOptions();

			if (Request.Arguments.Length == 0)
				return PrintSource();

			var args          = Request.Arguments.ToList();
			var subCommand    = args.First(); args.RemoveAt(0);
			Request.Arguments = args.ToArray();

			switch (subCommand.ToLower()) {
				case "add":  return AddSource();
				case "rm":   return RemoveSource();
				default:
					return new Response("Unknown source subcommand: {0}", subCommand);
			}
		}

		public virtual Response PrintSource() {
			var project = new Project(Global.Project);
			
            if (project.DoesNotExist())
				return new Response("No project found");

            if (project.CompilePaths.Count == 0)
                return new Response("This project has no source paths");

            var response = new Response();
            foreach (var source in project.CompilePaths)
                    response.Append("{0}\n", source.Include);
            return response;
		}

		public virtual Response AddSource() {
			// TODO - this really needs to stop and we need to fix this!  Global.Project needs to be a PROJECT OBJECT!
			var project = new Project(Global.Project);
			if (project.DoesNotExist())
				return new Response("No project found"); // this should use STERR ... Helper: new Response("", error: true) .... or Response.Error()

			if (Request.Arguments.Length == 0)
				return new Response("No source passed to add?");

			var response = new Response();
			foreach (var reference in Request.Arguments) {
				if (project.CompilePaths.FirstOrDefault(source => source.Include == Project.NormalizePath(reference)) == null) {
					response.Append("Added {0} to {1}\n", reference, project.Name);
					project.CompilePaths.Add(include: reference);
				} else
					response.Append("{0} already added to {1}\n", reference, project.Name);
			}

			project.Save();
			return response;
		}

		public virtual Response RemoveSource() {
			// TODO - this really needs to stop and we need to fix this!  Global.Project needs to be a PROJECT OBJECT!
			var project = new Project(Global.Project);
			if (project.DoesNotExist())
				return new Response("No project found"); // this should use STERR ... Helper: new Response("", error: true) .... or Response.Error()

			if (Request.Arguments.Length == 0)
				return new Response("No source passed to add?");

			var response = new Response();
			foreach (var reference in Request.Arguments) {
				var path = project.CompilePaths.FirstOrDefault(source => source.Include == Project.NormalizePath(reference));
				if (path != null) {
					path.Remove();
					response.Append("Removed {0} from {1}\n", reference, project.Name);
				}
			}

			project.Save();
			return response;
		}

		public void ParseOptions() {}
	}
}
