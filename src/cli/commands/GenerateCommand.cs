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

		public virtual string HelpText {
			get { return @"
Usage: clide generate [Template] [TemplateOptions]

  Examples:
    clide gen                        Displays a list of all templates found in CLIDE_TEMPLATES path
    clide gen Model                  Displays the usage for the Model template (from .clide-template file)
    clide gen Model User             Generates the Model template (with token $Arg1$ replaced with 'User')
    clide gen Model User Foo=""Bar""   Generates the Model template (Arg1: 'User', Foo: 'Bar')

  Options:
    -o, --output              Specify a directory to output this template into.  Defaults to current directory.
        --no-project          Do not use the current project's properties when replacing tokens in .pp files
        --missing-tokens-ok   Don't skip generation of files/dirs with tokens in their name (which aren't passed)

  Adding templates:
    To generate a template, put a .clide-template file into the directory you would like to become a template.
    Then update the CLIDE_TEMPLATES environment variable to include the directory that your directory is in.
    By default, we search the current dir and your home dir for a .clide/templates directory with templates.

  Using tokens in templates:
    When we generate a template, we will replace any Project Properties found in file/directory names:
      eg. a file named $RootNamespace$.cs will become [your project's namespace].cs
    If you want us to process these tokens inside of the text of your file, you must add a .pp extension to it.
    Your template can also accept arbitrary tokens:
      eg. clide g Tmpl Foo ""Hi There"" will replace instances of $arg1$ with Foo and instances of $arg2$ with ""Hi There""
      eg. clide g Tmpl Foo This=That will replace instances of $arg1$ with Foo and instances of $This$ with That

  Further help:
    You can view a sample .clide/templates directory at: https://github.com/remi/clide-templates

COMMON".Replace("COMMON", Global.CommonOptionsText).TrimStart('\n'); }
		}

		string _outputDirectory;

		public virtual Request Request { get; set; }

		public virtual Response Invoke() {
			if (Global.Help) return new Response(HelpText);
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
				response.Append("No available templates\n");
			else {
				var spaces = templates.Select(t => t.Name.Length).Max() + 4;
				response.Append("Available templates:\n\n");
				foreach (var template in templates)
					response.Append("    {0}{1}\n", template.Name.WithSpaces(spaces), template.Description);
			}

			return response;
		}

		public virtual Response PrintTemplateUsage(string templateName) {
			var template = GetTemplate(templateName);
			if (template == null) return new Response("Template not found: {0}", templateName);
			return new Response(template.Usage);
		}

		public virtual Response GenerateTemplate(string templateName, string[] arguments) {
			var template = GetTemplate(templateName);
			if (template == null) return new Response("Template not found: {0}", templateName);

			var pp = new PP();
            pp.Excludes.Add(path => new string[]{ ".clide-template", "_clide-template" }.Contains(Path.GetFileName(path)));
            pp.SkipIfMissingTokens = ! MissingTokensOk;
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

        /// <summary>If set to true, we don't skip generation of files/dirs with tokens in their name (which aren't passed)</summary>
        public virtual bool MissingTokensOk { get; set; }

		public void ParseOptions() {
			var options = new OptionSet {
				{ "o|output=",         v => SetOutputDirectory(v)  },
				{ "no-project",        v => NoProject       = true },
                { "missing-tokens-ok", v => MissingTokensOk = true }
			};
			var extra = options.Parse(Request.Arguments);
			Request.Arguments = extra.ToArray();
		}

		public virtual Template GetTemplate(string templateName) {
			var template = Template.Get(templateName);

			// If we can't find this template by name, see if it's a directory with a .clide-template file in it
			if (template == null) {
				var path = Path.IsPathRooted(templateName) ? templateName : Path.Combine(Global.WorkingDirectory, templateName);
				if (Directory.Exists(path) && File.Exists(Path.Combine(path, ".clide-template")))
					template = new Template(path);
			}

			return template;
		}
	}
}
