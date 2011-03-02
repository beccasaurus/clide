using System;
using System.Linq;
using System.Collections.Generic;
using Mono.Options;
using ConsoleRack;

namespace Clide {

	/// <summary>For now, we're putting all middleware in here.  When we have more, we'll organize this.</summary>
	public static class AllMiddleware {

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
				return new Response("Splash!");
			else
				return app.Invoke(req);
		}
	}
}
