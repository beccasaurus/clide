using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Clide.Specs {

	[TestFixture]
	public class SolutionSpec : Spec {

		[SetUp]
		public void Before() {
			base.BeforeEach();
		}

		[Test]
		public void can_read_the_solution_format_version() {
			var sln1 = new Solution(Example("NET20", "WebApplication1", "WebApplication1.sln"));
			sln1.FormatVersion.ShouldEqual("11.00");
			sln1.VisualStudioVersion.ShouldEqual("2010");

			var sln2 = new Solution(Example("MonoDevelop", "NET35", "CsharpConsoleProject", "CsharpConsoleProject.sln"));
			sln2.FormatVersion.ShouldEqual("10.00");
			sln2.VisualStudioVersion.ShouldEqual("2008");
		}

		[Test]
		public void can_read_projects() {
			var sln1 = new Solution(Example("NET20", "WebApplication1", "WebApplication1.sln"));
			sln1.Projects.Count.ShouldEqual(1);
			sln1.Projects.First().ShouldHaveProperties(new {
				Name          = "WebApplication1",
				RelativePath  = @"WebApplication1\WebApplication1.csproj",
				Id            = Guid.Parse("11FC4B99-DB31-4D0C-A472-4F794098F900"),
				ProjectTypeId = Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC")
			});

			var sln2 = new Solution(Example("MonoDevelop", "NET35", "CsharpConsoleProject", "CsharpConsoleProject.sln"));
			sln2.Projects.Count.ShouldEqual(1);
			sln2.Projects.First().ShouldHaveProperties(new {
				Name          = "CsharpConsoleProject",
				RelativePath  = @"CsharpConsoleProject\CsharpConsoleProject.csproj",
				Id            = Guid.Parse("DE8DC42E-C399-4367-8E34-735B7F9AA54C"),
				ProjectTypeId = Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC")
			});
		}

		[Test]
		public void can_read_sections() {
			var sln1 = new Solution(Example("NET20", "WebApplication1", "WebApplication1.sln"));
			sln1.Sections.Count.ShouldEqual(3);

			sln1.Sections[0].Name.ShouldEqual("SolutionConfigurationPlatforms");
			sln1.Sections[0].PreSolution.Should(Be.True);
			sln1.Sections[0].Text.ShouldEqual("\t\tDebug|Any CPU = Debug|Any CPU\n\t\tRelease|Any CPU = Release|Any CPU");

			sln1.Sections[1].Name.ShouldEqual("ProjectConfigurationPlatforms");
			sln1.Sections[1].PreSolution.Should(Be.False);
			sln1.Sections[1].Text.ShouldEqual(
					"\t\t{11FC4B99-DB31-4D0C-A472-4F794098F900}.Debug|Any CPU.ActiveCfg = Debug|Any CPU\n" + 
					"\t\t{11FC4B99-DB31-4D0C-A472-4F794098F900}.Debug|Any CPU.Build.0 = Debug|Any CPU\n" + 
					"\t\t{11FC4B99-DB31-4D0C-A472-4F794098F900}.Release|Any CPU.ActiveCfg = Release|Any CPU\n" + 
					"\t\t{11FC4B99-DB31-4D0C-A472-4F794098F900}.Release|Any CPU.Build.0 = Release|Any CPU");

			sln1.Sections[2].Name.ShouldEqual("SolutionProperties");
			sln1.Sections[2].PreSolution.Should(Be.True);
			sln1.Sections[2].Text.ShouldEqual("\t\tHideSolutionNode = FALSE");

			// ---------
			
			var sln2 = new Solution(Example("MonoDevelop", "NET35", "CsharpConsoleProject", "CsharpConsoleProject.sln"));
			sln2.Sections.Count.ShouldEqual(3);

			sln2.Sections[0].Name.ShouldEqual("SolutionConfigurationPlatforms");
			sln2.Sections[0].PreSolution.Should(Be.True);
			sln2.Sections[0].Text.ShouldEqual("\t\tDebug|x86 = Debug|x86\n\t\tRelease|x86 = Release|x86");

			sln2.Sections[1].Name.ShouldEqual("ProjectConfigurationPlatforms");
			sln2.Sections[1].PreSolution.Should(Be.False);
			sln2.Sections[1].Text.ShouldEqual(
					"\t\t{DE8DC42E-C399-4367-8E34-735B7F9AA54C}.Debug|x86.ActiveCfg = Debug|x86\n" +
					"\t\t{DE8DC42E-C399-4367-8E34-735B7F9AA54C}.Debug|x86.Build.0 = Debug|x86\n" +
					"\t\t{DE8DC42E-C399-4367-8E34-735B7F9AA54C}.Release|x86.ActiveCfg = Release|x86\n" +
					"\t\t{DE8DC42E-C399-4367-8E34-735B7F9AA54C}.Release|x86.Build.0 = Release|x86");

			sln2.Sections[2].Name.ShouldEqual("MonoDevelopProperties");
			sln2.Sections[2].PreSolution.Should(Be.True);
			sln2.Sections[2].Text.ShouldEqual("\t\tStartupItem = CsharpConsoleProject\\CsharpConsoleProject.csproj");
		}

		[Test]
		public void can_print_out_the_sln_text_for_a_blank_solution() {
			var sln = new Solution { FormatVersion = "11.00", VisualStudioVersion = "2010" };
			sln.ToText().ShouldEqual(@"
				Microsoft Visual Studio Solution File, Format Version 11.00
				# Visual Studio 2010
				Global
				EndGlobal
				".TrimLeadingTabs(4));
		}

		[Test]
		public void can_print_out_the_sln_text_for_a_solution_with_one_section() {
			var sln = new Solution { FormatVersion = "11.00", VisualStudioVersion = "2010" };
			sln.Add(new Section { Name = "SolutionProperties", PreSolution = true, Text = "\t\tHideSolutionNode = FALSE" });
			sln.ToText().ShouldEqual(@"
				Microsoft Visual Studio Solution File, Format Version 11.00
				# Visual Studio 2010
				Global
					GlobalSection(SolutionProperties) = preSolution
						HideSolutionNode = FALSE
					EndGlobalSection
				EndGlobal
				".TrimLeadingTabs(4));
		}

		[Test]
		public void can_print_out_the_sln_text_for_a_solution_with_one_project() {
			var sln = new Solution { FormatVersion = "11.00", VisualStudioVersion = "2010", AutoGenerateProjectConfigurationPlatforms = false };
			sln.Add(new Project { Name = "CoolProject", Path = "src\\CoolProject.csproj", Id = new Guid("5791AA11-CBF2-4B79-BCB8-E7C1C7882F3E") });
			sln.ToText().ShouldEqual(@"
				Microsoft Visual Studio Solution File, Format Version 11.00
				# Visual Studio 2010
				Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""CoolProject"", ""src\CoolProject.csproj"", ""{5791AA11-CBF2-4B79-BCB8-E7C1C7882F3E}""
				EndProject
				Global
				EndGlobal
				".TrimLeadingTabs(4));
		}

		[Test]
		public void can_print_out_the_sln_text_for_a_solution_with_multiple_projects_and_sections() {
			var sln = new Solution { FormatVersion = "11.00", VisualStudioVersion = "2010" };
			sln.Add(new Section { Name = "SolutionConfigurationPlatforms", PreSolution  = true });
			sln.Add(new Section { Name = "ProjectConfigurationPlatforms",  PostSolution = true });
			sln.Add(new Project { Name = "CoolProject", Path = "src\\CoolProject.csproj", Id = new Guid("5791AA11-CBF2-4B79-BCB8-E7C1C7882F3E") });
			sln.Add(new Project { Name = "FooProject", Path = "src\\FooProject.csproj", Id = new Guid("F68046A5-0C57-4765-B6D8-4F1E1140E991") });
			sln.AutoGenerateProjectConfigurationPlatforms = false;
			sln.ToText().ShouldEqual(@"
				Microsoft Visual Studio Solution File, Format Version 11.00
				# Visual Studio 2010
				Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""CoolProject"", ""src\CoolProject.csproj"", ""{5791AA11-CBF2-4B79-BCB8-E7C1C7882F3E}""
				EndProject
				Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""FooProject"", ""src\FooProject.csproj"", ""{F68046A5-0C57-4765-B6D8-4F1E1140E991}""
				EndProject
				Global
					GlobalSection(SolutionConfigurationPlatforms) = preSolution
					EndGlobalSection
					GlobalSection(ProjectConfigurationPlatforms) = postSolution
					EndGlobalSection
				EndGlobal
				".TrimLeadingTabs(4));
		}

		[Test][Ignore]
		public void can_reload_incase_changes_were_made_on_the_file_system() {
		}

		// solution.AutoGenerateProjectConfigurationPlatforms
		[Test]
		public void can_auto_generate_project_configuration_platforms() {
			var project = new Project(Example("NET40", "Mvc3Application1", "Mvc3Application1", "Mvc3Application1.csproj"));

			var sln = new Solution();
			sln.Add(project);

			sln.AutoGenerateProjectConfigurationPlatforms = false;
			sln.ToText().ShouldEqual(@"
Microsoft Visual Studio Solution File, Format Version 11.00
# Visual Studio 2010
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""Mvc3Application1"", ""CSPROJ"", ""{ABE3332A-1495-4703-A248-7E47B6F871FC}""
EndProject
Global
EndGlobal
				".TrimLeadingTabs(4).Replace("CSPROJ", project.Path));

			sln.AutoGenerateProjectConfigurationPlatforms = true;
			sln.ToText().ShouldEqual(@"
Microsoft Visual Studio Solution File, Format Version 11.00
# Visual Studio 2010
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""Mvc3Application1"", ""CSPROJ"", ""{ABE3332A-1495-4703-A248-7E47B6F871FC}""
EndProject
Global
	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Release|Any CPU = Release|Any CPU
	EndGlobalSection
	GlobalSection(ProjectConfigurationPlatforms) = postSolution
		{ABE3332A-1495-4703-A248-7E47B6F871FC}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{ABE3332A-1495-4703-A248-7E47B6F871FC}.Debug|Any CPU.Build.0 = Debug|Any CPU
		{ABE3332A-1495-4703-A248-7E47B6F871FC}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{ABE3332A-1495-4703-A248-7E47B6F871FC}.Release|Any CPU.Build.0 = Release|Any CPU
	EndGlobalSection
EndGlobal
				".TrimLeadingTabs(4).Replace("CSPROJ", project.Path));
		}

		[Test][Ignore]
		public void can_save_blank_solution() {
		}

		[Test]
		public void can_add_project() {
			var solution = new Solution(Temp("Foo.sln"));
			solution.Add(new Project(Example("NET40", "ConsoleApplication1", "ConsoleApplication1", "ConsoleApplication1.csproj")));
			solution.Save();

			// Make sure it persists properly and we can re-read it
			solution = new Solution(Temp("Foo.sln"));
			solution.ToText().ShouldEqual(@"
				Microsoft Visual Studio Solution File, Format Version 11.00
				# Visual Studio 2010
				Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""ConsoleApplication1"", ""../examples/NET40/ConsoleApplication1/ConsoleApplication1/ConsoleApplication1.csproj"", ""{5C401261-E42F-4E97-B94C-2E495DF0D26D}""
				EndProject
				Global
					GlobalSection(SolutionConfigurationPlatforms) = preSolution
						Debug|Any CPU = Debug|Any CPU
						Release|Any CPU = Release|Any CPU
					EndGlobalSection
					GlobalSection(ProjectConfigurationPlatforms) = postSolution
						{5C401261-E42F-4E97-B94C-2E495DF0D26D}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
						{5C401261-E42F-4E97-B94C-2E495DF0D26D}.Debug|Any CPU.Build.0 = Debug|Any CPU
						{5C401261-E42F-4E97-B94C-2E495DF0D26D}.Release|Any CPU.ActiveCfg = Release|Any CPU
						{5C401261-E42F-4E97-B94C-2E495DF0D26D}.Release|Any CPU.Build.0 = Release|Any CPU
					EndGlobalSection
				EndGlobal
				".TrimLeadingTabs(4));
		}

		[Test]
		public void can_add_2_projects() {
			var solution = new Solution(Temp("Foo.sln"));
			solution.Add(new Project(Example("NET40", "ConsoleApplication1", "ConsoleApplication1", "ConsoleApplication1.csproj")));
			solution.Save();

			// Make sure it persists properly and we can re-read it
			solution = new Solution(Temp("Foo.sln"));
			solution.Add(new Project(Example("NET20", "ClassLibrary1", "ClassLibrary1", "ClassLibrary1.csproj")));
			solution.ToText().ShouldEqual(@"
				Microsoft Visual Studio Solution File, Format Version 11.00
				# Visual Studio 2010
				Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""ConsoleApplication1"", ""../examples/NET40/ConsoleApplication1/ConsoleApplication1/ConsoleApplication1.csproj"", ""{5C401261-E42F-4E97-B94C-2E495DF0D26D}""
				EndProject
				Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""ClassLibrary1"", ""../examples/NET20/ClassLibrary1/ClassLibrary1/ClassLibrary1.csproj"", ""{0DFFEFFC-B415-4661-A864-99B38C75DE37}""
				EndProject
				Global
					GlobalSection(SolutionConfigurationPlatforms) = preSolution
						Debug|Any CPU = Debug|Any CPU
						Release|Any CPU = Release|Any CPU
					EndGlobalSection
					GlobalSection(ProjectConfigurationPlatforms) = postSolution
						{5C401261-E42F-4E97-B94C-2E495DF0D26D}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
						{5C401261-E42F-4E97-B94C-2E495DF0D26D}.Debug|Any CPU.Build.0 = Debug|Any CPU
						{5C401261-E42F-4E97-B94C-2E495DF0D26D}.Release|Any CPU.ActiveCfg = Release|Any CPU
						{5C401261-E42F-4E97-B94C-2E495DF0D26D}.Release|Any CPU.Build.0 = Release|Any CPU
						{0DFFEFFC-B415-4661-A864-99B38C75DE37}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
						{0DFFEFFC-B415-4661-A864-99B38C75DE37}.Debug|Any CPU.Build.0 = Debug|Any CPU
						{0DFFEFFC-B415-4661-A864-99B38C75DE37}.Release|Any CPU.ActiveCfg = Release|Any CPU
						{0DFFEFFC-B415-4661-A864-99B38C75DE37}.Release|Any CPU.Build.0 = Release|Any CPU
					EndGlobalSection
				EndGlobal
				".TrimLeadingTabs(4));
		}
	}
}
