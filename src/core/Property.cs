using System;
using System.Xml;
using FluentXml;

namespace Clide {

	/// <summary>Represents a Property of a Project, eg. "OutputPath"</summary>
	public class Property {

		/// <summary>The constructor for Property. A Property requires its XmlNode and its parent ConfigurationProperties</summary>
		public Property(ConfigurationProperties properties, XmlNode node) {
			Properties = properties;
			Node       = node;
		}

		/// <summary>The node in the project XML representing this</summary>
		public virtual XmlNode Node { get; set; }

		/// <summary>The ConfigurationProperties collection of properties that this is associated with</summary>
		public virtual ConfigurationProperties Properties { get; set; }

		// /// <summary>The Project this property is associated with</summary>
		// public virtual Project Project { get; set; }

		// /// <summary>The Configuration this property is associated with.</summary>
		// /// <remarks>
		// /// If this is null, it's assumed that this is a "Global" project property property
		// /// </remarks>
		// public virtual Configuration Configuration { get; set; }

		/// <summary>This property's name, taken from the node name, eg. "OutputPath" or "WarningLevel"</summary>
		public virtual string Name { get { return Node.Name; } }

		/// <summary>The text defined in the project file for this property</summary>
		/// <remarks>
		/// Don't be confused.  This does NOT represent the "Value" of this property.
		///
		/// Properties may use variables in their text to reference environment variables, etc.
		///
		/// We don't currently support a way of getting the actual runtime value of properties, 
		/// because Clide really doesn't care.  Clide helps you create/edit project files, not 
		/// build/execute the project.
		/// </remarks>
		public virtual string Text {
			get { return Node.Text(); }
			set { Node.Text(value); }
		}

		/// <summary>If this property node has a Condition attribute, this is its text</summary>
		public virtual string Condition {
			get { return Node.Attr("Condition"); }
			set { Node.Attr("Condition", value); }
		}

		/// <summary>Remove this property from the Configuration.  Calling Project.Save() will persist this change.</summary>
		public virtual void Remove() {
			Node.ParentNode.RemoveChild(Node);
		}
	}
}
