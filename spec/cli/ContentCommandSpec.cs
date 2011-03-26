using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ConsoleRack;
using Clide;
using IO.Interfaces;

namespace Clide.Specs {

	[TestFixture]
	public class ContentCommandSpec : Spec {

		Project project;

		[SetUp]
		public void Before() {
			base.BeforeEach();
			
			// Make a Project
			Clide("new", "CoolProject");
			project = new Project(Temp("CoolProject.csproj"));
			project.Content.Should(Be.Empty);
		}

		[Test][Description("clide help content")][Ignore]
		public void clide_help_content() {
		}

		[Test][Description("clide content")][Ignore]
		public void clide_content() {
		}

		[Test][Description("clide content add Foo.txt")]
		public void clide_content_add_file() {
			Clide("content", "add", "Foo.txt").Text.ShouldContain("Added Foo.txt to CoolProject");

			project.Reload();
			project.Content.Count.ShouldEqual(1);
			project.Content.First().Include.ShouldEqual("Foo.txt");
		}

		[Test][Description("clide content add Foo.txt Bar.txt")]
		public void clide_content_add_files() {
			var output = Clide("content", "add", "Foo.txt", "Bar.txt").Text;
			output.ShouldContain("Added Foo.txt to CoolProject");
			output.ShouldContain("Added Bar.txt to CoolProject");

			project.Reload();
			project.Content.Count.ShouldEqual(2);
			project.Content.Select(path => path.Include).ToArray().ShouldEqual(new string[]{ "Foo.txt", "Bar.txt" });
		}

		[Test][Description("clide content add Foo.txt Bar.txt (already exists)")]
		public void clide_content_add_files_some_already_exists() {
			Clide("content", "add", "Foo.txt").Text.ShouldContain("Added Foo.txt to CoolProject");

			var output = Clide("content", "add", "Foo.txt", "Bar.txt").Text;
			output.ShouldContain("Added Bar.txt to CoolProject");
			output.ShouldNotContain("Added Foo.txt to CoolProject");
			output.ShouldContain("Foo.txt already added to CoolProject");

			project.Reload();
			project.Content.Count.ShouldEqual(2);
			project.Content.Select(path => path.Include).ToArray().ShouldEqual(new string[]{ "Foo.txt", "Bar.txt" });
		}

		[Test][Description("clide content rm Foo.txt")]
		public void clide_content_rm_file() {
			Clide("content", "add", "Foo.txt", "Bar.txt");
			project.Reload();
			project.Content.Count.ShouldEqual(2);

			Clide("content", "rm", "Bar.txt").Text.ShouldContain("Removed Bar.txt from CoolProject");

			project.Reload();
			project.Content.Count.ShouldEqual(1);
			project.Content.First().Include.ShouldEqual("Foo.txt");
		}

		[Test][Description("clide content rm Foo.txt Bar.txt")]
		public void clide_content_rm_files() {
			Clide("content", "add", "Foo.txt", "Bar.txt");

			var output = Clide("content", "rm", "Foo.txt", "Bar.txt").Text;
			output.ShouldContain("Removed Foo.txt from CoolProject");
			output.ShouldContain("Removed Bar.txt from CoolProject");

			project.Reload();
			project.Content.Count.ShouldEqual(0);
		}
	}
}
