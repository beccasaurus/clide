using System;
using System.Linq;
using System.Collections.Generic;
using Mono.Options;
using ConsoleRack;

namespace Clide {

	/// <summary>For now, we're putting all commands in here.  When we have more, we'll organize this.</summary>
	public static class AllCommands {

		[Command("info", "Prints out ... whatever")]
		public static Response InfoCommand(Request req) {
			return new Response("info ...");
		}

		[Command("commands", "List the available commands")]
		public static Response CommandsCommand(Request req) {
			var response = new Response();
			Crack.Commands.ForEach(cmd => response.Append("{0}\t{1}\n", cmd.Name, cmd.Description));
			return response;
		}
	}
}
