using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ConsoleRack;
using Clide;

namespace Clide.Specs {

	[TestFixture]
	public class ReferencesCommandSpec : Spec {

		Project project;

		[SetUp]
		public void Before() {
			base.BeforeEach();
			Clide("new", "CoolProject");
			project = new Project(Temp("CoolProject.csproj"));
			project.References.Should(Be.Empty);
		}

		[Test][Description("clide help references")][Ignore]
		public void clide_help_references() {
		}

		[Test][Description("clide references")][Ignore]
		public void clide_references() {
		}

		[Test][Description("clide references add Foo.dll")][Ignore]
		public void clide_references_add_dll() {
		}

		[Test][Description("clide references add System.Xml")]
		public void clide_references_add_gac() {
			Clide("references", "add", "System.Xml").Text.ShouldEqual("Added reference System.Xml to CoolProject\n");

			project.Reload();
			project.References.Count.ShouldEqual(1);
			project.References.First().Name.ShouldEqual("System.Xml");
		}

		[Test][Description("clide references add ../src/Foo.csproj")][Ignore]
		public void clide_references_add_project() {
		}

		[Test][Description("clide references rm Foo.dll")][Ignore]
		public void clide_references_rm_dll() {
		}

		[Test][Description("clide references rm System.Xml")][Ignore]
		public void clide_references_rm_gac() {
		}

		[Test][Description("clide references rm ../src/Foo.csproj")][Ignore]
		public void clide_references_rm_project() {
		}
	}
}
