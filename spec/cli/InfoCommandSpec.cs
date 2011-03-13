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

			Clide("--debug", "info", "-P", "Foo.csproj").Text.ShouldContain("Project: Foo.csproj"); // override
		}
	}
}
