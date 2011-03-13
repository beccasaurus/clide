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
			Invoke(args).Execute();
		}

		/// <summary>Returns the ConsoleRack.Response that calling our CLI with these arguments results in</summary>
		public static Response Invoke(string[] args) {
			return Crack.Invoke(args);
		}

		[Application]
		public static Response RunCommands(Request req) {
			var arguments = new List<string>(req.Arguments);
			var firstArg  = arguments.First(); arguments.RemoveAt(0);
			var commands  = Global.Commands.Match(firstArg);
			req.Arguments = arguments.ToArray();

			if (commands.Count == 0)
				return new Response("Command not found: {0}", firstArg);
			else if (commands.Count == 1)
				return commands.First().Invoke(req);
			else {
				var ambiguous = string.Join(", ", commands.Select(c => c.Name).ToArray());
				return new Response("{0} is ambiguous with commands: {1}", firstArg, ambiguous);
			}
		}
	}
}
