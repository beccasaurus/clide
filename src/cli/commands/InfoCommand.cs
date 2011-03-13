using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Options;
using ConsoleRack;
using Clide.Extensions;
using IO.Interfaces;

namespace Clide {

	/// <summary>clide info</summary>
	public class InfoCommand {

		[Command("info", "Prints out info about environment/configuration")]
		public static Response Invoke(Request req) {
			var response = new Response();

			// For now, this just prints out all global options ...
			foreach (var option in Global.Options)
				response.Append("{0}: {1}\n", option.Name, option.Value);

			return response;
		}
	}
}
