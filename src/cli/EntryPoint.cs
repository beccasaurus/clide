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

		[Application]
		public static Response RunCommands(Request req) {
			// Global options ...
			var globalOptions = new OptionSet();
			foreach (var option in Global.Options) {
				Console.WriteLine("Adding {0}", option.MonoOptionsString);
				globalOptions.Add(option.MonoOptionsString, option.InvokedWith);
			}
			var extra = globalOptions.Parse(req.Arguments);

			// var options = new OptionSet {
			// 	{ "V|verbose:", v => Console.WriteLine("the value of v is {0}", v) }
			// };
			// options.Parse(req.Arguments);

			return new Response("Hello from mack");
		}
	}
}
