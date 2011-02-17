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

		[Test][Ignore]
		public void can_read_sections() {
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
