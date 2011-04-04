using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Clide {

	// TODO - I don't think I like the static methods ... they're sorta helpful, but not really needed.
	//        Might wanna delete them and re-create them later once we realize exactly which methods are useful?

	/// <summary>Class for handling "PP" (Project Properties or Pre-Processed) templates using project configuration variables</summary>
	public class PP : Tokenizer {
		public PP() : base() {
			FileExtensionToProcess = "pp";
		}
		public PP(string text) : this(){
			Text = text;
		}
		public PP(Project project) : this() {
			Project = project;
		}

		/// <summary>The project associated with this PP processor.</summary>
		public virtual Project Project { get; set; }

		/// <summary>Returns a Dictionary from the given project's properties that can be used as tokens for string replacement</summary>
		public virtual Dictionary<string,string> ProjectToDictionary(Project project, string config = null, bool includeGlobal = true) {
			if (config == null) config = project.DefaultConfigurationName;

			var properties = new Dictionary<string,string>();

			if (includeGlobal && project.Global != null)
				foreach (var property in project.Global.Properties)
					properties[property.Name] = property.Text;

			if (! string.IsNullOrEmpty(config) && project.Config[config] != null)
				foreach (var property in project.Config[config].Properties)
					properties[property.Name] = property.Text;

			// the 'configuration' token is set in the global properties, but we should override it if a config is passed in
			if (! string.IsNullOrEmpty(config)) {
				var configToken = properties.Select(prop => prop.Key).FirstOrDefault(key => key.ToLower() == "configuration");
				if (configToken == null) configToken = "configuration";
				properties[configToken] = config;
			}

			return properties;
		}

		/// <summary>Returns the result of replacing the project properties from the given project in Text</summary>
		public virtual string Render(Project project, string config = null, bool includeGlobal = true) {
			return Render(Text, project, config, includeGlobal);
		}

		/// <summary>Returns the result of replacing the project properties from the given project in the given text</summary>
		public virtual string Render(string text, Project project, string config = null, bool includeGlobal = true) {
			return Render(text, ProjectToDictionary(project, config, includeGlobal));
		}

		/// <summary>Processes the provided pp file.</summary>
		public virtual string ProcessFile(string path, string outputPath = null, Project project = null, object tokens = null) {
			if (project == null) project = Project;
			var allTokens = (project == null) ? null : ProjectToDictionary(project);
			if (tokens != null)
				foreach (var item in ToDictionary(tokens))
					allTokens[item.Key] = (item.Value == null) ? null : item.Value.ToString();
			return base.ProcessFile(path: path, outputPath: outputPath, tokens: allTokens);
		}

		/// <summary>Helper method for replacing the project properties from the given project in the given string</summary>
		public static string Replace(string text, Project project, string config = null, bool includeGlobal = true) {
			return new PP().Render(text, project, config, includeGlobal);
		}
	}

	/// <summary>Class for replacing tokens in text</summary>
	public class Tokenizer {

		public static string DefaultLeftDelimiter   = "$";
		public static string DefaultRightDelimiter  = "$";
		public static bool   DefaultCaseInsensitive = true;

		public Tokenizer() {
			LeftDelimiter   = Tokenizer.DefaultLeftDelimiter;
			RightDelimiter  = Tokenizer.DefaultRightDelimiter;
			CaseInsensitive = Tokenizer.DefaultCaseInsensitive;
		}

		public Tokenizer(string text) : this() {
			Text = text;
		}

		string _workingDirectory;

		/// <summary>The string to look for at the left of a token</summary>
		public virtual string LeftDelimiter { get; set; }

		/// <summary>The string to look for at the right of a token</summary>
		public virtual string RightDelimiter { get; set; }

		/// <summary>The text that we want to replace tokens in</summary>
		public virtual string Text { get; set; }

		/// <summary>The current working directory (defaults to Directory.GetCurrentDirectory())</summary>
		public virtual string WorkingDirectory {
			get { return _workingDirectory ?? (_workingDirectory = Directory.GetCurrentDirectory()); }
			set { _workingDirectory = value; }
		}

		/// <summary>Whether or not we should replace tokens case insensitively</summary>
		public virtual bool CaseInsensitive { get; set; }

		/// <summary>The file extension that we should process.  If this is set, we won't process files without this extension.</summary>
		public virtual string FileExtensionToProcess { get; set; }

		/// <summary>Returns the result of replacing the given tokens in Text</summary>
		public virtual string Render(Dictionary<string,object> tokens) {
			return Render(Text, tokens);
		}

		/// <summary>Returns the result of replacing the given tokens in the given string</summary>
		public virtual string Render(string text, Dictionary<string,object> tokens) {
			var stringTokens = new Dictionary<string,string>();
			foreach (var token in tokens)
				stringTokens[token.Key] = (token.Value == null) ? string.Empty : token.Value.ToString();
			return Render(text, stringTokens);
		}

		/// <summary>Returns the result of replacing the given tokens in the given string</summary>
		public virtual string Render(string text, Dictionary<string,string> tokens) {
			var builder = new StringBuilder(text);
			foreach (var token in tokens)
				ReplaceToken(builder, key: token.Key, value: token.Value.ToString());
			return builder.ToString();
		}

		/// <summary>Given a StringBuilder, replaces the given key (wrapped with LeftDelimiter and RightDelimiter) with the value</summary>
		public virtual void ReplaceToken(StringBuilder builder, string key, string value, bool? caseInsensitive = null) {
			ReplaceString(builder, string.Format("{0}{1}{2}", LeftDelimiter, key, RightDelimiter), value, caseInsensitive);
		}

		/// <summary>Given a StringBuilder, replaces the given key with the value</summary>
		public virtual void ReplaceString(StringBuilder builder, string key, string value, bool? caseInsensitive = null) {
			if (caseInsensitive == null) caseInsensitive = CaseInsensitive;
			var comparison = ((bool) caseInsensitive) ? StringComparison.OrdinalIgnoreCase : StringComparison.InvariantCulture;
			ReplaceString(builder, key, value, comparison);
		}

		/// <summary>Given a StringBuilder, replaces the given key with the value</summary>
		/// <remarks>
		/// This method does the real work of finding and replacing strings.
		///
		/// Note: this currently loops and doesn't stop until it can't find the key anymore.  It doesn't move through the text.
		/// Note: this also calls StringBuilder.ToString() more often than I'd like so it can execute IndexOf() and Substring()
		/// </remarks>
		public virtual void ReplaceString(StringBuilder builder, string key, string value, StringComparison comparison) {
			if (builder == null || builder.Length == 0 || string.IsNullOrEmpty(key)) return;

			var original = builder.ToString();
			var index    = original.IndexOf(key, comparison);

			while (index > -1) {
				// We have an index!  Let's replace all instances of the found key with the value ...
				var foundKey = original.Substring(index, key.Length);

				builder.Replace(foundKey, value); // let the builder replace all instances of the key we found

				// Get the new string and look for another index
				original = builder.ToString();
				index    = original.IndexOf(key, comparison);
			}
		}

		/// <summary>Processes the given file (with an optional output path and file extension to check for)</summary>
		/// <remarks>
		/// If the output path is null, we output to WorkingDirectory (using the same file name).
		/// If the fileExtension is null, we use FileExtensionToProcess or we process the file anyway.
		/// </remarks>
		public virtual string ProcessFile(string path, Dictionary<string,string> tokens, string outputPath = null, string fileExtension = null) {
			if (! File.Exists(path))          throw new FileNotFoundException("Could not find file to process", path);
			if (outputPath == null)           outputPath    = Path.Combine(WorkingDirectory, Path.GetFileName(path));
			if (Directory.Exists(outputPath)) outputPath    = Path.Combine(outputPath, Path.GetFileName(path));
			if (fileExtension == null)        fileExtension = FileExtensionToProcess;

			if (fileExtension != null && ! path.ToLower().EndsWith(fileExtension.ToLower())) {
				// We specified a file extension, but this file doesn't end with it.  Simply copy the file (without processing it).
				File.Copy(path, outputPath, true);
			} else {
				// Remove dot and extension if output path has extension
				if (fileExtension != null && outputPath.ToLower().EndsWith(fileExtension.ToLower()))
					outputPath = outputPath.Substring(0, outputPath.Length - fileExtension.Length - 1);

				outputPath = Replace(outputPath, tokens);

				var text = Replace(File.ReadAllText(path), tokens);
				using (var writer = new StreamWriter(outputPath))
					writer.Write(text);
			}

			return outputPath;
		}

		/// <summary>Helper method for replacing the given tokens in the given string</summary>
		public static string Replace(string text, Dictionary<string,object> tokens) {
			return new Tokenizer().Render(text, tokens);
		}

		/// <summary>Helper method for replacing the given tokens in the given string</summary>
		public static string Replace(string text, Dictionary<string,string> tokens) {
			return new Tokenizer().Render(text, tokens);
		}

		/// <summary>Helper method for replacing the given tokens (as an anonymous object) in the given string</summary>
		public static string Replace(string text, object tokens) {
			return new Tokenizer().Render(text, ToDictionary(tokens));
		}

		/// <summary>Given an anonymous object, this returns a Dictionary of strings to objects</summary>	
		public static Dictionary<string, object> ToDictionary(object anonymousType) {
			if (anonymousType == null)                       return null;
			if (anonymousType is Dictionary<string, object>) return anonymousType as Dictionary<string, object>;
			var dict = new Dictionary<string, object>();
			if (anonymousType is Dictionary<string, string>) {
				foreach (var item in anonymousType as Dictionary<string, string>)
					dict[item.Key] = item.Value;
				return dict;
			}
			var attr = BindingFlags.Public | BindingFlags.Instance;
			foreach (var property in anonymousType.GetType().GetProperties(attr))
				if (property.CanRead)
					dict.Add(property.Name, property.GetValue(anonymousType, null));
			return dict;
		} 
	}
}
