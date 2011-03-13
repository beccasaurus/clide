using System;
using System.Linq;
using NUnit.Framework;
using ConsoleRack;
using Clide;

namespace Clide.Specs {

	[TestFixture]
	public class NewCommandSpec : Spec {

		[Test][Description("clide help new")]
		public void clide_help_new() {
			Clide("help", "new").Text.ShouldEqual("Helpful information about the 'new' command\n");
		}

		[Test][Description("clide new")]
		public void clide_new() {
			Clide("new").Text.ShouldEqual("Helpful information about the 'new' command\n");
		}

		/// <summary>Does the same thing as -d|--default</summary>
		[Test][Description("clide new MyProject")]
		public void clide_new_Name() {
			Clide("new", "MyProject").Text.ShouldEqual("Created new project: MyProject\n");

			var project = new Project(Temp("MyProject.csproj"));
			project.Configurations.Select(config => config.Name).ToArray().ShouldEqual(new string[]{ null, "Debug", "Release" });
		}

		[Test][Description("clide new MyProject -b|--bare")]
		public void clide_new_Name_bare() {
			Clide("new", "MyProject", "--bare").Text.ShouldEqual("Created new project: MyProject\n");

			var project = new Project(Temp("MyProject.csproj"));
			project.Configurations.Count.ShouldEqual(0);
		}
	}
}