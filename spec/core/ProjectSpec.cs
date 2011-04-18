using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using FluentXml;
using Clide.Extensions;

namespace Clide.Specs {

	[TestFixture]
	public class ProjectSpec : Spec {

		// NET40/Mvc3Application1/Mvc3Application1/Mvc3Application1.csproj
		// FluentXml.Specs.csproj

		[SetUp]
		public void Before() {
			base.BeforeEach();
			File.Copy(Example("FluentXml.Specs.csproj"), Temp("FluentXml.Specs.csproj"));
			File.Copy(Example("NET40", "Mvc3Application1", "Mvc3Application1", "Mvc3Application1.csproj"), Temp("Mvc3Application1.csproj"));
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

		/*
		  <ItemGroup>
			<ProjectReference Include="..\src\FluentXml.csproj">
			  <Project>{5D8673F4-0239-4D86-9093-B46A2075E722}</Project>
			  <Name>FluentXml</Name>
			</ProjectReference>
		  </ItemGroup>
		*/
		[Test]
		public void can_read_project_references() {
			var project = new Project(Temp("FluentXml.Specs.csproj"));

			project.ProjectReferences.Count.ShouldEqual(1);

			project.ProjectReferences.First().ShouldHaveProperties(new {
				Name        = "FluentXml",
				ProjectId   = new Guid("5D8673F4-0239-4D86-9093-B46A2075E722"),
				ProjectFile = @"..\src\FluentXml.csproj",
			});
		}

		[Test][Ignore]
		public void can_read_compile_paths() {
			var project    = new Project(Temp("NET40/Mvc3Application1/Mvc3Application1/Mvc3Application1.csproj"));
			// var project = new Project(Temp("FluentXml.Specs.csproj"));

			// project.CompilePaths.Count.ShouldEqual(3);

			// project.CompilePaths.Select(path => path.Include).ToArray().ShouldEqual(new string[]{
			// 		
			// });
		}

		[Test][Ignore]
		public void can_read_content() {
		}

		[Test][Ignore]
		public void can_read_folders() {
		}

		[Test]
		public void can_add_references_without_HintPath() {
			var project = new Project(Temp("FluentXml.Specs.csproj"));
			project.References.Count.ShouldEqual(4);
			project.References.Select(r => r.Name).ToArray().ShouldEqual(new string[]{ "System", "System.Core", "nunit.framework", "NUnit.Should" });

			project.References.AddGacReference("System.Xml");

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

			project.References.AddDll("Something", "../lib/foo/Something.dll");

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

			project.References.Remove("System.Core");
			project.Save();

			project = new Project(Temp("FluentXml.Specs.csproj"));
			project.References.Count.ShouldEqual(3);
			project.References.Select(r => r.Name).ToArray().ShouldEqual(new string[]{ "System", "nunit.framework", "NUnit.Should" });

			project.References.Remove("nunit.framework");
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

			project.Configurations.Count.ShouldEqual(3);

			project.Configurations[0].ToString().ShouldEqual("Global");

			project.Configurations[1].ToString().ShouldEqual("Debug|x86");
			project.Configurations[1].ShouldHaveProperties(new {
				Name     = "Debug",
				Platform = "x86"
			});

			project.Configurations[2].ToString().ShouldEqual("Release|x86");
			project.Configurations[2].ShouldHaveProperties(new {
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

		[Test]
		public void can_read_properties_for_project_configurations() {
			var project = new Project(Temp("FluentXml.Specs.csproj"));

			project.Config["Debug"]["OutputPath"].ShouldEqual(@"..\bin\Debug");
			project.Config["Debug"]["DefineConstants"].ShouldEqual("DEBUG");
			project.Config["Debug"].Properties.Select(p => p.Name).ToArray().ShouldEqual(new string[]{ 
				"DebugSymbols", "DebugType", "Optimize", "OutputPath", "DefineConstants", "ErrorReport", "WarningLevel", "ConsolePause"
			});

			project.Config["Release"]["OutputPath"].ShouldEqual(@"..\bin\Release");
			project.Config["Release"]["DefineConstants"].Should(Be.Null);
			project.Config["Release"].Properties.Select(p => p.Name).ToArray().ShouldEqual(new string[]{
				"DebugType", "Optimize", "OutputPath", "ErrorReport", "WarningLevel", "ConsolePause"
			});
		}

		[Test]
		public void can_modify_existing_configuration_property() {
			new Project(Temp("FluentXml.Specs.csproj")).Config["Debug"]["OutputPath"].ShouldEqual(@"..\bin\Debug");

			var project = new Project(Temp("FluentXml.Specs.csproj"));
			project.Config["Debug"]["OutputPath"] = "Different Path!";

			// has not changed
			new Project(Temp("FluentXml.Specs.csproj")).Config["Debug"]["OutputPath"].ShouldEqual(@"..\bin\Debug");

			// but, if we save ...
			project.Save();
			new Project(Temp("FluentXml.Specs.csproj")).Config["Debug"]["OutputPath"].ShouldEqual("Different Path!");
		}

		[Test]
		public void can_create_new_configuration_property() {
			new Project(Temp("FluentXml.Specs.csproj")).Config["Debug"]["FooBar"].Should(Be.Null);

			var project = new Project(Temp("FluentXml.Specs.csproj"));
			project.Config["Debug"]["FooBar"] = "Value of Foo Bar";

			// has not changed
			new Project(Temp("FluentXml.Specs.csproj")).Config["Debug"]["FooBar"].Should(Be.Null);

			// but, if we save ...
			project.Save();
			new Project(Temp("FluentXml.Specs.csproj")).Config["Debug"]["FooBar"].ShouldEqual("Value of Foo Bar");
		}

		[Test]
		public void can_remove_existing_configuration_property() {
			new Project(Temp("FluentXml.Specs.csproj")).Config["Debug"]["OutputPath"].ShouldEqual(@"..\bin\Debug");

			var project = new Project(Temp("FluentXml.Specs.csproj"));
			project.Config["Debug"].GetProperty("OutputPath").Remove();

			// has not changed
			new Project(Temp("FluentXml.Specs.csproj")).Config["Debug"]["OutputPath"].ShouldEqual(@"..\bin\Debug");

			// but, if we save ...
			project.Save();
			new Project(Temp("FluentXml.Specs.csproj")).Config["Debug"]["OutputPath"].Should(Be.Null);
		}

		[Test]
		public void can_read_global_properties() {
			var project = new Project(Temp("FluentXml.Specs.csproj"));
			project.GlobalProperties.Count.ShouldEqual(9);
			project.GlobalProperties.Select(p => p.Name).ToArray().ShouldEqual(new string[]{ 
				"Configuration", "Platform", "ProductVersion", "SchemaVersion", "ProjectGuid", 
				"OutputType", "RootNamespace", "AssemblyName", "TargetFrameworkVersion"
			});
			project.GlobalProperties.Last().Text.ShouldEqual("v4.0");
			project.GlobalProperties.Last().Text = "CHANGED";

			new Project(Temp("FluentXml.Specs.csproj")).GlobalProperties.Last().Text.ShouldEqual("v4.0");

			// but if we Save() ...
			project.Save();

			new Project(Temp("FluentXml.Specs.csproj")).GlobalProperties.Last().Text.ShouldEqual("CHANGED");
		}

		[Test]
		public void can_create_a_blank_project() {
			new Project().ToXml().ShouldEqual(@"
				<?xml version='1.0' encoding='utf-8'?>
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				</Project>".FixXml());
		}

		[Test]
		public void can_modify_base_project_properties() {
			var project = new Project();

			project.DefaultTargets.Should(Be.Null);
			project.DefaultTargets = "Build";
			project.DefaultTargets.ShouldEqual("Build");

			project.ToXml().ShouldEqual(@"
				<?xml version='1.0' encoding='utf-8'?>
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' DefaultTargets='Build'>
				</Project>".FixXml());

			project.ToolsVersion.Should(Be.Null);
			project.ToolsVersion = "4.0";
			project.ToolsVersion.ShouldEqual("4.0");

			project.ToXml().ShouldEqual(@"
				<?xml version='1.0' encoding='utf-8'?>
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003' DefaultTargets='Build' ToolsVersion='4.0'>
				</Project>".FixXml());
		}

		[Test]
		public void can_add_global_configuration() {
			var project = new Project();

			project.Configurations.Should(Be.Empty);
			project.Configurations.AddGlobalConfiguration(); // <--- add configuration
			project.Configurations.Count.ShouldEqual(1);
			project.Configurations.First().Name.Should(Be.Null);
			project.Configurations.First().IsGlobal.Should(Be.True);
			project.Configurations.First().Properties.Should(Be.Empty);
			project.ToXml().ShouldEqual(@"
				<?xml version='1.0' encoding='utf-8'?>
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				  <PropertyGroup />
				</Project>".FixXml());

			project.Global.Properties.Should(Be.Empty);
			project.Global["Foo"] = "Bar!";					// <--- add property
			project.Global.Properties.Count.ShouldEqual(1);
			project.ToXml().ShouldEqual(@"
				<?xml version='1.0' encoding='utf-8'?>
				<Project xmlns='http://schemas.microsoft.com/developer/msbuild/2003'>
				  <PropertyGroup>
				    <Foo>Bar!</Foo>
				  </PropertyGroup>
				</Project>".FixXml());
		}

		[Test]
		public void can_add_configurations() {
			var project = new Project();

			project.Configurations.Should(Be.Empty);

			project.Configurations.Add("Foo");
			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Foo|AnyCPU' "" />
				</Project>".TrimLeadingTabs(4).TrimStartNewline());

			project.Configurations.Add("Bar");
			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Foo|AnyCPU' "" />
				  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Bar|AnyCPU' "" />
				</Project>".TrimLeadingTabs(4).TrimStartNewline());

			project.Config["Foo"]["Hello"] = "there";
			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Foo|AnyCPU' "">
				    <Hello>there</Hello>
				  </PropertyGroup>
				  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Bar|AnyCPU' "" />
				</Project>".TrimLeadingTabs(4).TrimStartNewline());

			project.Configurations.AddGlobalConfiguration();
			project.Global["Hello"] = "Default Hello";
			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Foo|AnyCPU' "">
				    <Hello>there</Hello>
				  </PropertyGroup>
				  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Bar|AnyCPU' "" />
				  <PropertyGroup>
				    <Hello>Default Hello</Hello>
				  </PropertyGroup>
				</Project>".TrimLeadingTabs(4).TrimStartNewline());
		}

		[Test]
		public void can_add_default_global_configuration_properties() {
			var project = new Project();
			var global  = project.Configurations.AddGlobalConfiguration();
			global.Properties.Count.ShouldEqual(0);

			var id = Guid.NewGuid();
			global.AddDefaultGlobalProperties(id, "4", "Library", "FooNamespace", "MyAssembly");

			global.Properties.Select(prop => string.Format("{0} {1} {2}", prop.Name, prop.Text, prop.Condition)).ToArray().ShouldEqual(new string[]{
				"Configuration Debug  '$(Configuration)' == '' ",
				"Platform AnyCPU  '$(Platform)' == '' ",
				"ProductVersion 8.0.30703 ",
				"SchemaVersion 2.0 ",
				"ProjectGuid " + id.ToString().ToUpper().WithCurlies() + " ",
				"OutputType Library ",
				"RootNamespace FooNamespace ",
				"AssemblyName MyAssembly ",
				"TargetFrameworkVersion v4.0 ",
				"FileAlignment 512 "
			});

			// This should probably be in it's own [Test] ... snuck it in here:
			project.SetDefaultProjectAttributes();
			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" DefaultTargets=""Build"" ToolsVersion=""4.0"">
				  <PropertyGroup>
				    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
				    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
				    <ProductVersion>8.0.30703</ProductVersion>
				    <SchemaVersion>2.0</SchemaVersion>
				    <ProjectGuid>{PROJECT_ID}</ProjectGuid>
				    <OutputType>Library</OutputType>
				    <RootNamespace>FooNamespace</RootNamespace>
				    <AssemblyName>MyAssembly</AssemblyName>
				    <TargetFrameworkVersion>v4.0</TargetFrameworkVersion>
				    <FileAlignment>512</FileAlignment>
				  </PropertyGroup>
				</Project>".TrimLeadingTabs(4).TrimStartNewline().Replace("PROJECT_ID", id.ToString().ToUpper()));
		}

		[Test]
		public void can_add_default_debug_configuration_properties() {
			var project = new Project();
			var debug   = project.Configurations.Add("Debug");
			debug.Properties.Count.ShouldEqual(0);

			debug.AddDefaultDebugProperties();

			debug.Properties.Select(prop => string.Format("{0} {1}", prop.Name, prop.Text)).ToArray().ShouldEqual(new string[]{
				"DebugSymbols true",
				"DebugType full",
				"Optimize false",
				"OutputPath bin\\Debug\\",
				"DefineConstants DEBUG;TRACE'",
				"ErrorReport prompt",
				"WarningLevel 4"
			});
		}

		[Test]
		public void can_add_default_release_configuration_properties() {
			var project = new Project();
			var release   = project.Configurations.Add("Release");
			release.Properties.Count.ShouldEqual(0);

			release.AddDefaultReleaseProperties();

			release.Properties.Select(prop => string.Format("{0} {1}", prop.Name, prop.Text)).ToArray().ShouldEqual(new string[]{
				"DebugType pdbonly",
				"Optimize true",
				"OutputPath bin\\Release\\",
				"DefineConstants TRACE",
				"ErrorReport prompt",
				"WarningLevel 4"
			});
		}

		[Test]
		public void can_add_references_to_blank_project() {
			var project = new Project();

			project.References.AddGacReference("System");
			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <ItemGroup>
				    <Reference Include=""System"" />
				  </ItemGroup>
				</Project>".TrimLeadingTabs(4).TrimStartNewline());

			project.References.AddDll("Something", "../lib/foo/Something.dll");
			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <ItemGroup>
				    <Reference Include=""System"" />
				    <Reference Include=""Something"">
				      <HintPath>..\lib\foo\Something.dll</HintPath>
				      <SpecificVersion>False</SpecificVersion>
				    </Reference>
				  </ItemGroup>
				</Project>".TrimLeadingTabs(4).TrimStartNewline());
		}

		[Test]
		public void can_add_project_references() {
			var project = new Project();

			project.ProjectReferences.Add("CoolProject", @"..\src\CoolProject.csproj", new Guid("5D8673F4-0239-4D86-9093-B46A2075E722"));

			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <ItemGroup>
				    <ProjectReference Include=""..\src\CoolProject.csproj"">
				      <Project>{5D8673F4-0239-4D86-9093-B46A2075E722}</Project>
				      <Name>CoolProject</Name>
				    </ProjectReference>
				  </ItemGroup>
				</Project>".TrimLeadingTabs(4).TrimStartNewline());

			// throw some GAC references in here too, just for shits and giggles (and because it helps us recreate a bug!)
			project.References.AddGacReference("System.Foo");
			project.References.AddGacReference("System.Bar");
			project.ProjectReferences.Add("Another.Project", @"..\AnotherProject.csproj", new Guid("12345678-0239-4D86-9093-B46A2075E722"));

			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <ItemGroup>
				    <ProjectReference Include=""..\src\CoolProject.csproj"">
				      <Project>{5D8673F4-0239-4D86-9093-B46A2075E722}</Project>
				      <Name>CoolProject</Name>
				    </ProjectReference>
				    <ProjectReference Include=""..\AnotherProject.csproj"">
				      <Project>{12345678-0239-4D86-9093-B46A2075E722}</Project>
				      <Name>Another.Project</Name>
				    </ProjectReference>
				  </ItemGroup>
				  <ItemGroup>
				    <Reference Include=""System.Foo"" />
				    <Reference Include=""System.Bar"" />
				  </ItemGroup>
				</Project>".TrimLeadingTabs(4).TrimStartNewline());
		}

		[Test]
		public void can_read_include_paths_to_compile() {
			var project = new Project(Temp("Mvc3Application1.csproj"));

			project.CompilePaths.Count.ShouldEqual(2);

			project.CompilePaths.First().Include.ShouldEqual("Global.asax.cs");
			project.CompilePaths.First().DependentUpon.ShouldEqual("Global.asax");

			project.CompilePaths.Last().Include.ShouldEqual("Properties\\AssemblyInfo.cs");
			project.CompilePaths.Last().DependentUpon.Should(Be.Null);
		}

		[Test]
		public void can_read_content_paths() {
			var project = new Project(Temp("Mvc3Application1.csproj"));

			project.Content.Count.ShouldEqual(24);

			project.Content.Select(content => string.Format("{0}{1}", content.Include, content.DependentUpon)).ToArray().ShouldEqual(new string[]{
				"Global.asax", "Content\\Site.css", "Web.config", "Web.Debug.configWeb.config", "Web.Release.configWeb.config", "Scripts\\jquery-1.4.1.js",
				"Scripts\\jquery-1.4.1.min.js", "Scripts\\jquery-1.4.1-vsdoc.js", "Scripts\\jquery.unobtrusive-ajax.js", "Scripts\\jquery.unobtrusive-ajax.min.js",
				"Scripts\\jquery.validate.js", "Scripts\\jquery.validate.min.js", "Scripts\\jquery.validate.unobtrusive.js", "Scripts\\jquery.validate.unobtrusive.min.js", 
				"Scripts\\jquery.validate-vsdoc.js", "Scripts\\MicrosoftAjax.js", "Scripts\\MicrosoftAjax.debug.js", "Scripts\\MicrosoftMvcAjax.js",
				"Scripts\\MicrosoftMvcAjax.debug.js", "Scripts\\MicrosoftMvcValidation.js", "Scripts\\MicrosoftMvcValidation.debug.js",
				"Views\\Web.config", "Views\\_ViewStart.cshtml", "Views\\Shared\\_Layout.cshtml"
			});
		}

		[Test]
		public void can_read_imported_msbuild_targets() {
			var project = new Project(Temp("Mvc3Application1.csproj"));

			project.TargetImports.Count.ShouldEqual(2);

			project.TargetImports.First().Project.ShouldEqual(@"$(MSBuildBinPath)\Microsoft.CSharp.targets");
			project.TargetImports.Last().Project.ShouldEqual(@"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v10.0\WebApplications\Microsoft.WebApplication.targets");
		}

		[Test]
		public void can_add_include_paths_to_compile() {
			var project = new Project();

			project.CompilePaths.Add(include: @"foo\bar.cs");
			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <ItemGroup>
				    <Compile Include=""foo\bar.cs"" />
				  </ItemGroup>
				</Project>".TrimLeadingTabs(4).TrimStartNewline());

			// It should normalize Unix -> Windows paths
			project.CompilePaths.Add(include: @"foo/hi.cs");
			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <ItemGroup>
				    <Compile Include=""foo\bar.cs"" />
				    <Compile Include=""foo\hi.cs"" />
				  </ItemGroup>
				</Project>".TrimLeadingTabs(4).TrimStartNewline());
		}

		[Test]
		public void can_add_include_paths_to_compile_with_link() {
			var project = new Project();

			project.CompilePaths.Add(include: @"foo\bar.cs", link: "external\\foo");
			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <ItemGroup>
				    <Compile Include=""foo\bar.cs"">
				      <Link>external\foo</Link>
				    </Compile>
				  </ItemGroup>
				</Project>".TrimLeadingTabs(4).TrimStartNewline());
		}

		/*
		   <ItemGroup>
			   <Compile Include="*.cs" Exclude="a.cs;b.cs"/>
		   </ItemGroup>
		 */
		[Test][Ignore]
		public void can_add_and_edit_compile_paths_to_exclude() {
		}

		[Test]
		public void can_add_include_paths_to_include_as_content() {
			var project = new Project();

			project.Content.Add(include: @"foo\bar.txt");
			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <ItemGroup>
				    <Content Include=""foo\bar.txt"" />
				  </ItemGroup>
				</Project>".TrimLeadingTabs(4).TrimStartNewline());

			// It should normalize Unix -> Windows paths
			project.Content.Add(include: @"foo/hi.txt");
			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <ItemGroup>
				    <Content Include=""foo\bar.txt"" />
				    <Content Include=""foo\hi.txt"" />
				  </ItemGroup>
				</Project>".TrimLeadingTabs(4).TrimStartNewline());
		}

		[Test]
		public void can_add_msbuild_target_imports() {
			var project = new Project();

			project.Imports.Count.ShouldEqual(0);

			project.Imports.Add("foo.targets");

			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <Import Project=""foo.targets"" />
				</Project>".TrimLeadingTabs(4).TrimStartNewline());

			project.AddDefaultCSharpImport();

			project.ToXml().ShouldEqual(@"
				<?xml version=""1.0"" encoding=""utf-8""?>
				<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
				  <Import Project=""foo.targets"" />
				  <Import Project=""$(MSBuildBinPath)\Microsoft.CSharp.targets"" />
				</Project>".TrimLeadingTabs(4).TrimStartNewline());
		}

		[Test][Ignore]
		public void can_make_a_standard_default_project_with_one_method_call() {
		}

		[Test]
		public void can_get_the_string_for_TargetFrameworkVersion_given_simple_version_strings() {
			foreach(var item in new Dictionary<string,string> {
				{ "2",    "v2.0" },
				{ "20",   "v2.0" },
				{ "v20",  "v2.0" },
				{ "2.0",  "v2.0" },
				{ "v2.0", "v2.0" },
				{ "3",    "v3.0" },
				{ "35",   "v3.5" },
				{ "v35",  "v3.5" },
				{ "3.5",  "v3.5" },
				{ "v3.5", "v3.5" }
			})
				Configuration.TargetFrameworkVersionFromString(item.Key).ShouldEqual(item.Value);
		}
	}
}
