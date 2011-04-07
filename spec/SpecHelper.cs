using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using Clide;

namespace Clide.Specs {

	/// <summary>Global Before and After Hooks for all Clide.Specs</summary>
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

		[SetUp]
		public void BeforeEach() {
			if (Directory.Exists(TempRoot)) Directory.Delete(TempRoot, true);
			Directory.CreateDirectory(TempRoot);
			Global.ResetOptions();
			Global.WorkingDirectory = TempRoot;
            Environment.SetEnvironmentVariable("CLIDE_TEMPLATES", "no-exist"); // make sure no templates show up by default
		}

		[TearDown]
		public void AfterEach() {
			// nothing yet	
		}

		/// <summary>Lists the files in the current working directory</summary>
		public void Ls() {
			foreach (var file in Directory.GetDirectories(Global.WorkingDirectory))
				Console.WriteLine(file + "/");
			foreach (var file in Directory.GetFiles(Global.WorkingDirectory))
				Console.WriteLine(file);
		}

		/// <summary>Returns the Response object that calling Clide with these arguments generates</summary>
		public ConsoleRack.Response Clide(params string[] arguments) {
			return EntryPoint.Invoke(arguments);
		}

		public string ProjectRoot  { get { return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..")); } }
		public string ExamplesRoot { get { return Path.GetFullPath(Path.Combine(ProjectRoot, "spec", "content", "examples"));  } }
		public string TempRoot     { get { return Path.GetFullPath(Path.Combine(ProjectRoot, "spec", "content", "tmp"));       } }

		public string Example(params string[] parts) {
			var allParts = new List<string>(parts);
			allParts.Insert(0, ExamplesRoot);
			return Path.GetFullPath(Path.Combine(allParts.ToArray()));
		}

		public string Temp(params string[] parts) {
			var allParts = new List<string>(parts);
			allParts.Insert(0, TempRoot);
			return Path.GetFullPath(Path.Combine(allParts.ToArray()));
		}

        /// <summary>This takes a relative path like "Foo/Bar" and will replace the slashes with Windows ones, if on Windows</summary>
        public virtual string Rel(string path) {
            if (Environment.OSVersion.ToString().Contains("Windows"))
                return path.Replace("/", "\\");
            else
                return path; // assume that we use Unix paths everywhere
        }
	}
}
