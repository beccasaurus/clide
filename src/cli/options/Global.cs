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
			// new GlobalOption("V", "verbose", "Verbosity", "VERBOSITY", false, "If set to true")
		};
	}
}
