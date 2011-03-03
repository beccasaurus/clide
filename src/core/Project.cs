using System;
using System.IO;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using IO.Interfaces;
using FluentXml;

namespace Clide {

	/// <summary>Represents a project file, eg. a csproj file</summary>
	/// <remarks>
	/// If we start to support many different *proj files and they require different 
	/// implementations of certain things, we'll use this as a base class and move 
	/// custom stuff into classes like CsProj and VbProj etc.
	/// </remarks>
	public class Project : IFile, IXmlNode {

		/// <summary>The "Project Type" GUID that every Visual Studio / MSBuild project seems to use</summary>
		public static readonly Guid TypicalProjectTypeGuid = new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC");

		/// <summary>The code XML that we make new projects with.  Nothing more than an XML declaration and Project node</summary>
		public static readonly string BlankProjectXML = 
			"<?xml version=\"1.0\" encoding=\"utf-8\"?><Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"></Project>";

		/// <summary>Empty constructor.</summary>
		/// <remarks>
		/// Sets some defaults, eg. Generates a Guid for Id and uses the typical ProjectTypeId
		/// </remarks>
		public Project() {
			Id            = Guid.NewGuid();
			ProjectTypeId = Project.TypicalProjectTypeGuid;
		}

		/// <summary>Creates a Project with the given Path.  If the file is found, we will parse it.</summary>
		public Project(string path) : this() {
			Path = path;
		}

		string                _relativePath;
		XmlDocument           _doc;
		ProjectReferences     _references;
		ProjectConfigurations _configurations;

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
			get { if (_doc == null) Reload(); return _doc; }
			set { _doc = value; }
		}

		/// <summary>IXmlNode implementation</summary>
		public virtual XmlNode Node { get { return Doc as XmlNode; } }

		/// <summary>The root Project node</summary>
		public virtual XmlNode ProjectNode { get { return Doc.Node("Project"); } }

		/// <summary>Get or set the DefaultTargets attribute on the code Project node</summary>
		public virtual string DefaultTargets {
			get { return ProjectNode.Attr("DefaultTargets"); }
			set { ProjectNode.Attr("DefaultTargets", value); }
		}

		/// <summary>Get or set the ToolsVersion attribute on the code Project node</summary>
		public virtual string ToolsVersion {
			get { return ProjectNode.Attr("ToolsVersion"); }
			set { ProjectNode.Attr("ToolsVersion", value); }
		}

		/// <summary>This project's references</summary>
		public virtual ProjectReferences References {
			get { return _references ?? (_references = new ProjectReferences(this)); }
			set { _references = value; }
		}

		/// <summary>This project's configurations</summary>
		public virtual ProjectConfigurations Configurations {
			get { return _configurations ?? (_configurations = new ProjectConfigurations(this)); }
			set { _configurations = value; }
		}

		/// <summary>Shortcut for Configurations</summary>
		public virtual ProjectConfigurations Config {
			get { return Configurations; }
			set { Configurations = value; }
		}

		/// <summary>Shortcut to getting a configuration's properties</summary>
		public virtual ConfigurationProperties PropertiesFor(string configurationName) {
			return Configurations.PropertiesFor(configurationName);
		}

		/// <summary>Shortcut to getting the "global" configuration</summary>
		public virtual Configuration Global { get { return Configurations.Global; } }

		/// <summary>Shortcut to getting the "global" configuration's properties</summary>
		public virtual ConfigurationProperties GlobalProperties { get { return Configurations.Global.Properties; } }

		/// <summary>Persists any changes we've made to the XML Doc (eg. using AddReference) to disk (saves to Path)</summary>
		public virtual Project Save() {
			Doc.SaveToFile(Path);
			return this;
		}

		/// <summary>Returns the XML representation of this Project's XmlDocument (which is persists itself to)</summary>
		public virtual string ToXml() {
			return Doc.ToXml();
		}

		/// <summary>Parse (or re-Parse) this project file (if it exists).</summary>
		/// <remarks>
		/// This re-reads the file and re-parses references, configurations, etc.
		/// </remarks>
		public virtual Project Reload() {
			Doc = this.Exists() ? FluentXmlDocument.FromFile(Path) : FluentXmlDocument.FromString(BlankProjectXML);
			return this;
		}

		/// <summary>.sln/.csproj files seem to use the \ path separator, regardless of platform. we currently replace ALL / with \</summary>
		public static string NormalizePath(string path) {
			if (path == null) return null;
			return path.Replace("/", "\\");
		}
	}
}
