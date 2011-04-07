using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using ConsoleRack;
using IO.Interfaces;
using Clide.Extensions;

// TODO Global.Project and Global.Solution should be OBJECTS!  We can use ProjectPath and SolutionPath for the strings
namespace Clide {

	/// <summary>Represents abunchof global options and other stuff for clide.exe</summary>
	/// <remarks>
	/// To check if debug mode is enabled, check Global.Debug.  Or Global.Verbose for verbosity.
	/// </remarks>
	public class Global {

		static string _homeDirectory;
		static bool? _isWindows;

		/// <summary>Our global option definitions.</summary>
		/// <remarks>
		/// Global.Verbose is just a shortcut to getting Global.Options["Verbose"].Value;
		/// </remarks>
		public static GlobalOptions DefaultOptions {
			get {
				var dir = System.IO.Directory.GetCurrentDirectory();

				// find the first *.*proj file in the WorkingDirectory
				var project = new Func<object>(() => {
					var csproj = Global.WorkingDirectory.AsDir().Search("*.*proj").FirstOrDefault();
					return (csproj == null) ? null : csproj.Path;
				});

				// find the first *.sln file in the WorkingDirectory
				var solution = new Func<object>(() => {
					var sln = Global.WorkingDirectory.AsDir().Search("*.sln").FirstOrDefault();
					return (sln == null) ? null : sln.Path;
				});

				var templates = @".clide\templates;~\.clide\templates";

				// Lack of indentation is indentional, so it's easy to read the full line
				return new GlobalOptions {
//              Short  Long           Name               ENV variable       Argument    Default    Description
new GlobalOption('V', "verbose",     "Verbosity",        "VERBOSE",         "None",     false,     "Can be set to true or a level, eg. INFO or WARN"),
new GlobalOption('D', "debug",       "Debug",            "DEBUG",           "None",     false,     "If set to true, additional debug data may be available"),
new GlobalOption('C', "config",      "Configuration",    "CONFIG",          "Required", "Debug",   "The project Configuration that you want to use"),
new GlobalOption('G', "global",      "Global",           "GLOBAL",          "None",     false,     "If set to true, this change is applied to all configurations"),
new GlobalOption('P', "project",     "Project",          "PROJECT",         "Required", project,   "Name of project in solution of path to project file (csproj)"),
new GlobalOption('S', "solution",    "Solution",         "SOLUTION",        "Required", solution,  "Path to the .sln solution file"),
new GlobalOption('F', "force",       "Force",            "FORCE",           "None",     false,     "Some options support --force to override warnings, etc"),
new GlobalOption('H', "help",        "Help",             "HELP",            "None",     false,     "If set to true, we want to print out help/usage documentation"),
new GlobalOption('W', "working-dir", "WorkingDirectory", "WORKING_DIR",     "Required", dir,       "Sets the working directory. Defaults to the current directory"),
new GlobalOption('T', "templates",   "TemplatesPath",    "CLIDE_TEMPLATES", "Required", templates, "The PATH used to find directories of clide templates")
				};
			}
		}

		static GlobalOptions _options;

		/// <summary>Reference to our GlobalOptions.  If not set yet, ResetOptions() is called (which sets the GlobalOptions from our defaults)</summary>
		public static GlobalOptions Options {
			get {
				if (_options == null) ResetOptions();
				return _options;
			}
			set { _options = value; }
		}

		/// <summary>Resets Options to our DefaultOptions</summary>
		public static void ResetOptions() {
			Options = DefaultOptions;
		}

		/// <summary>Returns whether or not Debug is currently set</summary>
		public static bool Debug {
			get { return Options["Debug"].ToBool(); }
			set { Options["Debug"].Value = value;   }
		}

		/// <summary>Returns whether or not Help is currently set</summary>
		public static bool Help {
			get { return Options["Help"].ToBool(); }
			set { Options["Help"].Value = value;   }
		}

		/// <summary>Returns whether or not Global is currently set</summary>
		public static bool UseGlobal {
			get { return Options["Global"].ToBool(); }
			set { Options["Global"].Value = value;   }
		}

