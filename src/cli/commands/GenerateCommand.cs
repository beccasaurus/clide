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
	/// <summary>clide generate</summary>
	public class GenerateCommand {

		[Command("generate", "Generate a clide template")]
		public static Response GenerateCmd(Request req) { return new GenerateCommand(req).Invoke(); }

		public GenerateCommand(Request request) {
			Request = request;
		}

		string _outputDirectory;

		public virtual Request Request { get; set; }

		public virtual Response Invoke() {
			ParseOptions();

			if (Request.Arguments.Length == 0)
				return PrintAvailableTemplates();
			else if (Request.Arguments.Length == 1)
				return PrintTemplateUsage(Request.Arguments[0]);
			else {
				var args = new List<string>(Request.Arguments);
				var name = args.First(); args.RemoveAt(0);
				return GenerateTemplate(name, args.ToArray());
			}
		}

		public virtual Response PrintAvailableTemplates() {
			var response = new Response();

			var templates = Template.All.OrderBy(template => template.Name.ToLower()).ToList();
			if (templates.Count == 0)
				response.Append("No templates found\n");
			else
				foreach (var template in templates)
					response.Append("{0}: {1}\n", template.Name, template.Description);

			//response.Append("\nTo generate a template, put a .clide-template file into the directory you would like to become a template.");
			//response.Append("\nThen update the CLIDE_TEMPLATES environment variable to include the directory that your directory is in.\n");

			return response;
		}

		public virtual Response PrintTemplateUsage(string templateName) {
			var template = Template.Get(templateName);
			if (template == null) return new Response("Template not found: {0}", templateName);
			return new Response(template.Usage);
		}

		public virtual Response GenerateTemplate(string templateName, string[] arguments) {
			var template = Template.Get(templateName);
			if (template == null) return new Response("Template not found: {0}", templateName);

			var pp = new PP();
			if (! NoProject && ! string.IsNullOrEmpty(Global.Project))
				pp.Project = new Project(Global.Project);

			pp.ProcessDirectory(path: template.Path, outputDir: OutputDirectory, tokens: ArgumentsToTokens(arguments));

			return new Response("Generated template {0}", template.Name);
		}

		/// <summary>Takes arguments like 'foo bar this=that' and turns them into tokens like 'arg1=foo arg2=bar this=that'</summary>
		public virtual Dictionary<string, object> ArgumentsToTokens(string[] arguments) {
			var tokens = new Dictionary<string, object>();

			var i = 1;
			foreach (var arg in arguments) {
				var indexOfEquals = arg.IndexOf("=");
				var propertyName  = (indexOfEquals > -1) ? arg.Substring(0, indexOfEquals)  : arg;
				var propertyValue = (indexOfEquals > -1) ? arg.Substring(indexOfEquals + 1) : null;

				if (propertyValue == null) { // Name actually the value.  We use Arg* as the name ...
					tokens["ARG" + i.ToString()] = propertyName;
					i++;
				} else { // We have a real name and valuu key pair
					tokens[propertyName] = propertyValue;
				}
			}

			return tokens;
		}

		/// <summary>If you pass an --output directory, this returns that.  Else it returns the current WorkingDirectory</summary>
		public virtual string OutputDirectory {
			get { return _outputDirectory ?? Global.WorkingDirectory; }
			set { _outputDirectory = value; }
		}

		public void SetOutputDirectory(string value) {
			if (string.IsNullOrEmpty(value)) return;
			if (Path.IsPathRooted(value))
				OutputDirectory = value;
			else
				OutputDirectory = Path.Combine(Global.WorkingDirectory, value);
		}

		/// <summary>If set to true, we don't use project properties when generating (pp files are still evaluated, but not using the current csproj)</summary>
		public virtual bool NoProject { get; set; }

		public void ParseOptions() {
			var options = new OptionSet {
				{ "o|output=",  v => SetOutputDirectory(v) },
				{ "no-project", v => NoProject       = true }
			};
			var extra = options.Parse(Request.Arguments);
			Request.Arguments = extra.ToArray();
		}
	}
}
