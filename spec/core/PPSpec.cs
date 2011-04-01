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

		[Test][Ignore]
		public void can_replace_project_properties_in_a_file_given_a_project() {
		}

		[Test][Ignore]
		public void can_replace_project_properties_in_all_files_in_a_directory_given_a_project() {
		}
	}
}
