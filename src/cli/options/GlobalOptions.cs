using System;
using System.Linq;
using System.Collections.Generic;

namespace Clide {

	/// <summary>Represents a List of GlobalOption.  Wraps the list, providing a few helper methods</summary>
	public class GlobalOptions : List<GlobalOption>, IList<GlobalOption>, IEnumerable<GlobalOption> {

		/// <summary>Returns the GlobalOption with the provided name (or null)</summary>
		public virtual GlobalOption this[string name] {
			get { return this.FirstOrDefault(option => option.Name == name); }
		}
	}
}
