using System;
using System.Linq;
using System.Collections.Generic;
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

		[Middleware]
		public static Response SplashScreen(Request req, Application app) {
			if (req.Arguments.Length == 0)
				return new Response("Splash!");
			else
				return app.Invoke(req);
		}

		[Command("foo", "does stuff")]
		public static Response FooCommand(Request req) {
			return new Response("You called the Foo command!");
		}

		[Command("foot", "does stuff")]
		public static Response FootCommand(Request req) {
			return new Response("You called the FooT command!");
		}

		[Application]
		public static Response RunCommands(Request req) {
			var arguments = new List<string>(req.Arguments);
			var firstArg  = arguments.First(); arguments.RemoveAt(0);
			var commands  = Crack.Commands.Match(firstArg);
			req.Arguments = arguments.ToArray();

			if (commands.Count == 0)
				return new Response("Command not found: {0}", firstArg);
			else if (commands.Count > 1)
				return new Response("{0} is ambiguous with commands: {1}", firstArg, string.Join(", ", commands.Select(c => c.Name).ToArray()));
			else
				return commands.First().Invoke(req);
		}
	}
}
