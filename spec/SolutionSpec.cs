using System;
using System.Linq;
using NUnit.Framework;

namespace NVS.Specs {

	[TestFixture]
	public class SolutionSpec : Spec {

		[Test]
		public void can_read_projects() {
			var sln1 = new Solution(Example("NET20", "WebApplication1", "WebApplication1.sln"));
			sln1.Projects.Count.ShouldEqual(1);
			sln1.Projects.First().ShouldHaveProperties(new {
				Name = "WebApplication1",
				Path = @"WebApplication1\WebApplication1.csproj",
				Id   = Guid.Parse("11FC4B99-DB31-4D0C-A472-4F794098F900")
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
		}

		[Test][Ignore]
		public void can_add_a_project() {
		}

		[Test][Ignore]
		public void can_remove_a_project() {
		}

		[Test][Ignore]
		public void can_edit_sections() {
		}
	}
}
