using System;
using System.Linq;

namespace Mack {

	/// <summary>Represents a reference to an Assembly in a Project's configuration</summary>
	public class Reference {

		string _hintPath;

		/// <summary>This reference's name, eg. "System" or "MyAssembly, Version=1.0, Culture=neutral, ..."</summary>
		public virtual string FullName { get; set; }

		/// <summary>The value of this Reference node's HintPath node</summary>
		/// <remarks>
		/// This should be relative to the project file and should be normalized (should use backslashes)
		/// </remarks>
		public virtual string HintPath {
			get { return _hintPath; }
			set { _hintPath = Project.NormalizePath(value); }
		}

		/// <summary>The value of this Reference node's SpecificVersion node</summary>
		public virtual bool SpecificVersion { get; set; }

		/// <summary>This reference's short name, eg. "System" or "MyAssembly."  This reads from FullName.</summary>
		public virtual string Name { get { return FullName.Split(',').FirstOrDefault(); } }
	}
}
