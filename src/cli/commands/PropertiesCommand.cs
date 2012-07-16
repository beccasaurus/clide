using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NDesk.Options;
using ConsoleRack;
using Clide.Extensions;
using IO.Interfaces;

namespace Clide {

	/// <summary>clide properties</summary>
	public class PropertiesCommand {

		public static string HelpText {
			get { return @"
Usage: clide properties [Name][=Value] [Name2][=Value2] [...]

  Examples:
    clide prop                         Displays all of the Debug properties for the current project
    clide prop -P src/Foo.csproj       Displays all of the Debug properties for the provided project
    clide prop -G                      Displays all of the Global properties for the current project
    clide prop OutputPath              Displays the value of the OutpathPath Debug property
    clide prop OutputPath -C Release   Displays the value of the OutpathPath Release property
    clide prop OutputPath DebugType    Displays the values of the OutputPath and DebugType properties
    clide prop OutputPath=bin          Sets the value of the OutputPath Debug propety to 'bin'
    clide prop OutputPath=bin X=Y      Sets the values of the OutputPath and X Debug properties

COMMON".Replace("COMMON", Global.CommonOptionsText).TrimStart('\n'); }
		}

		[Command("properties", "Get or set configuration properties")]
		public static Response Invoke(Request req) {
			if (Global.Help) return new Response(HelpText);
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
