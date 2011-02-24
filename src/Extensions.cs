using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using IO.Interfaces;

// For now, this is miscellaneous extension methods ... we'll split it out into more files, as needed

namespace NVS.Extensions {

	public static class StringExtensions {
		public static Guid ToGuid(this string guid) {
			var str = guid.TrimStart('{').TrimEnd('}'); // in sln and csproj files, guids are often wrapped with curly braces
			if (str.Length != 36) throw new Exception(string.Format("This doesn't look like a valid GUID, it's not 36 characters: {0}", str));
			return new Guid(str);
		}
	}
}
