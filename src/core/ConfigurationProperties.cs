using System;
using System.Xml;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using FluentXml;

namespace Clide {

	/// <summary>Thin wrapper around a List of Property for a given Configuration</summary>
	/// <remarks>
	/// You should NOT modify this list and expect any changes to occur.
	///
	/// Instead, please look at Configuration.AddProperty() or Configuration.RemoveProperty().
	///
	/// You can update the value of a property freely, and those changes will be saved when you call project.Save()
	/// </remarks>
	public class ConfigurationProperties : IEnumerable<Property> {

		/// <summary>The constructor for ConfigurationProperties.  Requires a Configuration.</summary>
		public ConfigurationProperties(Configuration configuration) {
			Configuration = configuration;
		}

		/// <summary>This ConfigurationProperties's parent Configuration</summary>
		public virtual Configuration Configuration { get; set; }

		/// <summary>Provide a generic enumerator for our Properties</summary>
		public IEnumerator<Property> GetEnumerator() {
			return GetProperties().GetEnumerator();
		}

		/// <summary>Provide a non-generic enumerator for our Properties</summary>
		IEnumerator IEnumerable.GetEnumerator() {
			return GetProperties().GetEnumerator();
		}

		/// <summary>Property count</summary>
		public virtual int Count { get { return GetProperties().Count; } }

		/// <summary>Actual method to go and get and return Properties.</summary>
		/// <remarks>
		/// Note, this is not cached!  Hence, why it's a method instead of a property.
		///
		/// We'll very likely cache this later, but I don't want to add caching before it's truly necessary.
		/// </remarks>
		public virtual List<Property> GetProperties() {
			return Configuration.Node.Nodes().Select(node => new Property(this, node)).ToList();
		}

		/// <summary>Returns a property with the given name or null (if it doesn't exist)</summary>
		public virtual Property GetProperty(string propertyName) {
			return GetProperties().FirstOrDefault(prop => prop.Name == propertyName);
		}

		/// <summary>Returns an existing property with this name, if it exists, else creates a new property.</summary>
		public virtual Property FindOrCreateProperty(string propertyName) {
			return GetProperty(propertyName) ?? NewProperty(propertyName);
		}

		/// <summary>Creates and returns a new property with the given name</summary>
		public virtual Property NewProperty(string propertyName) {
			Configuration.Node.NewNode(propertyName);
			return GetProperty(propertyName);
		}

		/// <summary>Returns the text of the property with the given name or null, if it doesn't exist.</summary>
		public virtual string GetText(string propertyName) {
			return GetProperty(propertyName).Text;
		}

		/// <summary>Sets the text of property with this name to this value.  Creates a new property if it doesn't exist.</summary>
		public virtual void SetText(string propertyName, string value) {
			FindOrCreateProperty(propertyName).Text = value;
		}

		/// <summary>Exposes a simple way to get or set the Text of a property</summary>
		/// <remarks>
		/// This is for getting and setting property Text, not the properties themselves.
		/// </remarks>
		public virtual string this[string propertyName] {
			get { return GetText(propertyName); }
			set { SetText(propertyName, value); }
		}
	}
}
