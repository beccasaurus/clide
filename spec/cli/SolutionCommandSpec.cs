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

			Console.WriteLine(sln.ToText());
			sln.ToText().ShouldContain(string.Format(@"""AwesomeProject"", ""{0}"", ""{1}""", Path.GetFullPath(project.Path), project.Id.ToString().WithCurlies().ToUpper()));
		}

		[Test][Description("clide sln add Foo.csproj")][Ignore]
		public void clide_sln_add_project() {
		}

		[Test][Description("clide sln rm Foo.proj")][Ignore]
		public void clide_sln_remove_project() {
		}
	}
}