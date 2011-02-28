using System;
using ConsoleRack;

namespace Mack {

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
			return new Response("Hello from mack");
		}
	}
}
