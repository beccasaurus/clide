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
			var response = new Response();

			var project = new Project(Global.Project);
			if (project.DoesNotExist())
				return new Response("Project not found: {0}", project.Path);

			var config = Global.UseGlobal ? project.Global : project.Config[Global.Configuration];
			if (config == null)
				return new Response("Configuration not found in project: {0}", Global.UseGlobal ? "GLOBAL" : Global.Configuration);

			if (req.Arguments.Length == 0) {
				response.Append("Selected configuration: {0}\n", Global.UseGlobal ? "GLOBAL" : config.Name);
				foreach (var property in config.Properties)
					response.Append("{0}: {1}\n", property.Name, property.Text);
				return response;
			}

			var madeChanges = false;
			foreach (var arg in req.Arguments) {
				var indexOfEquals = arg.IndexOf("=");
				var propertyName  = (indexOfEquals > -1) ? arg.Substring(0, indexOfEquals)  : arg;
				var propertyValue = (indexOfEquals > -1) ? arg.Substring(indexOfEquals + 1) : null;

				if (arg.Contains("=")) {
					response.Append("Setting {0} to {1}\n", propertyName, propertyValue);
					config[propertyName] = propertyValue;
					madeChanges = true;
				} else {
					response.Append("{0}\n", config[propertyName]);
				}
			}

			if (madeChanges) project.Save();

			return response;
		}
	}
}
