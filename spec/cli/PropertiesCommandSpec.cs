using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ConsoleRack;
using Clide;

namespace Clide.Specs {

	[TestFixture]
	public class PropertiesCommandSpec : Spec {

		[SetUp]
		public void Before() {
			base.BeforeEach();
			File.Copy(Example("FluentXml.Specs.csproj"), Temp("FluentXml.Specs.csproj"));
		}

		[Test][Description("clide help properties")][Ignore]
		public void clide_help_properties() {
		}

		// Defaults to Debug
		[Test][Description("clide properties")]
		public void clide_properties() {
			var output = Clide("properties").Text;
			output.ShouldContain(@"OutputPath: ..\bin\Debug");
			output.ShouldNotContain(@"OutputPath: ..\bin\Release");
		}

		[Test][Description("clide properties --config Release")]
		public void clide_properties_release() {
			var release = Clide("properties", "--config", "Release").Text;
			release.ShouldContain("Selected configuration: Release");
			release.ShouldContain(@"OutputPath: ..\bin\Release");
			release.ShouldNotContain(@"OutputPath: ..\bin\Debug");

			var debug = Clide("properties", "--config", "Debug").Text;
			debug.ShouldContain("Selected configuration: Debug");
			debug.ShouldContain(@"OutputPath: ..\bin\Debug");
			debug.ShouldNotContain(@"OutputPath: ..\bin\Release");
		}

		[Test][Description("clide properties --global")]
		public void clide_properties_global() {
			var global = Clide("properties", "--global").Text;
			global.ShouldContain("Selected configuration: GLOBAL");
			global.ShouldNotContain(@"OutputPath");
			global.ShouldContain("TargetFrameworkVersion: v4.0");
			global.ShouldContain("OutputType: Library");
		}

		[Test][Description("clide properties OutputPath")][Ignore]
		public void clide_properties_get_property() {
		}

		[Test][Description("clide properties OutputPath=bin")][Ignore]
		public void clide_properties_set_property() {
		}

		[Test][Description("clide properties OutputPath=bin Different=\"Hi\" This=\"that\" --config Release")][Ignore]
		public void clide_properties_setting_many_properties() {
		}

		[Test][Description("clide properties OutputPath=\"My Directory\"")][Ignore]
		public void clide_properties_set_property_with_quote() {
		}

		[Test][Description("clide properties \"OutputPath=My Directory\"")][Ignore]
		public void clide_properties_set_property_with_quote_alt() {
		}

		[Test][Description("clide properties OutputPath=\"My Directory = Hi There\"")][Ignore]
		public void clide_properties_set_property_that_has_an_equal_sign_in_value() {
		}
	}
}
