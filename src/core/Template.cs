using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using IO.Interfaces;

namespace Clide {

	/// <summary>Represents a single template.  Also has useful static methods for getting a List of Template</summary>
	public class Template : IDirectory {

		public Template(){}
		public Template(string path) {
			Path = path;
		}

		string _usage;

		/// <summary>The path to this Template's directory</summary>
		public virtual string Path { get; set; }

		/// <summary>Returns the path to this template's .clide-template meta file</summary>
		public virtual string ClideTemplateFilePath {
			get { return GetClideTemplateFile(Path); }
		}

		/// <summary>Returns the text from this template's .clide-template meta file</summary>
		/// <remarks>
		/// If the ClideTemplateFilePath cannot be found, this returns an empty string.
		/// This allows you to manually specify that you want to use a directory as a template without making a .clide-template file.
		/// </remarks>
		public virtual string Usage {
			get {
				if (_usage == null)
					_usage = File.Exists(ClideTemplateFilePath) ? File.ReadAllText(ClideTemplateFilePath) : string.Empty;
				return _usage;
			}
		}

		/// <summary>Returns this template's "name."  Returns the directory name.</summary>
		public virtual string Name { get { return this.Name(); } }

		/// <summary>Returns this template's description, read from the .clide-template meta file.  This may be null.</summary>
		/// <remarks>
		/// If the meta file has a line that starts with "Description:", this reads the text after that
		/// </remarks>
		public virtual string Description {
			get {
				var match = Regex.Match(Usage, @"^Description:[\s\r]*(.*)", RegexOptions.Multiline);
				return match.Success ? match.Groups[1].ToString().Trim() : null;
			}
		}

		/// <summary>Returns a template by name (or null) [via Template.All)</summary>
		/// <remarks>This is case-insensitive</remarks>
		public static Template Get(string name) {
			return All.FirstOrDefault(template => template.Name.ToLower() == name.ToLower());
		}

		/// <summary>Returns a list of all Template (based on the CLIDE_TEMPLATES path)</summary>
		public static List<Template> All {
			get { return AllInDirectories(Global.TemplateDirectories.ToArray()); }
		}

		/// <summary>Returns a list of all Template in the given paths.  The first paths take precedence if there are name collisions.</summary>
		public static List<Template> AllInDirectories(params string[] paths) {
			var templates = new Dictionary<string, Template>(); // 1 template per name
			foreach (var path in paths)
				foreach (var template in AllInDirectory(path))
					if (! templates.ContainsKey(template.Name))
						templates.Add(template.Name, template);
			return templates.Values.ToList();
		}

		/// <summary>Returns a list of all Template in the given path</summary>
		/// <remarks>
		/// For a directory to be returned, it MUST have a .clide-template file.
		/// Only top-level directories are searched for .clide-template files.
		/// This does NOT recurse down into subdirectories.
		/// </remarks>
		public static List<Template> AllInDirectory(string path) {
			var templates = new List<Template>();
			if (! Directory.Exists(path)) return templates;
			foreach (var directory in Directory.GetDirectories(path))
				if (IsTemplate(directory))
					templates.Add(new Template(directory));
			return templates;
		}

		/// <summary>Whether or not the given directory appears to be a template (ie. if it has a .clide-template file)</summary>
		public static bool IsTemplate(string directory) {
			return ! string.IsNullOrEmpty(GetClideTemplateFile(directory));
		}

		/// <summary>Returns the full path to the .clide-template or _clide-template file in the given directory (or null)</summary>
		public static string GetClideTemplateFile(string directory) {
			if (File.Exists(System.IO.Path.Combine(directory, ".clide-template")))
				return System.IO.Path.Combine(directory, ".clide-template");
			else if (File.Exists(System.IO.Path.Combine(directory, "_clide-template")))
				return System.IO.Path.Combine(directory, "_clide-template");
			else
				return null;
		}
	}
}
