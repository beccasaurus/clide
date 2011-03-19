using System;
using System.Linq;
using ConsoleRack;
using IO.Interfaces;

// TODO Global.Project and Global.Solution should be OBJECTS!  We can use ProjectPath and SolutionPath for the strings
namespace Clide {

	/// <summary>Represents abunchof global options and other stuff for clide.exe</summary>
	/// <remarks>
	/// To check if debug mode is enabled, check Global.Debug.  Or Global.Verbose for verbosity.
	/// </remarks>
	public class Global {

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

				// Lack of indentation is indentional, so it's easy to read the full line
				return new GlobalOptions {
//              Short  Long           Name               ENV variable   Argument    Default   Description
new GlobalOption('V', "verbose",     "Verbosity",        "VERBOSE",     "None",     false,    "Can be set to true or a level, eg. INFO or WARN"),
new GlobalOption('D', "debug",       "Debug",            "DEBUG",       "None",     false,    "If set to true, additional debug data may be available"),
new GlobalOption('C', "config",      "Configuration",    "CONFIG",      "Required", "Debug",  "The project Configuration that you want to use"),
new GlobalOption('G', "global",      "Global",           "GLOBAL",      "None",     false,    "If set to true, this change is applied to all configurations"),
new GlobalOption('P', "project",     "Project",          "PROJECT",     "Required", project,  "Name of project in solution of path to project file (csproj)"),
new GlobalOption('S', "solution",    "Solution",         "SOLUTION",    "Required", solution, "Path to the .sln solution file"),
new GlobalOption('F', "force",       "Force",            "FORCE",       "None",     false,    "Some options support --force to override warnings, etc"),
new GlobalOption('H', "help",        "Help",             "HELP",        "None",     false,    "If set to true, we want to print out help/usage documentation"),
new GlobalOption('W', "working-dir", "WorkingDirectory", "WORKING_DIR", "Required", dir,      "Sets the working directory. Defaults to the current directory")
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

		/// <summary>Returns all of Clide's Commands.  Right now, this just defers to Crack.Commands</summary>
		public static CommandList Commands { get { return Crack.Commands; } }
	}
}
