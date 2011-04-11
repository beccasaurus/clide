using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ConsoleRack;
using Clide;

namespace Clide.Specs {

	[TestFixture]
	public class NewCommandSpec : Spec {

		[Test][Description("clide help new")]
		public void clide_help_new() {
			Clide("help", "new").Text.ShouldContain("Usage: clide new [ProjectName] [options]");
		}

		[Test][Description("clide new")]
		public void clide_new() {
			File.Exists(Temp("tmp.csproj")).Should(Be.False);

			Clide("new"); // assumes directory name as the project name

			File.Exists(Temp("tmp.csproj")).Should(Be.True);

			// defaults to an Exe
			new Project(Temp("tmp.csproj")).OutputType.ShouldEqual("Exe");
		}

		[Test][Description("clide new --exe")]
		public void clide_new_exe() {
			File.Exists(Temp("tmp.csproj")).Should(Be.False);

			Clide("new", "--exe");

			new Project(Temp("tmp.csproj")).OutputType.ShouldEqual("Exe");
		}

		[Test][Description("clide new --library")]
		public void clide_new_library() {
			File.Exists(Temp("tmp.csproj")).Should(Be.False);

			Clide("new", "--library");

			new Project(Temp("tmp.csproj")).OutputType.ShouldEqual("Library");
		}

		[Test][Description("clide new --winexe")]
		public void clide_new_winexe() {
			File.Exists(Temp("tmp.csproj")).Should(Be.False);

			Clide("new", "--winexe");

			new Project(Temp("tmp.csproj")).OutputType.ShouldEqual("WinExe");
		}

		/// <summary>Does the same thing as -d|--default</summary>
		[Test][Description("clide new MyProject")]
		public void clide_new_Name() {
			Clide("new", "MyProject").Text.ShouldContain("Created new project: MyProject");

			var project = new Project(Temp("MyProject.csproj"));
			project.Configurations.Select(config => config.Name).ToArray().ShouldEqual(new string[]{ null, "Debug", "Release" });
		}

		[Test][Description("clide new MyProject -b|--bare")]
		public void clide_new_Name_bare() {
			Clide("new", "MyProject", "--bare").Text.ShouldContain("Created new project: MyProject");

			var project = new Project(Temp("MyProject.csproj"));
			project.Configurations.Count.ShouldEqual(0);
		}

		[Test][Description("clide new Source/Foo | clide new Source\\Foo\\Bar")]
		public void clide_new_project_in_subdirectory() {
			if (Path.DirectorySeparatorChar == '/') {
				Clide("new", "Source/Foo").Text.ShouldContain("Created new project: Foo");
				var project = new Project(Temp("Source", "Foo.csproj"));
				project.Name.ShouldEqual("Foo");
			} else {
				Clide("new", "Source\\Foo\\Bar").Text.ShouldContain("Created new project: Bar");
				var project = new Project(Temp("Source", "Foo", "Bar.csproj"));
				project.Name.ShouldEqual("Bar");
			}
		}

		[Test][Description("clide new Source/Hi.csproj")]
		public void clide_new_project_subdir_and_csproj_extension() {
			Clide("new", "Source/Hi.csproj").Text.ShouldContain("Created new project: Hi");
			var project = new Project(Temp("Source", "Hi.csproj"));
			project.Name.ShouldEqual("Hi");
		}

		[Test][Description("clide new Foo --source Foo.cs")]
		public void code_new_project_with_source_file() {
			Clide("new", "Foo", "--source", "Foo.cs");
			var project = new Project(Temp("Foo.csproj"));
			project.CompilePaths.Count.ShouldEqual(1);
			project.CompilePaths.First().Include.ShouldEqual("Foo.cs");
		}

		[Test][Description("clide new Foo -s Foo.cs -s Bar.cs")]
		public void code_new_project_with_source_files() {
			Clide("new", "Foo", "--source", "Foo.cs", "-s", "Bar.cs");
			var project = new Project(Temp("Foo.csproj"));
			project.CompilePaths.Count.ShouldEqual(2);
			project.CompilePaths.First().Include.ShouldEqual("Foo.cs");
			project.CompilePaths.Last().Include.ShouldEqual("Bar.cs");
		}

		[Test][Description("clide new Foo --content Foo.txt")]
		public void code_new_project_with_content_file() {
			Clide("new", "Foo", "--content", "Foo.txt");
			var project = new Project(Temp("Foo.csproj"));
			project.Content.Count.ShouldEqual(1);
			project.Content.First().Include.ShouldEqual("Foo.txt");
		}

		[Test][Description("clide new Foo --reference Foo.dll -r System.Xml")]
		public void code_new_project_with_references() {
			Clide("new", "Foo", "--reference", Example("Foo.dll"), "-r", "System.Xml", "-r", Example("FluentXml.Specs.csproj"));
			var project = new Project(Temp("Foo.csproj"));
			project.References.Count.ShouldEqual(2);
			project.References.First().Name.ShouldEqual("Foo");
			project.References.First().FullName.ShouldEqual("Foo, Version=1.2.3.4567, Culture=neutral, PublicKeyToken=null"); // <-- actually reads the DLL
			project.References.Last().Name.ShouldEqual("System.Xml");

			project.ProjectReferences.Count.ShouldEqual(1);
			project.ProjectReferences.First().Name.ShouldEqual("FluentXml.Specs");
		}
	}
}
