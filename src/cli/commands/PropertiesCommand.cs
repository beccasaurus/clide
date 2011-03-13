using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Options;
using ConsoleRack;
using Clide.Extensions;

namespace Clide {

	/// <summary>clide properties</summary>
	public class PropertiesCommand {

		[Command("properties", "Get or set configuration properties")]
		public static Response Invoke(Request req) { return new PropertiesCommand(req).Invoke(); }

		public PropertiesCommand(Request req) {}

		public Response Invoke() { return null; }
	}
}
