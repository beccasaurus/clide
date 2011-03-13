using System;
using NUnit.Framework;
using ConsoleRack;
using Clide;

namespace Clide.Specs {

	[TestFixture]
	public class NewCommandSpec : Spec {

		[Test][Description("clide help new")]
		public void clide_help_new() {
			Clide("help", "new").Text.ShouldEqual("Helpful information about the 'new' command\n");
		}

		[Test][Description("clide new")]
		public void clide_new() {
			Clide("new").Text.ShouldEqual("Helpful information about the 'new' command\n");
		}

		/// <summary>Does the same thing as -d|--default</summary>
		[Test][Description("clide new MyProject")]
		public void clide_new_Name() {
			Clide("new", "MyProject").Text.ShouldEqual("... fixing current directory ...");
		}

		[Test][Description("clide new MyProject -d|--default")][Ignore]
		public void clide_new_Name_default() {
		}

		[Test][Description("clide new MyProject -b|--bare")][Ignore]
		public void clide_new_Name_bare() {
		}
	}
}
