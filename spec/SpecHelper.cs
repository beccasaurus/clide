using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;

namespace NVS.Specs {

	/// <summary>Global Before and After Hooks for all NVC.Specs</summary>
	[SetUpFixture]
	public class SpecsSetup {

		[SetUp]
		public void BeforeAll() {
			// ...
		}

		[TearDown]
		public void AfterAll() {
			// ...
		}
	}

	/// <summary>Base class for our specs ... for helper methods and whatnot</summary>
	public class Spec {
		public string ProjectRoot  { get { return Path.Combine(Directory.GetCurrentDirectory(), "..", ".."); } }
		public string ExamplesRoot { get { return Path.Combine(ProjectRoot, "spec", "content", "examples");  } }

		public string Example(params string[] parts) {
			var allParts = new List<string>(parts);
			allParts.Insert(0, ExamplesRoot);
			return Path.GetFullPath(Path.Combine(allParts.ToArray()));
		}
	}
}
