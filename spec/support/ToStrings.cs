using System;
using System.Linq;
using System.Collections.Generic;

namespace NUnit.Framework {
	public static class ToStringsExtension {
		public static string[] ToStrings<T>(this IEnumerable<T> stuff) {
			return stuff.Select(item => (item == null) ? string.Empty : item.ToString()).ToArray();
		}
		public static string ToJoinedStrings<T>(this IEnumerable<T> stuff) {
			return string.Join(", ", stuff.ToStrings());
		}
	}
}
