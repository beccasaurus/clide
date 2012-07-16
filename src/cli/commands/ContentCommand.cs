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
	/// <summary>clide content</summary>
	public class ContentCommand {

		[Command("content", "Manage a Project's code content files")]
		public static Response ContentCmd(Request req) { return new ContentCommand(req).Invoke(); }

		public ContentCommand(Request request) {
			Request = request;
		}

		public virtual string HelpText {
			get { return @"
Usage: clide content add|rm file1.html file2.txt

  Examples:
    clide content add file.txt          Adds file.txt as a content file
    clide content add stuff/*.txt       Adds all .txt files under stuff (adds each individually)
    clide content add 'stuff/*.txt'     Adds all .txt files under stuff (adds wildcard matcher to txtproj)
    clide content rm file.txt           Removes file.txt from files
    clide content rm stuff/*.txt        Removes each of the .txt files under stuff, individually
    clide content rm 'stuff/*.txt'      Removes this exact wildcard matcher from txtproj

COMMON".Replace("COMMON", Global.CommonOptionsText).TrimStart('\n'); }
		}

		public virtual Request Request { get; set; }

		public virtual Response Invoke() {
			if (Global.Help) return new Response(HelpText);
			ParseOptions();

			if (Request.Arguments.Length == 0)
				return PrintContent();

			var args          = Request.Arguments.ToList();
			var subCommand    = args.First(); args.RemoveAt(0);
			Request.Arguments = args.ToArray();

			switch (subCommand.ToLower()) {
				case "add":  return AddContent();
				case "rm":   return RemoveContent();
				default:
					return new Response("Unknown content subcommand: {0}", subCommand);
			}
		}

		public virtual Response PrintContent() {
			var project = new Project(Global.Project);
			
            if (project.DoesNotExist())
				return new Response("No project found");

            if (project.Content.Count == 0)
                return new Response("This project has no content");

            var response = new Response();
            foreach (var content in project.Content)
                    response.Append("{0}\n", content.Include);
            return response;
		}

		public virtual Response AddContent() {
			// TODO - this really needs to stop and we need to fix this!  Global.Project needs to be a PROJECT OBJECT!
			var project = new Project(Global.Project);
			if (project.DoesNotExist())
				return new Response("No project found"); // this should use STERR ... Helper: new Response("", error: true) .... or Response.Error()

			if (Request.Arguments.Length == 0)
				return new Response("No content passed to add?");

			var response = new Response();
			foreach (var reference in Request.Arguments) {
				if (project.Content.FirstOrDefault(content => content.Include == Project.NormalizePath(reference)) == null) {
					response.Append("Added {0} to {1}\n", reference, project.Name);
					project.Content.Add(include: reference);
				} else
					response.Append("{0} already added to {1}\n", reference, project.Name);
			}

			project.Save();
			return response;
		}

		public virtual Response RemoveContent() {
			// TODO - this really needs to stop and we need to fix this!  Global.Project needs to be a PROJECT OBJECT!
			var project = new Project(Global.Project);
			if (project.DoesNotExist())
				return new Response("No project found"); // this should use STERR ... Helper: new Response("", error: true) .... or Response.Error()

			if (Request.Arguments.Length == 0)
				return new Response("No content passed to add?");

			var response = new Response();
			foreach (var reference in Request.Arguments) {
				var path = project.Content.FirstOrDefault(content => content.Include == Project.NormalizePath(reference));
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
