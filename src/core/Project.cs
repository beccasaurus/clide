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

		Guid?       _id;
		string      _relativePath, _name;
		XmlDocument _doc;

		/// <summary>This project's ProjectGuid ID</summary>
		public virtual Guid? Id {
			get {
				if (this.Exists() && ProjectGuid != null)
					return new Guid(ProjectGuid);
				else
					return _id;
			}
			set {
				if (this.Exists())
					ProjectGuid = value.ToString();
				else
					_id = value;
			}
		}

		/// <summary>The project's ProjectType Guid (nearly always the same)</summary>
		public virtual Guid? ProjectTypeId { get; set; }

		/// <summary>The file system path to this project file, relative to the Solution.</summary>
		public virtual string RelativePath {
			get { return _relativePath; }
			set { 
				if (File.Exists(value)) Path = value;
				_relativePath = NormalizePath(value).TrimStart(@"\/".ToCharArray());
			}
		}

		/// <summary>The real file system path to this project file.</summary>
		/// <remarks>
		/// May be relative (typically to a Solution) but this is a *real* system path.  It is not normalized.
		/// </remarks>
		public virtual string Path { get; set; }

		/// <summary>This project's "Name."  This is really just an alias to the AssemblyName (and SolutionName).</summary>
		public virtual string Name {
			get {
				return (Global == null) ? SolutionName : AssemblyName;
			}
			set {
				if (SolutionName == null) SolutionName = value;
				if (Global       != null) AssemblyName = value;
			}
		}

		/// <summary>If this project is referenced in a Solution, this is this project's "Name" in the solution</summary>
		/// <remarks>
		/// So a project has a Name *and* a SolutionName?  WTF?
		///
		/// Well ... for awhile, we were sharing Project.Name with the name we use in a Solution, but that got icky.
		///
		/// It's a much cleaner solution to simply store the custom Solution name for a project in this different property.
		///
		/// Typically, you can just use project.Name but, if you need the solution's exact name for this project, use this.
		/// </remarks>
		public virtual string SolutionName { get; set; }

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

		/// <summary>Sets the default attributes on the Project node (DefaultTargets and ToolsVersion)</summary>
		public virtual void SetDefaultProjectAttributes() {
			DefaultTargets = "Build";
			ToolsVersion   = "4.0";
		}

		/// <summary>Assembly references (from the GAC or a path to a specific DLL)</summary>
		public virtual ProjectReferences References { get { return new ProjectReferences(this); } }

		/// <summary>References to other projects (eg. csproj's)</summary>
		public virtual ProjectProjectReferences ProjectReferences { get { return new ProjectProjectReferences(this); } }

		/// <summary>Source files to compile</summary>
		public virtual ProjectCompilePaths CompilePaths { get { return new ProjectCompilePaths(this); } }

		/// <summary>Content (to include but not compile)</summary>
		public virtual ProjectContent Content { get { return new ProjectContent(this); } }

		/// <summary>Imports of MSBuild targets</summary>
		public virtual ProjectTargetImports TargetImports { get { return new ProjectTargetImports(this); } }

		/// <summary>MSBuild targets (this does NOT look at any imports, just adds the nodes)</summary>
		public virtual ProjectImports Imports { get { return new ProjectImports(this); } }

		/// <summary>Adds the default MSBuild import that all C# projects use</summary>
		public virtual Import AddDefaultCSharpImport() {
			return Imports.Add(@"$(MSBuildBinPath)\Microsoft.CSharp.targets");
		}

		/*
		/// <summary>Resources to embed into this project when compiled</summary>
		public virtual ProjectEmbeddedResources EmbeddedResources { get { return new ProjectEmbeddedResources(this); } }

		/// <summary>Explicitly included folders</summary>
		public virtual ProjectFolders Folders { get { return new ProjectFolders(this); } }
		*/

		/// <summary>This project's configurations</summary>
		public virtual ProjectConfigurations Configurations { get { return new ProjectConfigurations(this); } }

		/// <summary>Shortcut for Configurations</summary>
		public virtual ProjectConfigurations Config { get { return Configurations; } }

		/// <summary>Shortcut to getting a configuration's properties</summary>
		public virtual ConfigurationProperties PropertiesFor(string configurationName) {
			return Configurations.PropertiesFor(configurationName);
		}

		/// <summary>Shortcut to getting the "global" configuration</summary>
		public virtual Configuration Global { get { return Configurations.Global; } }

		/// <summary>Shortcut to getting the "global" configuration's properties</summary>
		public virtual ConfigurationProperties GlobalProperties { get { return Configurations.Global.Properties; } }

		/// <summary>Shortcut to Global property "OutputType"</summary>
		public virtual string OutputType {
			get { return Global["OutputType"];  }
			set { Global["OutputType"] = value; }
		}

		/// <summary>Shortcut to Global property "RootNamespace"</summary>
		public virtual string RootNamespace {
			get { return Global["RootNamespace"];  }
			set { Global["RootNamespace"] = value; }
		}

		/// <summary>Shortcut to Global property "AssemblyName"</summary>
		public virtual string AssemblyName {
			get { return Global["AssemblyName"];  }
			set { Global["AssemblyName"] = value; }
		}

		/// <summary>Shortcut to Global property "TargetFrameworkVersion"</summary>
		public virtual string TargetFrameworkVersion {
			get { return Global["TargetFrameworkVersion"];  }
			set { Global["TargetFrameworkVersion"] = value; }
		}

		/// <summary>Shortcut to Global property "ProjectGuid"</summary>
		public virtual string ProjectGuid {
			get { return Global["ProjectGuid"];  }
			set { Global["ProjectGuid"] = value; }
		}

		/// <summary>Persists any changes we've made to the XML Doc (eg. using AddReference) to disk (saves to Path)</summary>
		public virtual Project Save() {
			Directory.CreateDirectory(System.IO.Path.GetDirectoryName(this.FullPath())); // make sure the directory exists
			Doc.SaveToFile(Path);
			return this;
		}

		/// <summary>Returns the XML representation of this Project's XmlDocument (which is persists itself to)</summary>
		public virtual string ToXml() {
            return Doc.ToXml().TrimStart((char) 65279); // On Windows, we get a weird little invisible char at the beginning
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
