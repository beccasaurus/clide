using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Clide {

	/// <summary>An object that we can instantiate in another AppDomain and marshal back info about stuff in that domain</summary>
	public class RemoteAppDomainProxy : MarshalByRefObject {

		/// <summary>Returns basic information about the assembly with the given name (loads it into this instance's AppDomain!)</summary>
		/// <remarks>
		/// Do NOT do this unless you've created this instance in another domain ... otherwise there's really no point!
		/// </remarks>
		public virtual AssemblyInfo GetInfoForAssembly(string assemblyPath) {
			Assembly assembly = null;

			// Incase we care about loading up dependencies ...
			AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += (o,e) => null;

			try {
				assembly = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
			} catch (FileNotFoundException) {
				// Continue loading, even if we couldn't find a referenced assembly
			} catch (BadImageFormatException) {
				return null; // Not a valid Assembly?
			}

			return new AssemblyInfo {
				Name     = assembly.GetName().Name,
				FullName = assembly.FullName
			};
		}

		/// <summary>Static method that you should use to actually interact with RemoteAppDomainProxy/AssemblyInfo</summary>
		/// <remarks>
		/// Given a path to a DLL, this returns back null if we couldn't load the DLL, else an AssemblyInfo
		/// </remarks>
        public static AssemblyInfo GetAssemblyInfo(string path) {
            if (! File.Exists(path)) return null;

            var appDomain = AppDomain.CreateDomain(
                friendlyName: string.Format("{0}-DomainFor-{1}", DateTime.Now.Ticks, Path.GetFileName(path)),
                securityInfo: AppDomain.CurrentDomain.Evidence,
                info:         AppDomain.CurrentDomain.SetupInformation
            );

            try {
                // Get a reference to a AssemblyInfo object (loaded in our other AppDomain) ... it will do the work for us ...
                var remoteType     = typeof(RemoteAppDomainProxy);
                var remoteInstance = appDomain.CreateInstanceFrom(assemblyFile: remoteType.Assembly.Location, typeName: remoteType.FullName).Unwrap() as RemoteAppDomainProxy;
                return remoteInstance.GetInfoForAssembly(path);
            } finally {
                AppDomain.Unload(appDomain);
            }
        }
	}
}
