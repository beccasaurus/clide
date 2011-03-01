using System;

namespace Clide {

	/// <summary>Represents abunchof global options for clide.exe</summary>
	/// <remarks>
	/// To check if debug mode is enabled, check Global.Debug.  Or Global.Verbose for verbosity.
	/// </remarks>
	public class Global {

		/// <summary>Our global option definitions.</summary>
		/// <remarks>
		/// Global.Verbose is just a shortcut to getting Global.Options["Verbose"].Value;
		/// </remarks>
		public static GlobalOptions Options = new GlobalOptions {
			new GlobalOption('V', "verbose",  "Verbosity",     "VERBOSE",  false,   "Can be set to true or a level, eg. INFO or WARN"),
			new GlobalOption('D', "debug",    "Debug",         "DEBUG",    false,   "If set to true, additional debug data may be available"),
			new GlobalOption('C', "config",   "Configuration", "CONFIG",   "Debug", "The project Configuration that you want to use"),
			new GlobalOption('G', "global",   "Global",        "GLOBAL",   false,   "If set to true, this change is applied to all configurations"),
			new GlobalOption('P', "project",  "Project",       "PROJECT",  null,    "Name of project in solution of path to project file (csproj)"),
			new GlobalOption('S', "solution", "Solution",      "SOLUTION", null,    "Path to the .sln solution file"),
			new GlobalOption('F', "force",    "Force",         "FORCE",    false,   "Some options support --force to override warnings, etc")
		};
	}
}
