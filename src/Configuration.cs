using System;

namespace NVS {

	/// <summary>Represents a Project's configuration, eg. "Debug|x86"</summary>
	/// <remarks>
	/// In MSBuild project files (eg. csproj), configurations aren't explicitly defined.
	///
	/// IDEs like Visual Studio and MonoDevelop persist different configurations as PropertyGroup 
	/// elements in project files with a Condition that matches the configuration's and platform.
	///
	/// This pretty much just wraps that Name and Platform.
	/// </remarks>
	public class Configuration {

		/// <summary>The Project that this configuration is for</summary>
		public virtual Project Project { get; set; }

		/// <summary>This configuration's name, eg. "Debug" or "Release"</summary>
		public virtual string Name { get; set; }

		/// <summary>This configuration's platform, eg. "x86" or "AnyCPU"</summary>
		public virtual string Platform { get; set; }

		/// <summary>String representation of this configuration, eg. "Debug|x86"</summary>
		public override string ToString() { return string.Format("{0}|{1}", Name, Platform); }
	}
}
