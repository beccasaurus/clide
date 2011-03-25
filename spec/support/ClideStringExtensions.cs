using System;
using System.Text;
using System.Text.RegularExpressions;

namespace NUnit.Framework {

    // String Extensions to help us test that our Project XML and Solution Text is what we expect it to be
	public static class ClideStringExtensions {

		/// <summary>Fixes XML by Chomp()-ing the leading newline, trimming tabs, and replacing single quotes with double quotes</summary>
		public static string FixXml(this string str) {
			var fixedXml = str.TrimStart('\n').TrimLeadingTabs(4).Replace("'", "\"");

            // On Windows, the XML we get sometimes doens't have a newline before the closing </Project> tag ... weird, eh?
            // This only seems to happen if the Project has no content, so you just have <xml>\n<Project></Project> ...
            if (Environment.OSVersion.ToString().Contains("Windows"))
                if (fixedXml.Split('\n').Length < 4)
                    fixedXml = fixedXml.Replace("\r\n</Project>", "</Project>");

            return fixedXml;
		}

		/// <summary>Let's us easily remove the leading tabs from a string ... useful when we define lots of inline text in our tests</summary>
        /// <remarks>
        /// TODO: Merge this with FixXml and just use FixXml ... maybe?
        /// 
        /// This does way more than just TrimLeadingTabs ... it also converts newlines from unix -> Windows when running on Windows
        /// </remarks>
		public static string TrimLeadingTabs(this string str, int numberOfTabsToTrim) {
			var tabs = GetTabs(numberOfTabsToTrim);

			// if a line starts with (^) these tab characters, replace those tabs with ""
			var trimmed = Regex.Replace(str, @"^" + tabs, string.Empty, RegexOptions.Multiline);

            if (Environment.OSVersion.ToString().Contains("Windows"))
                trimmed = trimmed.Replace("\n", "\r\n");

            return trimmed;
		}

		public static string GetTabs(int numberOfTabs) {
			string tabs = "";
			for (int i = 0; i < numberOfTabs; i++)
				tabs += "\t";
			return tabs;
		}

        /// <summary>Removes the beginning newline (Windows or unix)</summary>
        public static string TrimStartNewline(this string str) {
            return str.TrimStart(Environment.NewLine.ToCharArray());
        }
	}
}
