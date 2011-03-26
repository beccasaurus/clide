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

		public virtual Request Request { get; set; }

		public virtual Response Invoke() {
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

        public class AssemblyInfo {
            public virtual string Name     { get; set; }
            public virtual string FullName { get; set; }
        }

        /// <summary>Given a path to a DLL, this returns back null if we couldn't load the DLL, else an AssemblyInfo</summary>
        public static AssemblyInfo GetAssemblyInfo(string path) {
            Console.WriteLine("Trying to load: {0}", path);

            if (! File.Exists(path)) return null;

            // Setup the new AppDomain
            var appDomainName = string.Format("{0}-DomainForFile-{1}", DateTime.Now.Ticks, Path.GetFileNameWithoutExtension(path));
            var domainSetup   = new AppDomainSetup { ApplicationName = appDomainName, ApplicationBase = Directory.GetCurrentDirectory() };
            var appDomain     = AppDomain.CreateDomain(appDomainName, null, domainSetup);

            // Grrr ... see: http://www.codeproject.com/Articles/42312/Loading-Assemblies-in-Separate-Directories-Into-a-.aspx?msg=3468132&display=Mobile
            // We need to clean this up and do it "properly" ...

            /*
            appDomain.ReflectionOnlyAssemblyResolve

            // We don't need to resolve dependencies - we're just loading the assembly to get it's name
            AppDomain.CurrentDomain.AssemblyResolve += (o,e) => {
                return appDomain.Load(e.Name);

                Console.WriteLine("Trying to load dependency: {0}", e.Name);
                // return e.Name.Contains("clide") ? appDomain.Load(File.ReadAllBytes(Assembly.GetExecutingAssembly().Location))) : null;
                // if (e.Name.Contains("clide")) {
                //     Console.WriteLine("Trying to load Clide");
                //     return Assembly.GetExecutingAssembly();
                // } else {
                //     Console.WriteLine("Hmm ... trying to load something else ... {0}", e.Name);
                // }
                return null;
            };
             * */

            try {
                var assembly = appDomain.Load(File.ReadAllBytes(path));
                return new AssemblyInfo {
                    Name     = assembly.GetName().Name,
                    FullName = assembly.FullName
                };
            } catch (Exception ex) {
                Console.WriteLine("BOOM!  {0}", ex);
                return null;
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
                // URHERE - this won't go up dirs and make a path like ..\..\foo ... i think?
				Console.WriteLine("dir: {0}", projectDir);
				// Console.WriteLine("reference: {0}", reference);
				// Console.WriteLine("relative ref: {0}", projectDir.AsDir().Relative(reference));
				var relativePath      = projectDir.AsDir().Relative(reference).TrimStart('/').TrimStart('\\');
                Console.WriteLine("relative: {0}", relativePath);
				project.ProjectReferences.Add(referencedProject.Name, relativePath, referencedProject.Id);
				response.Append("Added reference {0} to {1}\n", referencedProject.Name, project.Name);
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
