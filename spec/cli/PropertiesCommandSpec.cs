using System;
using System.Linq;
using NUnit.Framework;
using ConsoleRack;
using Clide;

namespace Clide.Specs {

	[TestFixture]
	public class PropertiesCommandSpec : Spec {

		[Test][Description("clide properties")][Ignore]
		public void clide_properties() {
		}

		[Test][Description("clide help properties")][Ignore]
		public void clide_help_properties() {
		}

		[Test][Description("clide properties -c Release")][Ignore]
		public void clide_properties_release() {
		}

		[Test][Description("clide properties -c Global")][Ignore]
		public void clide_properties_global() {
		}

		[Test][Description("clide properties OutputPath")][Ignore]
		public void clide_properties_get_property() {
		}

		[Test][Description("clide properties OutputPath=bin")][Ignore]
		public void clide_properties_set_property() {
		}

		[Test][Description("clide properties OutputPath=bin Different=\"Hi\" This=\"that\" -c Release")][Ignore]
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
