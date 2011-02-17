using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace NVS {

	/// <summary>Represents a .sln solution file</summary>
	public class Solution {

		public Solution() {}
		public Solution(string path) {
			
		}

		public virtual List<Project> Projects { get { return null; } }
	}
}
