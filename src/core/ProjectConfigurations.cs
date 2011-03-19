using System;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using FluentXml;

namespace Clide {

	/// <summary>API for getting/setting a Project's configurations</summary>
	public class ProjectConfigurations : IEnumerable<Configuration> {

		/// <summary>Main constructor.  ProjectConfigurations must have a project.</summary>
		public ProjectConfigurations(Project project) {
			Project = project;
		}

		/// <summary>The Project that these configurations are for</summary>
		public virtual Project Project { get; set; }

		/// <summary>Provide a generic enumerator for our Configurations</summary>
		public IEnumerator<Configuration> GetEnumerator() {
			return GetConfigurations().GetEnumerator();
		}

		/// <summary>Provide a non-generic enumerator for our Configurations</summary>
		IEnumerator IEnumerable.GetEnumerator() {
			return GetConfigurations().GetEnumerator();
		}

		/// <summary>Configuration count</summary>
		public virtual int Count { get { return GetConfigurations().Count; } }

		/// <summary>Returns the Configuration at the given index</summary>
		public virtual Configuration this[int index] { get { return GetConfigurations()[index]; } }

		/// <summary>Returns the Configuration with the given name (or null if it doesn't exist)</summary>
		public virtual Configuration this[string configurationName] { get { return GetConfiguration(configurationName); } }

		/// <summary>Returns all of the project configurations *except* the Default configuration</summary>
		public virtual List<Configuration> Custom {
			get { return GetConfigurations().Where(config => ! config.IsGlobal).ToList(); }
		}

		/// <summary>Actual method to go and get and return Configurations.</summary>
		/// <remarks>
		/// Note, this is not cached!  Hence, why it's a method instead of a property.
		///
		/// We'll very likely cache this later, but I don't want to add caching before it's truly necessary.
		/// </remarks>
		public virtual List<Configuration> GetConfigurations() {
			return Project.Doc.Nodes("PropertyGroup").Select(node => new Configuration(this, node)).ToList();
		}

		/// <summary>Get an existing configuration with the given name.  Null is a valid "name" and will return the "global" configuration.</summary>
		public virtual Configuration GetConfiguration(string configurationName) {
			return GetConfigurations().FirstOrDefault(config => config.Name == configurationName);
		}

		/// <summary>Finds or creates the "Global" configuration node</summary>
		public virtual Configuration AddGlobalConfiguration() {
			return FindOrCreateConfiguration(null);
		}

		/// <summary>Finds or creates a configuration with the given name</summary>
		public virtual Configuration Add(string configurationName) {
			return FindOrCreateConfiguration(configurationName);	
		}

		/// <summary>If a configuration with the given name exists, we return it, else we make a new one and return it.</summary>
		public virtual Configuration FindOrCreateConfiguration(string configurationName) {
			return GetConfiguration(configurationName) ?? NewConfiguration(configurationName);
		}

		/// <summary>Make a new configuration with the given name.</summary>
		public virtual Configuration NewConfiguration(string configurationName) {
			var node = Project.Doc.Node("Project").NewNode("PropertyGroup");
			if (configurationName != null)
				node.Attr("Condition", string.Format(" '$(Configuration)|$(Platform)' == '{0}|AnyCPU' ", configurationName));
			return GetConfiguration(configurationName);
		}

		/// <summary>Get properties for the configuration with this name.  Returns null if configuration not found.</summary>
		public virtual ConfigurationProperties PropertiesFor(string configurationName) {
			var config = GetConfiguration(configurationName);
			return (config == null) ? null : config.Properties;
		}

		/// <summary>Returns the "Global" configuration</summary>
		public virtual Configuration Global { get { return GetConfiguration(null); } }
	}
}
