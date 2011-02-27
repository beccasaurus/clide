using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using IO.Interfaces;

namespace NVS {

	/// <summary>Represents a project file, eg. a csproj file</summary>
	/// <remarks>
	/// If we start to support many different *proj files and they require different 
	/// implementations of certain things, we'll use this as a base class and move 
	/// custom stuff into classes like CsProj and VbProj etc.
	/// </remarks>
	public class Project : IFile {

		public static readonly Guid TypicalProjectTypeGuid = new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");

		/// <summary>Empty constructor.</summary>
		/// <remarks>
		/// Sets some defaults, eg. Generates a Guid for Id and uses the typical ProjectTypeId
		/// </remarks>
		public Project() {
			Id            = Guid.NewGuid();
			ProjectTypeId = Project.TypicalProjectTypeGuid;
		}

		/// <summary>Creates a Project with the given Path.  If the file is found, we will parse it.</summary>
		public Project(string path) {
			Path = path;
			Parse();
		}

		string _path;
		XmlDocument _doc;
		List<Configuration> _configurations;

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

		/// <summary>This file, represented as an XmlDocument.  If the path does not exist, this will be null;</summary>
		public virtual XmlDocument Doc {
			get { if (_doc == null) Parse(); return _doc; }
		}

		/// <summary>All of this project's configurations (eg. "Debug|x86")</summary>
		public virtual List<Configuration> Configurations {
			get { if (_configurations == null) Parse(); return _configurations; }
		}

		public virtual Project Parse() {
			_doc            = new XmlDocument();
			_configurations = new List<Configuration>();
			
			if (this.DoesNotExist()) return this;

			Doc.Load(Path);

			// ... i want to pull in my XmlNode and XmlDocument extensions ...

			return this;
		}

	// private

		/// <summary>.sln and .csproj files seem to use the '\' path separator, regardless of platform.  we currently replace ALL '/' with '\'</summary>
		static string NormalizePath(string path) {
			return path.Replace("/", "\\");
		}
	}
}
