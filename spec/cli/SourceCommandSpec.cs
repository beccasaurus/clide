using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ConsoleRack;
using Clide;
using IO.Interfaces;

namespace Clide.Specs {

	[TestFixture]
	public class SourceCommandSpec : Spec {

		Project project;

		[SetUp]
		public void Before() {
			base.BeforeEach();
			
			// Make a Project
			Clide("new", "CoolProject");
			project = new Project(Temp("CoolProject.csproj"));
			project.CompilePaths.Should(Be.Empty);
		}

		[Test][Description("clide help source")][Ignore]
		public void clide_help_source() {
		}

		[Test][Description("clide source")][Ignore]
		public void clide_source() {
		}

		[Test][Description("clide source add Foo.txt")]
		public void clide_source_add_file() {
			Clide("source", "add", "Foo.txt").Text.ShouldContain("Added Foo.txt to CoolProject");

			project.Reload();
			project.CompilePaths.Count.ShouldEqual(1);
			project.CompilePaths.First().Include.ShouldEqual("Foo.txt");
		}

		[Test][Description("clide source add Foo.txt Bar.txt")]
		public void clide_source_add_files() {
			var output = Clide("source", "add", "Foo.txt", "Bar.txt").Text;
			output.ShouldContain("Added Foo.txt to CoolProject");
			output.ShouldContain("Added Bar.txt to CoolProject");

			project.Reload();
			project.CompilePaths.Count.ShouldEqual(2);
			project.CompilePaths.Select(path => path.Include).ToArray().ShouldEqual(new string[]{ "Foo.txt", "Bar.txt" });
		}

		[Test][Description("clide source add Foo.txt Bar.txt (already exists)")]
		public void clide_source_add_files_some_already_exists() {
			Clide("source", "add", "Foo.txt").Text.ShouldContain("Added Foo.txt to CoolProject");

			var output = Clide("source", "add", "Foo.txt", "Bar.txt").Text;
			output.ShouldContain("Added Bar.txt to CoolProject");
			output.ShouldNotContain("Added Foo.txt to CoolProject");
			output.ShouldContain("Foo.txt already added to CoolProject");

			project.Reload();
			project.CompilePaths.Count.ShouldEqual(2);
			project.CompilePaths.Select(path => path.Include).ToArray().ShouldEqual(new string[]{ "Foo.txt", "Bar.txt" });
		}

		[Test][Description("clide source rm Foo.txt")]
		public void clide_source_rm_file() {
			Clide("source", "add", "Foo.txt", "Bar.txt");
			project.Reload();
			project.CompilePaths.Count.ShouldEqual(2);

			Clide("source", "rm", "Bar.txt").Text.ShouldContain("Removed Bar.txt from CoolProject");

			project.Reload();
			project.CompilePaths.Count.ShouldEqual(1);
			project.CompilePaths.First().Include.ShouldEqual("Foo.txt");
		}

		[Test][Description("clide source rm Foo.txt Bar.txt")]
		public void clide_source_rm_files() {
			Clide("source", "add", "Foo.txt", "Bar.txt");

			var output = Clide("source", "rm", "Foo.txt", "Bar.txt").Text;
			output.ShouldContain("Removed Foo.txt from CoolProject");
			output.ShouldContain("Removed Bar.txt from CoolProject");

			project.Reload();
			project.CompilePaths.Count.ShouldEqual(0);
		}
	}
}
