using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Clide {

	/// <summary>Represents a 'GlobalSection' in a Solution .sln file</summary>
	public class Section {

		/// <summary>The name of this section taken from GlobalSection(NAME)</summary>
		public virtual string Name { get; set; }

		/// <summary>Whether this Section is a 'preSolution' section</summary>
		public virtual bool PreSolution { get; set; }

		/// <summary>Whether this Section is a 'postSolution' section</summary>
		public virtual bool PostSolution {
			get { return ! PreSolution;  }
			set { PreSolution = ! value; }
		}

		/// <summary>The text between this section's GlobalSection and EndGlobalSection lines</summary>
		public virtual string Text { get; set; }
	}
}
