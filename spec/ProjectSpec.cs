using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

namespace NVS.Specs {

	[TestFixture]
	public class ProjectSpec : Spec {

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
		public void Path_is_normalized_to_use_backslashes_instead_of_forward_slashes() {
			var paths = new Dictionary<string,string> {
				{ "foo",          "foo"           },
				{ "foo/bar",      "foo\\bar"      },
				{ "Hello World",  "Hello World"   },
				{ "/hello",       "\\hello"       },
				{ "hi / there",   "hi \\ there"   },
				{ "src\\foo/bar", "src\\foo\\bar" }
			};

			foreach (var path in paths)
				new Project { Path = path.Key }.Path.ShouldEqual(path.Value);
		}
	}
}
