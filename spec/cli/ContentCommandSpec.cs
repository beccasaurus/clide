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

		[Test][Description("clide help content")]
		public void clide_help_content() {
            Clide("help", "content").Text.ShouldContain("Usage: clide content add|rm file1.html file2.txt");
		}

		[Test][Description("clide content")]
		public void clide_content() {
            Clide("content").Text.ShouldContain("This project has no content");

            Global.WorkingDirectory = Example("NET40", "Mvc3Application1", "Mvc3Application1");
            var output = Clide("content").Text;
            
            output.ShouldContain(@"Global.asax");
            output.ShouldContain(@"Web.config");
            output.ShouldContain(@"Content\Site.css");
            output.ShouldContain(@"Scripts\jquery.validate.unobtrusive.min.js");
            output.ShouldContain(@"Views\Shared\_Layout.cshtml");
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
