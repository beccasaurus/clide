using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Options;
using ConsoleRack;
using Clide.Extensions;

namespace Clide {

	/// <summary>For now, we're putting all commands in here.  When we have more, we'll organize this.</summary>
	public static class AllCommands {

		[Command("commands", "List the available commands")]
		public static Response CommandsCommand(Request req) {
			var response = new Response("clide commands:\n");
			var commands = Global.Commands;
			var spaces   = commands.Select(c => c.Name.Length).Max() + 4;
			foreach (var command in commands)
				response.Append("    {0}{1}\n", command.Name.WithSpaces(spaces), command.Description);
			return response;
		}

		[Command("help", "Provide help on the 'clide' command")]
		public static Response HelpCommand(Request req) {
			var args = new List<string>(req.Arguments);

			if (args.Count == 0)
				return new Response(@"
CLIDE is a CLI IDE for .NET

  Usage:
    clide -h/--help
    clide -v/--version
    clide command [arguments...] [options...]

  Examples:
    clide new ProjectName
    clide prop RootNamespace=Foo
    clide ref add ../lib/Foo.dll
    clide gen

  Further help:
    clide commands         list all 'clide' commands
    clide help <COMMAND>   show help on COMMAND

  Further information:
    https://github.com/remi/clide".TrimStart('\n'));

			var commandName = args.First(); args.RemoveAt(0); // Shift() 
			var command     = Global.Commands.FirstOrDefault(cmd => cmd.Name == commandName);
			if (command == null)
				return new Response("Command not found: {0}", commandName);
			else {
				Global.Help = true;
				req.Arguments = args.ToArray();
				return command.Invoke(req);
			}
		}
	}
}
