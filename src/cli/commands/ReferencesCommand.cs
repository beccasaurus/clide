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
	/// <summary>clide references</summary>
	public class ReferencesCommand {

		[Command("references", "Manage a Project's references")]
		public static Response ReferencesCmd(Request req) { return new ReferencesCommand(req).Invoke(); }

		public ReferencesCommand(Request request) {
			Request = request;
		}

		public virtual Request Request { get; set; }

		public virtual Response Invoke() {
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
			return new Response("This would print out references");
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
			var path = Path.Combine(Global.WorkingDirectory, reference);
			if (path.AsFile().DoesNotExist()) {
				project.References.AddGacReference(reference);
				response.Append("Added reference {0} to {1}\n", reference, project.Name);
				return;
			}

			// It's a MSBuild project file?
			if (reference.ToLower().EndsWith("proj")) {
				var referencedProject = new Project(reference);
				var projectDir        = Path.GetFullPath(project.Path).AsFile().DirName();
				Console.WriteLine("dir: {0}", projectDir.AsDir());
				Console.WriteLine("reference: {0}", reference);
				Console.WriteLine("relative ref: {0}", projectDir.AsDir().Relative(reference));
				var relativePath      = projectDir.AsDir().Relative(reference).TrimStart('/');
				project.ProjectReferences.Add(referencedProject.Name, relativePath, referencedProject.Id);
				response.Append("Added reference {0} to {1}\n", referencedProject.Name, project.Name);
				return;
			}

			Assembly assembly;

			// Try to read the assembly info to populate the Reference.FullName (<Reference Include="" />
			try {
				assembly = Assembly.ReflectionOnlyLoadFrom(path);
			} catch (Exception ex) {
				project.References.AddDll(Path.GetFileName(reference), reference);
				response.Append("Couldn't load assembly: {0}.  Adding anyway.  Error: {1}\n", reference, ex.Message);
				response.Append("Added reference {0} to {1}\n", reference, project.Name);
				return;
			}
			
			// The assembly loaded properly
			project.References.AddDll(assembly.FullName, reference);
			response.Append("Added reference {0} to {1}\n", assembly.GetName().Name, project.Name);
		}

		public virtual Response RemoveReferences() {
			return new Response("This would REMOVE references");
		}

		public void ParseOptions() {}
	}
}
