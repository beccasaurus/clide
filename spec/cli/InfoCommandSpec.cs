using System;
using System.Linq;
using NUnit.Framework;
using ConsoleRack;
using Clide;

namespace Clide.Specs {

	[TestFixture]
	public class InfoCommandSpec : Spec {

		[Test]
		public void clide_info_with_project() {
			new Project(Temp("Foo.csproj")).Save();

			Clide("info").Text.ShouldContain("Project: " + Temp("Foo.csproj"));
		}

		[Test]
		public void clide_info_with_many_projects() {
			new Project(Temp("Foo.csproj")).Save();
			new Project(Temp("Bar.csproj")).Save();

			Clide("info").Text.ShouldContain("Project: " + Temp("Bar.csproj")); // <--- alphabetically, the first

			Clide("info", "-P", "Foo.csproj").Text.ShouldContain("Project: Foo.csproj"); // override
		}

		[Test]
		public void clide_info_with_solution() {
			new Solution(Temp("Foo.sln")).Save();

			Clide("info").Text.ShouldContain("Solution: " + Temp("Foo.sln"));
		}

		[Test]
		public void clide_info_with_many_solutions() {
			new Solution(Temp("Foo.sln")).Save();
			new Solution(Temp("Bar.sln")).Save();

			Clide("info").Text.ShouldContain("Solution: " + Temp("Bar.sln")); // <--- alphabetically, the first

			Clide("info", "-S", "Foo.sln").Text.ShouldContain("Solution: Foo.sln"); // override
		}
	}
}
