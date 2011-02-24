using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace NVS {

	/// <summary>Represents a project file, eg. a csproj file</summary>
	/// <remarks>
	/// If we start to support many different *proj files and they require different 
	/// implementations of certain things, we'll use this as a base class and move 
	/// custom stuff into classes like CsProj and VbProj etc.
	/// </remarks>
	public class Project {

		public static readonly Guid TypicalProjectTypeGuid = new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");

		/// <summary>Empty constructor.</summary>
		/// <remarks>
		/// Sets some defaults, eg. Generates a Guid for Id and uses the typical ProjectTypeId
		/// </remarks>
		public Project() {
			Id            = Guid.NewGuid();
			ProjectTypeId = Project.TypicalProjectTypeGuid;
		}

		string _path;

		/// <summary>This project's ProjectGuid ID</summary>
		public virtual Guid? Id { get; set; }

		/// <summary>The project's ProjectType Guid (nearly always the same)</summary>
		public virtual Guid? ProjectTypeId { get; set; }

		/// <summary>The file system path to this project file.  May be relative (typically to a Solution)</summary>
		public virtual string Path { get { return _path; } set { _path = NormalizePath(value); } }

		/// <summary>This project's "Name."  If this project has a Solution, this is set by that.  Else we use the project's AssemblyName.</summary>
		public virtual string Name { get; set; }

		/// <summary>If this Project was loaded by a Solution, this is a reference to that Solution.  May be null.</summary>
		public virtual Solution Solution { get; set; }

		/// <summary>.sln and .csproj files seem to use the '\' path separator, regardless of platform.  we currently replace ALL '/' with '\'</summary>
		static string NormalizePath(string path) {
			return path.Replace("/", "\\");
		}
	}
}
