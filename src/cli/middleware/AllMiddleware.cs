using System;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;
using NDesk.Options;
using ConsoleRack;

namespace Clide {

	/// <summary>For now, we're putting all middleware in here.  When we have more, we'll organize this.</summary>
	public static class AllMiddleware {

		[Middleware("Catches Exceptions", First = true)]
		public static Response CatchExceptions(Request req, Application app) {
			try {
				return app.Invoke(req);
			} catch (Exception ex) {
				while (ex.InnerException != null && ex is TargetInvocationException)
					ex = ex.InnerException;
				return new Response("Oh noes!\n\n{0}\n\n{1}", ex.Message, ex);
			}
		}

		[Middleware("Processes and strips our all of our global options, eg. -V/--verbose", First = true)]
		public static Response ProcessGlobalOptions(Request req, Application app) {
			var globalOptions = new OptionSet();

			// register options
			foreach (var option in Global.Options)
				globalOptions.Add(option.MonoOptionsString, option.InvokedWith);

			// parse.  we get back a List<string> with un-used arguments
			var extra = globalOptions.Parse(req.Arguments);

			req.Arguments = extra.ToArray();

			return app.Invoke(req);
		}

		[Middleware("If no arguments were passed, display a splash screen")]
		public static Response SplashScreen(Request req, Application app) {
			if (req.Arguments.Length == 0)
				return new Response(@"
   _____  _       _      _       
  / ____|| |     (_)    | |      
 | |     | |      _   __| |  ___ 
 | |     | |     | | / _` | / _ \
 | |____ | |____ | || (_| ||  __/
  \_____||______||_| \__,_| \___|						

   CLIDE is a CLI IDE for .NET

Run clide help for help documentation".TrimStart('\n'));
			else
				return app.Invoke(req);
		}

		[Middleware("Displays the current Clide version if -v/--version are passed")]
		public static Response DisplayVersion(Request req, Application app) {
			if (req.Arguments.Contains("-v") || req.Arguments.Contains("--version"))
				return new Response("clide " + FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion.ToString());
			else
				return app.Invoke(req);
		}
	}
}
