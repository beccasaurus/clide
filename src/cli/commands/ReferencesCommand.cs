using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mono.Options;
using ConsoleRack;
using IO.Interfaces;
using Clide.Extensions;

namespace Clide {

	// TODO extract stuff out of here into a base ClideCommand class (for parsing, etc)
	/// <summary>clide references</summary>
	public class ReferencesCommand {

		[Command("references", "Manage a Project's references")]
		public static Response ReferencesCmd(Request req) { return new ReferencesCommand(req).Invoke(); }

		public ReferencesCommand(Request request) {
			Request = request;
		}

		public virtual string HelpText {
			get { return @"
Usage: clide references [add|rm] [gac|dll|csproj]

  Examples:
    clide ref                         Displays all of the GAC/DLL/Project references for the current project
    clide ref -P src/Foo.csproj       Displays all of the references for the provided project
    clide ref add lib/Foo.dll         Adds a reference to Foo.dll to the current project (using relative path)
    clide ref add System.Xml          Adds a reference to the System.Xml assembly in the GAC
    clide ref add spec/Specs.csproj   Adds a project reference to the Specs.csproj project (using relative path)
    clide ref rm lib/Foo.dll          Removes reference to Foo.dll to the current project
    clide ref rm System.Xml           Removes reference to the System.Xml assembly in the GAC
    clide ref rm spec/Specs.csproj    Removes project reference to the Specs.csproj project (using relative path)

COMMON".Replace("COMMON", Global.CommonOptionsText).TrimStart('\n'); }
		}

		public virtual Request Request { get; set; }

		public virtual Response Invoke() {
			if (Global.Help) return new Response(HelpText);
			ParseOptions();

			if (Request.Arguments.Length == 0)
				return PrintReferences();

			var args          = Request.Arguments.ToList();
			var subCommand    = args.First(); args.RemoveAt(0);
			Request.Arguments = args.ToArray();

			switch (subCommand.ToLower()) {
				case "add":  return AddReferences();
				case "rm":   return RemoveReferences();
				default:
					return new Response("Unknown references subcommand: {0}", subCommand);
			}
		}

		public virtual Response PrintReferences() {
			return new Response("This would print out references");
		}

		public virtual Response AddReferences() {
			// TODO - this really needs to stop and we need to fix this!  Global.Project needs to be a PROJECT OBJECT!
			var project = new Project(Global.Project);
			if (project.DoesNotExist())
				return new Response("No project found"); // this should use STERR ... Helper: new Response("", error: true) .... or Response.Error()

			if (Request.Arguments.Length == 0)
				return new Response("No references passed to add?");

			var response = new Response();
			foreach (var reference in Request.Arguments) {
				AddReference(response, reference, project);
			}

			project.Save();
			return response;
		}

        // TODO - move out of here
        [Serializable]
        public class AssemblyInfo {
            public virtual string Name     { get; set; }
            public virtual string FullName { get; set; }
        }

        // TODO - move out of here
        public class RemoteAppDomainThingy : MarshalByRefObject {
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
        }

        // TODO - move out of here
        /// <summary>Given a path to a DLL, this returns back null if we couldn't load the DLL, else an AssemblyInfo</summary>
        public static AssemblyInfo GetAssemblyInfo(string path) {
            if (! File.Exists(path)) return null;

            var appDomain = AppDomain.CreateDomain(
                friendlyName: string.Format("{0}-DomainFor-{1}", DateTime.Now.Ticks, Path.GetFileName(path)),
                securityInfo: AppDomain.CurrentDomain.Evidence,
                info:         AppDomain.CurrentDomain.SetupInformation
            );

            try {
                // Get a reference to a AssemblyInfo object (loaded in our other AppDomain) ... it will do the work for us ...
                var remoteType     = typeof(RemoteAppDomainThingy);
                var remoteInstance = appDomain.CreateInstanceFrom(assemblyFile: remoteType.Assembly.Location, typeName: remoteType.FullName).Unwrap() as RemoteAppDomainThingy;
                return remoteInstance.GetInfoForAssembly(path);
            } finally {
                AppDomain.Unload(appDomain);
            }
        }

		public virtual void AddReference(Response response, string reference, Project project) {
			var path = Path.Combine(Global.WorkingDirectory, reference);
			if (path.AsFile().DoesNotExist()) {
				project.References.AddGacReference(reference);
				response.Append("Added reference {0} to {1}\n", reference, project.Name);
				return;
			}

			// It's a MSBuild project file?
			if (reference.ToLower().EndsWith("proj")) {
				var referencedProject = new Project(reference);
				var projectDir        = Path.GetFullPath(project.Path).AsFile().DirName();
				var relativePath      = Project.NormalizePath(projectDir.AsDir().Relative(reference).TrimStart('/').TrimStart('\\'));
				response.Append("Added reference {0} to {1}\n", referencedProject.Name, project.Name);
				project.ProjectReferences.Add(referencedProject.Name, relativePath, referencedProject.Id);
				return;
			}

			AssemblyInfo assemblyInfo;

			// Try to read the assembly info to populate the Reference.FullName (<Reference Include="" />
            assemblyInfo = GetAssemblyInfo(path);

			if (assemblyInfo == null) {
				project.References.AddDll(Path.GetFileName(reference), reference);
				response.Append("Couldn't load assembly: {0}.  Adding anyway." + Environment.NewLine, reference);
				response.Append("Added reference {0} to {1}\n", Path.GetFileName(reference), project.Name);
			} else {
                project.References.AddDll(assemblyInfo.FullName, reference);
			    response.Append("Added reference {0} to {1}\n", assemblyInfo.Name, project.Name);
            }
		}

		public virtual Response RemoveReferences() {
			return new Response("This would REMOVE references");
		}

		public void ParseOptions() {}
	}
}
