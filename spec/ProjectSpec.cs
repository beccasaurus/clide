using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using FluentXml;

namespace NVS.Specs {

	[TestFixture]
	public class ProjectSpec : Spec {

		[SetUp]
		public void Before() {
			base.BeforeEach();
			File.Copy(Example("FluentXml.Specs.csproj"), Temp("FluentXml.Specs.csproj"));
		}

		[Test]
		public void new_projects_create_their_own_Id_if_Id_not_set() {
			new Project().Id.ToString().Length.ShouldEqual(36); // unique Guid
			new Project().Id.ShouldNotEqual(new Project().Id);
		}

		[Test]
		public void new_projects_use_the_typical_ProjectTypeId_if_not_set() {
			new Project().ProjectTypeId.ShouldEqual(new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"));
		}

		[Test]
		public void RelativePath_is_normalized_to_use_backslashes_instead_of_forward_slashes() {
			var paths = new Dictionary<string,string> {
				{ "foo",          "foo"           },
				{ "foo/bar",      "foo\\bar"      },
				{ "Hello World",  "Hello World"   },
				{ "/hello",       "\\hello"       },
				{ "hi / there",   "hi \\ there"   },
				{ "src\\foo/bar", "src\\foo\\bar" }
			};

			foreach (var path in paths)
				new Project { RelativePath = path.Key }.RelativePath.ShouldEqual(path.Value);
		}

		[Test]
		public void can_read_references() {
			var project    = new Project(Temp("FluentXml.Specs.csproj"));
			var references = project.References;

			references.Count.ShouldEqual(4);

			references[0].Name.ShouldEqual("System");

			references[1].Name.ShouldEqual("System.Core");

			references[2].Name.ShouldEqual("nunit.framework");
			references[2].FullName.ShouldEqual("nunit.framework, Version=2.5.8.10295, Culture=neutral, PublicKeyToken=96d09a1eb7f44a77");
			references[2].SpecificVersion.Should(Be.False);
			references[2].HintPath.ShouldEqual(@"..\lib\nunit.framework.dll");

			references[3].Name.ShouldEqual("NUnit.Should");
			references[3].FullName.ShouldEqual("NUnit.Should, Version=1.0.1.0, Culture=neutral, PublicKeyToken=null");
			references[3].SpecificVersion.Should(Be.False);
			references[3].HintPath.ShouldEqual(@"..\lib\NUnit.Should.dll");
		}

		[Test]
		public void can_add_references_without_HintPath() {
			var project = new Project(Temp("FluentXml.Specs.csproj"));
			project.References.Count.ShouldEqual(4);
			project.References.Select(r => r.Name).ToArray().ShouldEqual(new string[]{ "System", "System.Core", "nunit.framework", "NUnit.Should" });

			project.AddReference(new Reference { FullName = "System.Xml" });

			// our References get updated
			project.References.Count.ShouldEqual(5);
			project.References.Select(r => r.Name).ToArray().ShouldEqual(new string[]{ "System", "System.Core", "nunit.framework", "NUnit.Should", "System.Xml" });

			// if we re-parse, from scratch, we can't see it yet ...
			var readAgain = new Project(Temp("FluentXml.Specs.csproj"));
			readAgain.References.Count.ShouldEqual(4);

			project.Save(); // <--- explicitly need to Save()

			// but, if we Save(), then re-read ...
			readAgain = new Project(Temp("FluentXml.Specs.csproj"));
			readAgain.References.Count.ShouldEqual(5);
			readAgain.References.Select(r => r.Name).ToArray().ShouldEqual(new string[]{ "System", "System.Core", "nunit.framework", "NUnit.Should", "System.Xml" });
		}

		[Test]
		public void can_add_references_with_HintPath() {
			var project = new Project(Temp("FluentXml.Specs.csproj"));
			project.References.Count.ShouldEqual(4);
			project.References.Select(r => r.Name).ToArray().ShouldEqual(new string[]{ "System", "System.Core", "nunit.framework", "NUnit.Should" });

			project.AddReference(new Reference { FullName = "Something", HintPath = "../lib/foo/Something.dll" });

			// our References get updated
			project.References.Count.ShouldEqual(5);
			project.References.Select(r => r.Name).ToArray().ShouldEqual(new string[]{ "System", "System.Core", "nunit.framework", "NUnit.Should", "Something" });

			// if we re-parse, from scratch, we can't see it yet ...
			var readAgain = new Project(Temp("FluentXml.Specs.csproj"));
			readAgain.References.Count.ShouldEqual(4);

			project.Save(); // <--- explicitly need to Save()

			// but, if we Save(), then re-read ...
			readAgain = new Project(Temp("FluentXml.Specs.csproj"));
			readAgain.References.Count.ShouldEqual(5);
			readAgain.References.Select(r => r.Name).ToArray().ShouldEqual(new string[]{ "System", "System.Core", "nunit.framework", "NUnit.Should", "Something" });
			readAgain.References.Last().HintPath.ShouldEqual(@"..\lib\foo\Something.dll");
		}

		[Test]
		public void can_remove_references() {
			var project = new Project(Temp("FluentXml.Specs.csproj"));
			project.References.Count.ShouldEqual(4);
			project.References.Select(r => r.Name).ToArray().ShouldEqual(new string[]{ "System", "System.Core", "nunit.framework", "NUnit.Should" });

			project.RemoveReference("System.Core");
			project.Save();

			project = new Project(Temp("FluentXml.Specs.csproj"));
			project.References.Count.ShouldEqual(3);
			project.References.Select(r => r.Name).ToArray().ShouldEqual(new string[]{ "System", "nunit.framework", "NUnit.Should" });

			project.RemoveReference("nunit.framework");
			project.Save();

			project = new Project(Temp("FluentXml.Specs.csproj"));
			project.References.Count.ShouldEqual(2);
			project.References.Select(r => r.Name).ToArray().ShouldEqual(new string[]{ "System", "NUnit.Should" });
		}

		[Test][Ignore]
		public void can_read_and_modify_files_to_compile() {
		}

		[Test][Ignore]
		public void can_read_attributes_from_root_project_node() {
		}

		[Test]
		public void can_read_project_configurations_from_a_typical_csproj_file() {
			var project = new Project(Example("NET40", "ConsoleApplication1", "ConsoleApplication1", "ConsoleApplication1.csproj"));

			project.Configurations.Count.ShouldEqual(2);

			project.Configurations.First().ToString().ShouldEqual("Debug|x86");
			project.Configurations.First().ShouldHaveProperties(new {
				Name     = "Debug",
				Platform = "x86"
			});

			project.Configurations.Last().ToString().ShouldEqual("Release|x86");
			project.Configurations.Last().ShouldHaveProperties(new {
				Name     = "Release",
				Platform = "x86"
			});

			// project.ConfigurationNames.ShouldEqual(new List<string>{ "Debug", "Release" });
			// project.PlatformNames.ShouldEqual(new List<string>{ "x86" });
		}

		[Test][Ignore]
		public void can_read_project_configurations_from_a_csproj_with_abunchof_configurations() {
			// var project = new Project("MonoDevelop", "NET35", "ConsoleProjectWithConfigurations", "ConsoleProjectWithConfigurations", "ConsoleProjectWithConfigurations.csproj");
		}

		[Test][Ignore]
		public void can_read_properties_for_project_configurations() {
		}

		[Test][Ignore]
		public void can_read_root_properties() {
		}

		[Test][Ignore]
		public void can_read_low_level_global_property_groups() {
			// For NVS, we don't REALLY care much about *EVALUATING* project files, because we don't build/run them.
			//
			// Really, we just care about making it really easy to EDIT these project files.  So we need to make it 
			// easy to modify the "typical" Debug/Release/etc property groups
			//
			// If necessary, we'll implement the ability to read these variables ... but that might not be necessary.  YAGNI!

			// var project = new Project(Example("NET40", "ConsoleApplication1", "ConsoleApplication1", "ConsoleApplication1.csproj"));
			// project.PropertyGroups.ShouldEqual(3);

			// // <PropertyGroup>
			// project.GlobalPropertyGroup.Properties.Count.ShouldEqual(12);
			// project.GlobalPropertyGroup.Properties.Select(p => p.Name).ToArray().ShouldEqual(new string[] { });

			// // <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|x86' ">
			// project.DebugPropertyGroup.Properties.Count.ShouldEqual(8);
			// project.DebugPropertyGroup.Properties.Select(p => p.Name).ToArray().ShouldEqual(new string[] { });;

			// // <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|x86' ">
			// project.ReleasePropertyGroup.Properties.Count.ShouldEqual(8);
			// project.ReleasePropertyGroup.Properties.Select(p => p.Name).ToArray().ShouldEqual(new string[] { });;
		}
	}
}
