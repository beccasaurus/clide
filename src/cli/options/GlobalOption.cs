using System;

namespace Clide {

	/// <summary>Represents a global option for clide.exe</summary>
	/// <remarks>
	/// This can have its value set.  It can also have an environment variable that it checks if it hasn't been set manually.
	/// </remarks>
	public class GlobalOption {

		/// <summary>Default, empty constructor</summary>
		public GlobalOption(){}

		/// <summary>Constructor used in Global.cs to make it easy to define lots of options in a pretty way</summary>
		public GlobalOption(char shortArgument, string longArgument, string name, string environmentVariable, string defaultValue, string description) {
			ShortArgument       = shortArgument;
			LongArgument        = longArgument;
			Name                = name;
			EnvironmentVariable = environmentVariable;
			DefaultValue        = defaultValue;
			Description         = description;
		}

		object _value;

		/// <summary>This option's display name, eg. Verbosity</summary>
		public virtual string Name { get; set; }

		/// <summary>This option's default value, eg. true</summary>
		public virtual object DefaultValue { get; set; }

		/// <summary>Gets or sets the actual value.  If EnvironmentVariable is not null, we fall back to this (if it is set)</summary>
		public virtual object Value {
			get { return _value ?? ValueFromEnvironmentVariable; }
			set { _value = value; }
		}

		/// <summary>The name of an environment variable that we will use, if it is set, for this option's value</summary>
		public virtual string EnvironmentVariable { get; set; }

		/// <summary>If EnvironmentVariable is set, this will return the value of that EnvironmentVariable, else null.</summary>
		public virtual object ValueFromEnvironmentVariable {
			get { return (EnvironmentVariable == null) ? null : Environment.GetEnvironmentVariable(EnvironmentVariable); }
		}

		/// <summary>Description of how this option is used</summary>
		public virtual string Description { get; set; }

		/// <summary>Short command line argument that this uses, if any, eg. "D" (-D)</summary>
		public virtual char ShortArgument { get; set; }

		/// <summary>Long command line argument that this uses, if any, eg. "debug" (--debug)</summary>
		public virtual string LongArgument { get; set; }

		/// <summary>If this is true, we set this option's value to True when called (and ignore the argument passed)</summary>
		public bool SetToTrueIfCalled = true;

		/// <summary>If this is set to true, we take the command line argument and use it to set Value.  Else we simply SetToTrueIfCalled.</summary>
		public bool AcceptsArgumentValue {
			get { return ! SetToTrueIfCalled;  }
			set { SetToTrueIfCalled = ! value; }
		}
	}
}
