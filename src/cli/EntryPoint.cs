using System;
using Mono.Options;
using ConsoleRack;

namespace Clide {

	/// <summary>Class with the Main() method for mack.exe</summary>
	public static class EntryPoint {

		/// <summary>The main entry point for mack.exe</summary>
		public static void Main(string[] args) {
			Crack.Run(args);
		}

		// TODO we'll move this stuff out of here ... just playing around ...

		[Middleware]
		public static Response Version(Request req, Application app) {
			Console.WriteLine("i am middleware ...");
			return app.Invoke(req);
		}

		[Middleware]
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

		[Application]
		public static Response RunCommands(Request req) {
			// Handle commands!
			return new Response("Hello from mack");
		}
	}
}
