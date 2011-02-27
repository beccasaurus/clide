using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace NVS.Specs {

	[TestFixture]
	public class SolutionSpec : Spec {

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
				Path          = @"WebApplication1\WebApplication1.csproj",
				Id            = Guid.Parse("11FC4B99-DB31-4D0C-A472-4F794098F900"),
				ProjectTypeId = Guid.Parse("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC")
			});

			var sln2 = new Solution(Example("MonoDevelop", "NET35", "CsharpConsoleProject", "CsharpConsoleProject.sln"));
			sln2.Projects.Count.ShouldEqual(1);
			sln2.Projects.First().ShouldHaveProperties(new {
				Name          = "CsharpConsoleProject",
				Path          = @"CsharpConsoleProject\CsharpConsoleProject.csproj",
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
			sln1.Sections[0].Text.ShouldEqual("Debug|Any CPU = Debug|Any CPU\nRelease|Any CPU = Release|Any CPU");

			sln1.Sections[1].Name.ShouldEqual("ProjectConfigurationPlatforms");
			sln1.Sections[1].PreSolution.Should(Be.False);
			sln1.Sections[1].Text.ShouldEqual(
					"{11FC4B99-DB31-4D0C-A472-4F794098F900}.Debug|Any CPU.ActiveCfg = Debug|Any CPU\n" + 
					"{11FC4B99-DB31-4D0C-A472-4F794098F900}.Debug|Any CPU.Build.0 = Debug|Any CPU\n" + 
					"{11FC4B99-DB31-4D0C-A472-4F794098F900}.Release|Any CPU.ActiveCfg = Release|Any CPU\n" + 
					"{11FC4B99-DB31-4D0C-A472-4F794098F900}.Release|Any CPU.Build.0 = Release|Any CPU");

			sln1.Sections[2].Name.ShouldEqual("SolutionProperties");
			sln1.Sections[2].PreSolution.Should(Be.True);
			sln1.Sections[2].Text.ShouldEqual("HideSolutionNode = FALSE");

			// ---------
			
			var sln2 = new Solution(Example("MonoDevelop", "NET35", "CsharpConsoleProject", "CsharpConsoleProject.sln"));
			sln2.Sections.Count.ShouldEqual(3);

			sln2.Sections[0].Name.ShouldEqual("SolutionConfigurationPlatforms");
			sln2.Sections[0].PreSolution.Should(Be.True);
			sln2.Sections[0].Text.ShouldEqual("Debug|x86 = Debug|x86\nRelease|x86 = Release|x86");

			sln2.Sections[1].Name.ShouldEqual("ProjectConfigurationPlatforms");
			sln2.Sections[1].PreSolution.Should(Be.False);
			sln2.Sections[1].Text.ShouldEqual(
					"{DE8DC42E-C399-4367-8E34-735B7F9AA54C}.Debug|x86.ActiveCfg = Debug|x86\n" +
					"{DE8DC42E-C399-4367-8E34-735B7F9AA54C}.Debug|x86.Build.0 = Debug|x86\n" +
					"{DE8DC42E-C399-4367-8E34-735B7F9AA54C}.Release|x86.ActiveCfg = Release|x86\n" +
					"{DE8DC42E-C399-4367-8E34-735B7F9AA54C}.Release|x86.Build.0 = Release|x86");

			sln2.Sections[2].Name.ShouldEqual("MonoDevelopProperties");
			sln2.Sections[2].PreSolution.Should(Be.True);
			sln2.Sections[2].Text.ShouldEqual(@"StartupItem = CsharpConsoleProject\CsharpConsoleProject.csproj");
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
			sln.Add(new Section { Name = "SolutionProperties", PreSolution = true, Text = "HideSolutionNode = FALSE" });
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
			var sln = new Solution { FormatVersion = "11.00", VisualStudioVersion = "2010" };
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

		[Test][Ignore]
		public void can_print_out_the_sln_text_for_a_solution_with_projects_and_sections() {
			var sln = new Solution { FormatVersion = "11.00", VisualStudioVersion = "2010" };
		}

		[Test][Ignore]
		public void can_save_a_new_solution() {
			var solution = new Solution(Temp("Foo.sln"));
			File.Exists(Temp("Foo.sln")).Should(Be.False);

			/// ... NOTE: I want to add a blank sln, then add some stuff to it but:
			///
			/// 1. I need to make a real blank VS solution and see what's valid
			/// 2. Until I can read a *proj to get the configurations, i can't write the config part of the sln without just guessing
		}

		[Test][Ignore]
		public void can_add_a_project() {
			var newPath = Temp("Foo.sln");
			var sln1 = new Solution(Example("NET20", "WebApplication1", "WebApplication1.sln")).Parse();
			sln1.Projects.Count.ShouldEqual(1);

			sln1.Projects.Add(new Project { Name = "Awesome.Project", Path = "src/Totally Neat/code" });

			File.Exists(newPath).Should(Be.False);
			// sln1.Save(newPath);
			// sln1.Path.ShouldEqual(newPath);
			File.Exists(newPath).Should(Be.True);

			var saved = new Solution(newPath);
			saved.Projects.Count.ShouldEqual(1);
		}

		[Test][Ignore]
		public void can_remove_a_project() {
		}

		[Test][Ignore]
		public void can_edit_a_project() {
		}

		[Test][Ignore]
		public void can_edit_sections() {
		}

		[Test][Ignore]
		public void can_reload_incase_changes_were_made_on_the_file_systel() {
		}
	}
}