		/// <summary>Returns the selected Configuration, eg. Debug or Release</summary>
		public static string Configuration {
			get { return Options["Configuration"].ToString(); }
			set { Options["Configuration"].Value = value;   }
		}

		/// <summary>Returns the selected Project file</summary>
		public static string Project {
			get { return Options["Project"].ToString(); }
			set { Options["Project"].Value = value;   }
		}

		/// <summary>Returns the selected Solution file</summary>
		public static string Solution {
			get { return Options["Solution"].ToString(); }
			set { Options["Solution"].Value = value;   }
		}

		/// <summary>Gets or sets the current WorkingDirectory (defaults to the current directory)</summary>
		public static string WorkingDirectory {
			get { return Options["WorkingDirectory"].ToString(); }
			set { Options["WorkingDirectory"].Value = value;     }
		}

		/// <summary>Gets or sets the current raw CLIDE_TEMPLATES value</summary>
		public static string TemplatesPath {
			get { return Options["TemplatesPath"].ToString(); }
			set { Options["TemplatesPath"].Value = value;     }
		}

		/// <summary>Returns whether or not we think we're currently running on windows.  Can be set to override this (eg. for testing)</summary>
		public static bool IsWindows {
			get { return (_isWindows == null) ? Environment.OSVersion.ToString().Contains("Windows") : (bool) _isWindows; }
			set { _isWindows = value; }
		}

		/// <summary>Returns the full system paths to all of the paths defined in TemplatesPath (which is the raw string, eg. "foo;C:\bar"</summary>
		public static List<string> TemplateDirectories {
			get {
				return TemplatesPath.
							Split(';').
							Select(path => path.Trim()).
							Where(path => ! string.IsNullOrEmpty(path)).
							Select(path => RelativeToWorkingDirectory(path)).
							Select(path => IsWindows ? path.Replace("/", "\\") : path.Replace("\\", "/")).
							ToList();
			}
		}

		/// <summary>If the given path is relative, we return the full path relative to the WorkingDirectory.  If it's absolute, we return it.</summary>
		/// <remarks>
		/// NOTE: this also evaluates paths starting with ~ to start with the HomeDirectory
		/// </remarks>
		public static string RelativeToWorkingDirectory(string path) {
			if (Path.IsPathRooted(path))
				return path; // absolute
			if (path.StartsWith("~/") || path.StartsWith("~\\"))
				return Path.Combine(HomeDirectory, path.Substring(2));
			else
				return Path.Combine(WorkingDirectory, path);
		}

		/// <summary>Helper that returns the environment variable with the given name</summary>
		public static string ENV(string environmentVariable) {
			return Environment.GetEnvironmentVariable(environmentVariable);
		}

		/// <summary>Tries to find your user's home directory by looking for HOME, HOMEDRIVE and HOMEPATH, or USERPROFILE</summary>
		public static string HomeDirectory {
			get {
				if (! string.IsNullOrEmpty(_homeDirectory)) // <--- allows for an override if necessary (eg. from the test suite)
					return _homeDirectory;
				else if (ENV("HOME") != null)
					return ENV("HOME");
				else if (ENV("HOMEDRIVE") != null && ENV("HOMEPATH") != null)
					return string.Format("{0}{1}", ENV("HOMEDRIVE"), ENV("HOMEPATH"));
				else if (ENV("USERPROFILE") != null)
					return ENV("USERPROFILE");
				else
					throw new Exception("Could not determine where your home directory is");
			}
			set { _homeDirectory = value; }
		}

		/// <summary>Returns all of Clide's Commands.  Right now, this just defers to Crack.Commands</summary>
		public static CommandList Commands { get { return Crack.Commands; } }

		/// <summary>Generates option text for all of our common options</summary>
		public static string CommonOptionsText {
			get {
				var builder = new StringBuilder("  Common Options:\n");

				var spaces = Global.Options.Select(option => option.ArgumentsText.Length).Max() + 4;
				foreach (var option in Global.Options)
					builder.AppendFormat("    {0}{1}\n", option.ArgumentsText.WithSpaces(spaces), option.Description);

				return builder.ToString();
			}
		}
	}
}
