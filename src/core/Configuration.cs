using System;
using System.Xml;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentXml;

namespace Clide {

	/// <summary>Represents a Project's configuration, eg. "Debug|x86"</summary>
	/// <remarks>
	/// In MSBuild project files (eg. csproj), configurations aren't explicitly defined.
	///
	/// IDEs like Visual Studio and MonoDevelop persist different configurations as PropertyGroup 
	/// elements in project files with a Condition that matches the configuration's and platform.
	///
	/// This pretty much just wraps that Name and Platform.
	/// </remarks>
	public class Configuration : IXmlNode {

		static readonly Regex _getNameAndPlatform = new Regex(@"==\s*'([^\|]+)\|([^']+)'");

		/// <summary>Configuration constructor.  A Configuration requires an XmlNode and the ProjectConfigurations object</summary>
		public Configuration(ProjectConfigurations configurations, XmlNode node) {
			Configurations = configurations;
			Node           = node;
		}

		ConfigurationProperties _properties;

		/// <summary>The XmlNode that this Configuration is stored in</summary>
		public virtual XmlNode Node { get; set; }

		/// <summary>The ProjectConfigurations that this Configuration is a part of</summary>
		public virtual ProjectConfigurations Configurations { get; set; }

		/// <summary>Returns all all of this configuration's properties</summary>
		public virtual ConfigurationProperties Properties {
			get { return _properties ?? (_properties = new ConfigurationProperties(this)); }
			set { _properties = value; }
		}

		/// <summary>Set or get the text of the given propertyName</summary>
		public virtual string this[string propertyName] {
			get { return Properties.GetText(propertyName); }
			set { Properties.SetText(propertyName, value); }
		}

		/// <summary>Returns the Property with this name for this configuration, if it exists, else null</summary>
		public virtual Property GetProperty(string propertyName) {
			return Properties.GetProperty(propertyName);
		}

		/// <summary>This configuration's name, eg. "Debug" or "Release"</summary>
		public virtual string Name {
			get {
				var nameAndPlatform = GetNameAndPlatform();
				return (nameAndPlatform == null) ? null : nameAndPlatform.Groups[1].ToString();
			}
		}

		/// <summary>This configuration's platform, eg. "x86" or "AnyCPU"</summary>
		public virtual string Platform {
			get {
				var nameAndPlatform = GetNameAndPlatform();
				return (nameAndPlatform == null) ? null : nameAndPlatform.Groups[2].ToString();
			}
		}

		/// <summary>Returns whether or not this is the "Global" configuration.  Currently, this is true when the Name is null.</summary>
		public virtual bool IsGlobal { get { return Name == null; } }

		/// <summary>String representation of this configuration, eg. "Debug|x86"</summary>
		public override string ToString() { return IsGlobal ? "Global" : string.Format("{0}|{1}", Name, Platform); }

		/// <summary>Remove this configuration from the Project.  Calling Project.Save() will persist this change.</summary>
		public virtual void Remove() {
			Node.ParentNode.RemoveChild(Node);
		}

	// private

		Match GetNameAndPlatform() {
			var condition = Node.Attr("Condition");
			if (condition == null)
				return null;
			else
				return _getNameAndPlatform.Match(condition);
		}
	}
}
