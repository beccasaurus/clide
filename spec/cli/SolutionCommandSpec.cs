using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ConsoleRack;
using Clide;
using Clide.Extensions;

namespace Clide.Specs {

	[TestFixture]
	public class SolutionCommandSpec : Spec {

		[Test][Description("clide help solution")][Ignore]
		public void clide_help_solution() {
			Clide("help", "solution").Text.ShouldEqual("... ?");
		}

		[Test][Description("clide solution")]
		public void clide_solution() {
			File.Exists(Temp("tmp.sln")).Should(Be.False);

			Clide("solution");

			File.Exists(Temp("tmp.sln")).Should(Be.True);
		}

		// alias
		[Test][Description("clide sln")]
		public void clide_sln() {
			File.Exists(Temp("tmp.sln")).Should(Be.False);

			Clide("sln");

			File.Exists(Temp("tmp.sln")).Should(Be.True);
		}

		[Test][Description("clide sln (with project)")]
		public void clide_sln_with_project() {
			Clide("new", "AwesomeProject");
			Clide("sln");

			var project = new Project(Temp("AwesomeProject.csproj"));
			var sln     = new Solution(Temp("tmp.sln"));

			sln.Projects.Count.ShouldEqual(1);

			sln.ToText().ShouldContain(string.Format(@"""AwesomeProject"", ""AwesomeProject.csproj"", ""{0}""", 
					project.Id.ToString().WithCurlies().ToUpper()));
		}

		[Test][Description("clide sln -n Foo (with project)")]
		public void clide_sln_name_foo_with_project() {
			Clide("new", "AwesomeProject");
			Clide("sln", "--name", "WickedAwesome");

			File.Exists(Temp("tmp.sln")).Should(Be.False);
			File.Exists(Temp("WickedAwesome.sln")).Should(Be.True);

			var text = Solution.FromPath(Temp("WickedAwesome.sln")).ToText();
			text.ShouldContain("Microsoft Visual Studio Solution File");
			text.ShouldContain("AwesomeProject");
		}

		[Test][Description("clide sln --path foo/Bar.sln (with project)")][Ignore]
		public void clide_sln_with_path() {
		}

		[Test][Description("clide sln --blank (with project)")][Ignore]
		public void clide_sln_blank() {
		}

		[Test][Description("clide sln add Foo.csproj")]
		public void clide_sln_add_project() {
			Clide("sln");
			Clide("new", "AwesomeProject");

			new Solution(Temp("tmp.sln")).Projects.Should(Be.Empty);

			Clide("sln", "add", "AwesomeProject.csproj").Text.ShouldEqual("Added AwesomeProject to Solution\n");

			new Solution(Temp("tmp.sln")).Projects.Count.ShouldEqual(1);
			new Solution(Temp("tmp.sln")).Projects.First().Name.ShouldEqual("AwesomeProject");
		}

		[Test][Description("clide sln Dir/SubDir/Foo.csproj")]
		public void clide_sln_add_project_in_subdirectory() {
			Clide("sln");
			Clide("new", "Dir/SubDir/Foo");

			new Solution(Temp("tmp.sln")).Projects.Should(Be.Empty);

			Clide("sln", "add", "Dir/SubDir/Foo.csproj").Text.ShouldEqual("Added Foo to Solution\n");

			new Solution(Temp("tmp.sln")).Projects.Count.ShouldEqual(1);
			new Solution(Temp("tmp.sln")).Projects.First().Name.ShouldEqual("Foo");
			new Solution(Temp("tmp.sln")).Projects.First().RelativePath.ShouldEqual(@"Dir\SubDir\Foo.csproj");
		}

		[Test][Description("clide sln rm Foo.csproj")]
		public void clide_sln_remove_project() {
			Clide("sln");
			Clide("new", "Dir/SubDir/Foo");
			Clide("new", "Bar");
			Clide("sln", "add", "Dir/SubDir/Foo.csproj").Text.ShouldEqual("Added Foo to Solution\n");
			Clide("sln", "add", "Bar.csproj"           ).Text.ShouldEqual("Added Bar to Solution\n");

			new Solution(Temp("tmp.sln")).Projects.Select(p => p.Name).ToArray().ShouldEqual(new string[]{ "Foo", "Bar" });

			Clide("sln", "rm", "Bar.csproj").Text.ShouldEqual("Removed Bar from Solution\n");

			new Solution(Temp("tmp.sln")).Projects.Select(p => p.Name).ToArray().ShouldEqual(new string[]{ "Foo" });

			Clide("sln", "rm", "Dir/SubDir/Foo.csproj").Text.ShouldEqual("Removed Foo from Solution\n");

			new Solution(Temp("tmp.sln")).Projects.Should(Be.Empty);
		}

		[Test][Description("clide sln rm ProjectName")]
		public void clide_sln_remove_project_by_name() {
			Clide("sln");
			Clide("new", "Dir/SubDir/Foo");
			Clide("new", "Bar");
			Clide("sln", "add", "Dir/SubDir/Foo.csproj").Text.ShouldEqual("Added Foo to Solution\n");
			Clide("sln", "add", "Bar.csproj"           ).Text.ShouldEqual("Added Bar to Solution\n");

			new Solution(Temp("tmp.sln")).Projects.Select(p => p.Name).ToArray().ShouldEqual(new string[]{ "Foo", "Bar" });

			Clide("sln", "rm", "Foo").Text.ShouldEqual("Removed Foo from Solution\n");

			new Solution(Temp("tmp.sln")).Projects.Select(p => p.Name).ToArray().ShouldEqual(new string[]{ "Bar" });

			Clide("sln", "rm", "Bar").Text.ShouldEqual("Removed Bar from Solution\n");

			new Solution(Temp("tmp.sln")).Projects.Should(Be.Empty);
		}
	}
}
