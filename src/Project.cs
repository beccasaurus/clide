using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using IO.Interfaces;
using FluentXml;

namespace NVS {

	/// <summary>Represents a project file, eg. a csproj file</summary>
	/// <remarks>
	/// If we start to support many different *proj files and they require different 
	/// implementations of certain things, we'll use this as a base class and move 
	/// custom stuff into classes like CsProj and VbProj etc.
	/// </remarks>
	public class Project : IFile {

		public static readonly Guid TypicalProjectTypeGuid = new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");

		static readonly Regex _getConfigurationNameAndPlatform = new Regex(@"==\s*'(\w+)\|(\w+)'");

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

		string _relativePath;
		XmlDocument _doc;
		List<Configuration> _configurations;
		List<Reference> _references;

		/// <summary>This project's ProjectGuid ID</summary>
		public virtual Guid? Id { get; set; }

		/// <summary>The project's ProjectType Guid (nearly always the same)</summary>
		public virtual Guid? ProjectTypeId { get; set; }

		/// <summary>The file system path to this project file, relative to the Solution.</summary>
		public virtual string RelativePath {
			get { return _relativePath; }
			set { 
				if (File.Exists(value)) Path = value;
				_relativePath = NormalizePath(value);
			}
		}

		/// <summary>The real file system path to this project file.</summary>
		/// <remarks>
		/// May be relative (typically to a Solution) but this is a *real* system path.  It is not normalized.
		/// </remarks>
		public virtual string Path { get; set; }

		/// <summary>This project's "Name."  If this project has a Solution, this is set by that.  Else we use the project's AssemblyName.</summary>
		public virtual string Name { get; set; }

		/// <summary>If this Project was loaded by a Solution, this is a reference to that Solution.  May be null.</summary>
		public virtual Solution Solution { get; set; }

		/// <summary>This file, represented as an XmlDocument.  If the path does not exist, this will be null;</summary>
		public virtual XmlDocument Doc {
			get { if (_doc == null) Parse(); return _doc; }
		}

		/// <summary>All of this project's configurations (eg. "Debug|x86").  This is READ-ONLY.</summary>
		public virtual List<Configuration> Configurations {
			get { if (_configurations == null) Parse(); return _configurations; }
		}

		/// <summary>All of this project's references.  This is READ-ONLY.</summary>
		public virtual List<Reference> References {
			get { if (_references == null) Parse(); return _references; }
		}

		/// <summary>Adds a reference to </summary>
		public virtual Project AddReference(Reference reference) {
			// TODO fix!  right now, to make sure this works, we add a new item group for each new reference ...

			var group = Doc.Node("Project").NewNode("ItemGroup");
			var node  = group.NewNode("Reference");

			node.Attr("Include", reference.FullName);

			if (reference.HintPath != null) {
				node.NewNode("SpecificVersion").Text(reference.SpecificVersion.ToString());
				node.NewNode("HintPath").Text(reference.HintPath);
			}

			// add to our local references
			References.Add(reference);

			return this;
		}

		/// <summary>Persists any changes we've made to the XML Doc (eg. using AddReference) to disk (saves to Path)</summary>
		public virtual Project Save() {
			Doc.SaveToFile(Path);

			return this;
		}

		/// <summary>Parse (or re-Parse) this project file (if it exists).</summary>
		/// <remarks>
		/// This re-reads the file and re-parses references, configurations, etc.
		/// </remarks>
		public virtual Project Parse() {
			_doc            = new XmlDocument();
			_configurations = new List<Configuration>();
			_references     = new List<Reference>();

			if (this.DoesNotExist()) return this;

			Doc.Load(Path);

			// Each "Configuration" is defined by a PropertyGroup with a Condition, eg:
			// <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|x86' ">
			foreach (var group in Doc.Nodes("PropertyGroup")) {
				var condition = group.Attr("Condition");
				if (condition == null) continue;

				var match = _getConfigurationNameAndPlatform.Match(condition);
				if (match.Success)
					Configurations.Add(new Configuration {
						Name     = match.Groups[1].ToString(),
						Platform = match.Groups[2].ToString()
					});
			}

			// Get each <Reference> node under an <ItemGroup>
			foreach (var node in Doc.Nodes("ItemGroup Reference")) {
				var version   = node.Node("SpecificVersion").Text();
				var hintPath  = node.Node("HintPath").Text();
				var reference = new Reference {
					FullName        = node.Attr("Include"),
					HintPath        = hintPath,
					SpecificVersion = (version == null) ? false : bool.Parse(version)
				};
				References.Add(reference);
			}

			return this;
		}

	// private

		/// <summary>.sln and .csproj files seem to use the '\' path separator, regardless of platform.  we currently replace ALL '/' with '\'</summary>
		static string NormalizePath(string path) {
			return path.Replace("/", "\\");
		}
	}
}
