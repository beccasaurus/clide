using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using FluentXml;
using Clide.Extensions;

namespace Clide.Specs {

	[TestFixture]
	public class PPSpec : Spec {

		[SetUp]
		public void Before() {
			base.BeforeEach();
			File.Copy(Example("FluentXml.Specs.csproj"), Temp("FluentXml.Specs.csproj"));
			File.Copy(Example("NET40", "Mvc3Application1", "Mvc3Application1", "Mvc3Application1.csproj"), Temp("Mvc3Application1.csproj"));
		}

		[Test]
		public void can_replace_arbitrary_tokens_not_associated_with_a_project() {
			var text = "hello $foo$ the $money$ costs $9.95 and $bar foo$ is really cool!";

			// Dictionary<string,object>
			PP.Replace(text, new Dictionary<string,object>
					{ {"foo", 5} }).ShouldEqual("hello 5 the $money$ costs $9.95 and $bar foo$ is really cool!");
			PP.Replace(text, new Dictionary<string,object>
					{ {"foo", 5.5}, {"bar foo", "HI"} }).ShouldEqual("hello 5.5 the $money$ costs $9.95 and HI is really cool!");
			PP.Replace(text, new Dictionary<string,object>
					{ {" the ", true} }).ShouldEqual("hello $fooTruemoney$ costs $9.95 and $bar foo$ is really cool!");
			PP.Replace(text, new Dictionary<string,object>
					{ {" the ", true}, {"9.95 and ", 5} }).ShouldEqual("hello $fooTruemoney$ costs 5bar foo$ is really cool!");

			// Dictionary<string,string>
			PP.Replace(text, new Dictionary<string,string>
					{ {"foo", "5"} }).ShouldEqual("hello 5 the $money$ costs $9.95 and $bar foo$ is really cool!");
			PP.Replace(text, new Dictionary<string,string>
					{ {"foo", "5.5"}, {"bar foo", "HI"} }).ShouldEqual("hello 5.5 the $money$ costs $9.95 and HI is really cool!");
			PP.Replace(text, new Dictionary<string,string>
					{ {" the ", "ABC"} }).ShouldEqual("hello $fooABCmoney$ costs $9.95 and $bar foo$ is really cool!");
			PP.Replace(text, new Dictionary<string,string>
					{ {" the ", "ABC"}, {"9.95 and ", "5"} }).ShouldEqual("hello $fooABCmoney$ costs 5bar foo$ is really cool!");

			// anonymous object
			PP.Replace(text, new { foo = 5 }).ShouldEqual("hello 5 the $money$ costs $9.95 and $bar foo$ is really cool!");
			PP.Replace(text, new { foo = 5, money = "WINNING" }).ShouldEqual("hello 5 the WINNING costs $9.95 and $bar foo$ is really cool!");
		}

		[Test]
		public void can_replace_project_properties_in_a_string_given_a_project() {
			var project = new Project(Temp("FluentXml.Specs.csproj"));

			var text = "I $configuration$ $WarningLevel$ times faster than $Platform$! It's $DebugSymbols$";

			// Uses Global and default configurations, by default
			PP.Replace(text, project).ShouldEqual("I Debug 4 times faster than AnyCPU! It's true");

			PP.Replace(text, project, includeGlobal: false).ShouldEqual("I Debug 4 times faster than $Platform$! It's true");

			PP.Replace(text, project, config: "Release", includeGlobal: false).ShouldEqual("I Release 4 times faster than $Platform$! It's $DebugSymbols$");
			PP.Replace(text, project, config: "Release", includeGlobal: true).ShouldEqual("I Release 4 times faster than AnyCPU! It's $DebugSymbols$");
		}

		// TODO Should test lots of things ... no output path ... output path as file ... as dir ... split up and make new [Test] for these
		[Test]
		public void can_replace_project_properties_in_a_file_given_a_project() {
			File.Exists(Temp("Foo.cs")).Should(Be.False);
			File.Exists(Temp("HI")).Should(Be.False);
			File.Exists(Temp("Foo.HiThere.cs")).Should(Be.False);

			var pp = new PP {
				WorkingDirectory = Global.WorkingDirectory,
				Project          = new Project(Temp("FluentXml.Specs.csproj"))
			};

			// Outputs to the current directory by default
			pp.ProcessFile(Example("PP", "one-file", "Foo.cs.pp")).ShouldEqual(Temp("Foo.cs"));
			File.ReadAllText(Temp("Foo.cs")).ShouldEqual(@"
				// Part of Assembly: FluentXml.Specs
				namespace FluentXml.Specs {
					public class Foo {}
				}
				".TrimLeadingTabs(4).TrimStartNewline());

			// Can specify an output path
			pp.ProcessFile(Example("PP", "one-file", "Foo.cs.pp"), outputPath: Temp("HI")).ShouldEqual(Temp("HI"));
			File.ReadAllText(Temp("HI")).ShouldEqual(@"
				// Part of Assembly: FluentXml.Specs
				namespace FluentXml.Specs {
					public class Foo {}
				}
				".TrimLeadingTabs(4).TrimStartNewline());

			// Does replacements in the file name
			pp.ProcessFile(Example("PP", "one-file-with-tokens-in-name", "Foo.$Neato$.cs.pp"), tokens: new { Neato = "HiThere" }).ShouldEqual(Temp("Foo.HiThere.cs"));
			File.ReadAllText(Temp("Foo.HiThere.cs")).ShouldEqual(@"
				// Part of Assembly: FluentXml.Specs
				// Neato: HiThere
				// Another: $Another$
				namespace FluentXml.Specs {
					public class Foo {}
				}
				".TrimLeadingTabs(4).TrimStartNewline());

			// Provide $Another$ too ... provide a Dictionary, just cuz
			pp.ProcessFile(Example("PP", "one-file-with-tokens-in-name", "Foo.$Neato$.cs.pp"), 
					tokens: new Dictionary<string,string> { {"Neato","HiThere"}, {"Another", "Totally Awesome"} }).ShouldEqual(Temp("Foo.HiThere.cs"));
			File.ReadAllText(Temp("Foo.HiThere.cs")).ShouldEqual(@"
				// Part of Assembly: FluentXml.Specs
				// Neato: HiThere
				// Another: Totally Awesome
				namespace FluentXml.Specs {
					public class Foo {}
				}
				".TrimLeadingTabs(4).TrimStartNewline());

			// Doesn't process non .pp files (just copies them)
			pp.ProcessFile(Example("PP", "non-pp-file", "Foo.cs")).ShouldEqual(Temp("Foo.cs"));
			File.ReadAllText(Temp("Foo.cs")).ShouldEqual(@"
				// Part of Assembly: $AssemblyName$
				namespace $RootNamespace$ {
					public class Foo {}
				}
				".TrimLeadingTabs(4).TrimStartNewline());
		}

		[Test][Ignore]
		public void can_replace_project_properties_in_all_files_in_a_directory_given_a_project() {
		}
	}
}
