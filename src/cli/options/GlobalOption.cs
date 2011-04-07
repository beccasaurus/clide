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
		public GlobalOption(char shortArgument, string longArgument, string name, string environmentVariable, string argumentType, object defaultValue, string description) {
			ShortArgument       = shortArgument;
			LongArgument        = longArgument;
			Name                = name;
			EnvironmentVariable = environmentVariable;
			DefaultValue        = defaultValue;
			Description         = description;

			switch (argumentType.ToLower()) {
				case "none":
					AcceptsArgument = false;
					break;
				case "optional":
					AcceptsArgument  = true;
					ArgumentRequired = false;
					break;
				case "required":
					AcceptsArgument  = true;
					ArgumentRequired = true;
					break;
			}
		}

		object _value;

		/// <summary>This option's display name, eg. Verbosity</summary>
		public virtual string Name { get; set; }

		/// <summary>This option's default value, eg. true</summary>
		public virtual object DefaultValue { get; set; }

		/// <summary>Returns this option's DefaultValue.  If the value is an Action, we invoke it.  Else we return it directly.</summary>
		public virtual object GetDefaultValue() {
			if (DefaultValue == null)
				return null;
			else if (DefaultValue is Func<object>)
				return (DefaultValue as Func<object>).Invoke();
			else
				return DefaultValue;
		}

		/// <summary>Gets or sets the actual value.  If EnvironmentVariable is not null, we fall back to this (if it is set)</summary>
		public virtual object Value {
			get { return _value ?? ValueFromEnvironmentVariable ?? GetDefaultValue(); }
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

		/// <summary>Whether or not this option accepts an argument</summary>
		public virtual bool AcceptsArgument { get; set; }

		/// <summary>Whether or not an argument is required (assuming AcceptsArgument is true)</summary>
		public virtual bool ArgumentRequired { get; set; }

		/// <summary>Returns the string to use to register this option with Mono.Options</summary>
		public virtual string MonoOptionsString {
			get {
				if (AcceptsArgument)
					if (ArgumentRequired)
						return string.Format("{0}|{1}=", ShortArgument, LongArgument);
					else
						return string.Format("{0}|{1}:", ShortArgument, LongArgument);
				else
					return string.Format("{0}|{1}", ShortArgument, LongArgument);
			}
		}

		/// <summary>This method gets called with the command line argument passed to this option (if any) whenever this option is called</summary>
		public virtual void InvokedWith(string value) {
			if (Global.Debug)
				Console.WriteLine("Global Option {0}. Value: '{1}'", Name, value);

			if (AcceptsArgument)
				Value = value;
			else
				Value = true; // If this option doesn't accept an argument, then it must be a boolean argument
		}

		/// <summary>Returns the Value as a string</summary>
		public override string ToString() {
			return (Value == null) ? "" : Value.ToString();
		}

		/// <summary>Safely parses the Value to a bool</summary>
		public virtual bool ToBool() {
			if (Value == null)
				return false;

			if (Value.GetType() == typeof(bool))
				return (bool) Value;

			if (Value.ToString().ToLower().Contains("true"))
				return true;
			else
				return false;
		}

		/// <summary>Returns text explaining these arguments (meant to be used by Global.CommonOptionsText), eg. "-v, --version"</summary>
		public virtual string ArgumentsText {
			get {
				if (AcceptsArgument)
					if (ArgumentRequired)
						return string.Format("-{0}, --{1} {2}", ShortArgument, LongArgument, EnvironmentVariable);
					else
						return string.Format("-{0}, --{1} [{2}]", ShortArgument, LongArgument, EnvironmentVariable);
				else
					return string.Format("-{0}, --{1}", ShortArgument, LongArgument);
			}
		}
	}
}
