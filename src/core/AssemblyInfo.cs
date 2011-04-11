using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Clide {

	/// <summary>Stupid little class for getting info about an assembly.</summary>
	/// <remarks>
	/// To use this, load up a RemoteAppDomainProxy instance into another AppDomain and 
	/// ask it to give you the AssemblyInfo for the assembly at a given path.
	/// </remarks>
	[Serializable]
	public class AssemblyInfo {
		public virtual string Name     { get; set; }
		public virtual string FullName { get; set; }
	}
}
