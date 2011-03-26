using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ConsoleRack;
using Clide;
using IO.Interfaces;

namespace Clide.Specs {

	[TestFixture]
	public class ReferencesCommandSpec : Spec {

		string Slash = Path.DirectorySeparatorChar.ToString();

		Project project;

		[SetUp]
		public void Before() {
			base.BeforeEach();
			
			// Make a Project
			Clide("new", "CoolProject");
			project = new Project(Temp("CoolProject.csproj"));
			project.References.Should(Be.Empty);

			// Make some "dlls" to reference
			Temp("lib").AsDir().Create();
			Temp("FakeAssembly.dll").AsFile().Touch(); // Just a file - will not be able to load as an Assembly

			// A real .NET assembly
			// Foo, Version=1.2.3.4567, Culture=neutral, PublicKeyToken=null
			Example("Foo.dll").AsFile().Copy(Temp("lib", "Foo.dll"));
		}

		[Test][Description("clide help references")][Ignore]
		public void clide_help_references() {
		}

		[Test][Description("clide references")][Ignore]
		public void clide_references() {
		}

		[Test][Description("clide references add Foo.dll")]
		public void clide_references_add_assembly() {
			Clide("references", "add", "lib" + Slash + "Foo.dll").Text.ShouldContain("Added reference Foo to CoolProject");

			project.Reload();
			project.References.Count.ShouldEqual(1);
			project.References.First().FullName.ShouldEqual("Foo, Version=1.2.3.4567, Culture=neutral, PublicKeyToken=null");
			project.References.First().Name.ShouldEqual("Foo");
			project.References.First().HintPath.ShouldEqual(@"lib\Foo.dll");
		}

		[Test][Description("clide references add FakeAssembly.dll")]
		public void clide_references_add_assembly_we_cant_read() {
			var output = Clide("references", "add", "FakeAssembly.dll").Text;
			output.ShouldContain("Added reference FakeAssembly.dll to CoolProject\n");
			output.ShouldContain("Couldn't load assembly");

			project.Reload();
			project.References.Count.ShouldEqual(1);
			project.References.First().Name.ShouldEqual("FakeAssembly.dll");
			project.References.First().HintPath.ShouldEqual("FakeAssembly.dll");
		}

		[Test][Description("clide references add System.Xml")]
		public void clide_references_add_gac() {
			Clide("references", "add", "System.Xml").Text.ShouldEqual("Added reference System.Xml to CoolProject\n");

			project.Reload();
			project.References.Count.ShouldEqual(1);
			project.References.First().FullName.ShouldEqual("System.Xml");
			project.References.First().HintPath.Should(Be.Null);
		}

		[Test][Description("clide references add ../src/Foo.csproj")]
		public void clide_references_add_project() {
			Clide("references", "add", Example("FluentXml.Specs.csproj")).Text.ShouldContain("Added reference FluentXml.Specs to CoolProject");

			project.Reload();
			project.References.Count.ShouldEqual(0);
			project.ProjectReferences.Count.ShouldEqual(1);
			project.ProjectReferences.First().ProjectId.ToString().ShouldEqual("73123bfc-2a8a-4160-80fc-597a2b460c66");
			project.ProjectReferences.First().ProjectFile.ShouldEqual(@"..\examples\FluentXml.Specs.csproj"); // <--- relative!
			project.ProjectReferences.First().Name.ShouldEqual("FluentXml.Specs");
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
