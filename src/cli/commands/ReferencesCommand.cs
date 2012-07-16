using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NDesk.Options;
using ConsoleRack;
using IO.Interfaces;
using Clide.Extensions;

namespace Clide {

	// TODO extract stuff out of here into a base ClideCommand class (for parsing, etc)
	/// <summary>clide references</summary>
	public class ReferencesCommand {

		[Command("references", "Manage a Project's references")]
		public static Response ReferencesCmd(Request req) { return new ReferencesCommand(req).Invoke(); }

		public ReferencesCommand(Request request) {
			Request = request;
		}

		public virtual string HelpText {
			get { return @"
Usage: clide references [add|rm] [gac|dll|csproj]

  Examples:
    clide ref                         Displays all of the GAC/DLL/Project references for the current project
    clide ref -P src/Foo.csproj       Displays all of the references for the provided project
    clide ref add lib/Foo.dll         Adds a reference to Foo.dll to the current project (using relative path)
    clide ref add System.Xml          Adds a reference to the System.Xml assembly in the GAC
    clide ref add spec/Specs.csproj   Adds a project reference to the Specs.csproj project (using relative path)
    clide ref rm lib/Foo.dll          Removes reference to Foo.dll to the current project
    clide ref rm System.Xml           Removes reference to the System.Xml assembly in the GAC
    clide ref rm spec/Specs.csproj    Removes project reference to the Specs.csproj project (using relative path)

COMMON".Replace("COMMON", Global.CommonOptionsText).TrimStart('\n'); }
		}

		public virtual Request Request { get; set; }

		public virtual Response Invoke() {
			if (Global.Help) return new Response(HelpText);
			ParseOptions();

			if (Request.Arguments.Length == 0)
				return PrintReferences();

			var args          = Request.Arguments.ToList();
			var subCommand    = args.First(); args.RemoveAt(0);
			Request.Arguments = args.ToArray();

			switch (subCommand.ToLower()) {
				case "add":  return AddReferences();
				case "rm":   return RemoveReferences();
				default:
					return new Response("Unknown references subcommand: {0}", subCommand);
			}
		}

		public virtual Response PrintReferences() {
			var project = new Project(Global.Project);

			if (project.DoesNotExist())
				return new Response("No project found\n");

            if (project.References.Count == 0 && project.ProjectReferences.Count == 0)
                return new Response("This project has no references");

			var response = new Response();
			if (project.GacReferences.Any()) {
				response.Append("[GAC]\n");
				foreach (var reference in project.GacReferences)
					response.Append("\t{0}\n", reference.FullName);
			}
			if (project.DllReferences.Any()) {
				response.Append("[DLL]\n");
				foreach (var reference in project.DllReferences)
					response.Append("\t{0} => {1}\n", reference.FullName, reference.HintPath);
			}
			if (project.ProjectReferences.Any()) {
				response.Append("[PROJECT]\n");
				foreach (var reference in project.ProjectReferences)
					response.Append("\t{0} => {1}\n", reference.Name, reference.ProjectFile);
			}
			return response;
		}

		public virtual Response AddReferences() {
			// TODO - this really needs to stop and we need to fix this!  Global.Project needs to be a PROJECT OBJECT!
			var project = new Project(Global.Project);
			if (project.DoesNotExist())
				return new Response("No project found"); // this should use STERR ... Helper: new Response("", error: true) .... or Response.Error()

			if (Request.Arguments.Length == 0)
				return new Response("No references passed to add?");

			var response = new Response();
			foreach (var reference in Request.Arguments) {
				AddReference(response, reference, project);
			}

			project.Save();
			return response;
		}

		public virtual void AddReference(Response response, string reference, Project project) {
			project.AddReference(reference, response); // <--- it's not ideal, but a project actually knows how to write to our response for this ...
		}

		public virtual Response RemoveReferences() {
			return new Response("This would REMOVE references");
		}

		public void ParseOptions() {}
	}
}
