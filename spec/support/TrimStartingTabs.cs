using System;
using System.Text;
using System.Text.RegularExpressions;

namespace NUnit.Framework {
	public static class TrimLeadingTabsExtension {

		/// <summary>Let's us easily remove the leading tabs from a string ... useful when we define lots of inline text in our tests</summary>
		public static string TrimLeadingTabs(this string str, int numberOfTabsToTrim) {
			var tabs = GetTabs(numberOfTabsToTrim);

			// if a line starts with (^) these tab characters, replace those tabs with ""
			return Regex.Replace(str, @"^" + tabs, string.Empty, RegexOptions.Multiline);
		}

		public static string GetTabs(int numberOfTabs) {
			string tabs = "";
			for (int i = 0; i < numberOfTabs; i++)
				tabs += "\t";
			return tabs;
		}
	}
}
