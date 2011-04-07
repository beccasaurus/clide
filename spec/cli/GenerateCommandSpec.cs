using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ConsoleRack;
using Clide;

namespace Clide.Specs {

	[TestFixture]
	public class GenerateCommandSpec : Spec {

		[SetUp]
		public void Before() {
			base.BeforeEach();
			File.Copy(Example("FluentXml.Specs.csproj"), Temp("FluentXml.Specs.csproj"));
			Environment.SetEnvironmentVariable("CLIDE_TEMPLATES", Example("templates"));
		}

		[TearDown]
		public void After() {
			base.AfterEach();
			Environment.SetEnvironmentVariable("CLIDE_TEMPLATES", null);	
		}

		[Test]
		public void clide_gen() {
			Clide("gen").Text.ShouldContain("basic:");
			Clide("gen").Text.ShouldContain("Create basic something or other"); // <-- description
		}

		[Test]
		public void clide_help_gen() {
			Clide("help", "gen").Text.ShouldContain("Usage: clide generate [Template] [TemplateOptions]");
		}

		[Test][Ignore]
		public void clide_gen_basic() {
			Clide("gen", "basic").Text.ShouldContain("clide gen basic Name [Foo=] [Bar=]"); // <--- prints the usage
		}

		[Test]
		public void can_specify_an_output_directory() {
			Directory.Exists(Temp("Foo")).Should(Be.False);

			Clide("gen", "basic", "The Project Name", "-o", "Foo");

			Directory.Exists(Temp("Foo")).Should(Be.True);
			File.Exists(Temp("Foo", "README.markdown")).Should(Be.True);
			File.ReadAllText(Temp("Foo", "README.markdown")).ShouldEqual("# The Project Name is the coolest project\n\nFoo was set to $foo$\n\nBar was set to $bar$\n");
			File.Exists(Temp("Foo", "Models", "User.cs")).Should(Be.True);
			File.Exists(Temp("Foo", "Models", "$foo$.cs")).Should(Be.True);
		}

		[Test]
		public void can_specify_a_template_directory_if_name_isnt_found() {
			Environment.SetEnvironmentVariable("CLIDE_TEMPLATES", null);	
			
			Clide("gen", "basic").Text.ShouldContain("Template not found: basic");

			Clide("gen", Example("templates", "basic")).Text.ShouldContain("Usage:\n  clide gen basic Name [Foo=] [Bar=]\n"); // <--- usage was found!
		}

		[Test]
		public void clide_gen_basic_arg1() {
			File.Exists(Temp("README.markdown")).Should(Be.False);

			Clide("gen", "basic", "TheFirstArg");

			File.Exists(Temp("README.markdown")).Should(Be.True);
			File.ReadAllText(Temp("README.markdown")).ShouldEqual("# TheFirstArg is the coolest project\n\nFoo was set to $foo$\n\nBar was set to $bar$\n");
		}

		[Test]
		public void clide_gen_foo_equals_something() {
			File.Exists(Temp("README.markdown")).Should(Be.False);

			Clide("gen", "basic", "foo=This is foo");

			File.Exists(Temp("README.markdown")).Should(Be.True);
			File.ReadAllText(Temp("README.markdown")).ShouldEqual("# $ARG1$ is the coolest project\n\nFoo was set to This is foo\n\nBar was set to $bar$\n");
		}

		[Test]
		public void clide_gen_foo_arg1_and_equals_something() {
			File.Exists(Temp("README.markdown")).Should(Be.False);

			Clide("gen", "basic", "foo=This is foo", "MyArg", "bar=hi", "another");

			File.Exists(Temp("README.markdown")).Should(Be.True);
			File.ReadAllText(Temp("README.markdown")).ShouldEqual("# MyArg is the coolest project\n\nFoo was set to This is foo\n\nBar was set to hi\n");
		}
	}
}
