using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Options;
using ConsoleRack;
using Clide.Extensions;
using IO.Interfaces;

namespace Clide {

	/// <summary>clide properties</summary>
	public class PropertiesCommand {

		[Command("properties", "Get or set configuration properties")]
		public static Response Invoke(Request req) {
			var project = new Project(Global.Project);
			if (project.DoesNotExist())
				return new Response("Project not found: {0}", project.Path);

			var config = Global.UseGlobal ? project.Global : project.Config[Global.Configuration];
			if (config == null)
				return new Response("Configuration not found in project: {0}", Global.UseGlobal ? "GLOBAL" : Global.Configuration);

			var response = new Response("Selected configuration: {0}", Global.UseGlobal ? "GLOBAL" : config.Name);
			foreach (var property in config.Properties)
				response.Append("{0}: {1}\n", property.Name, property.Text);
			return response;
		}
	}
}
