// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

namespace Files.App.Resources
{
	/// <summary>
	/// Manages YAML resource files for localization and retrieves values based on keys.
	/// </summary>
	public static class ResourceManager
	{
		// The default locale used if no other locale is specified.
		private const string DefaultLocale = "en_US";

		// The default folder where resource files are stored.
		private const string DefaultFolder = "Strings";

		// The default filename of the resource file.
		private const string DefaultFilename = "Resources.yml";

		// The format string for the resource path.
		private static string _resourcePathFormat = string.Empty;

		/// <summary>
		/// The assembly containing the resource.
		/// </summary>
		public static Assembly AssemblyData { get; private set; } = Assembly.GetExecutingAssembly();

		/// <summary>
		/// The namespace of the resource.
		/// </summary>
		public static string Namespace { get; private set; } = typeof(ResourceManager).Namespace!;

		/// <summary>
		/// The folder containing the resource.
		/// </summary>
		public static string Folder { get; private set; } = DefaultFolder;

		/// <summary>
		/// The filename of the resource.
		/// </summary>
		public static string Filename { get; private set; } = DefaultFilename;

		// The current locale.
		private static string _locale = DefaultLocale;

		/// <summary>
		/// The current locale, with '-' replaced by '_'.
		/// </summary>
		public static string Locale
		{
			get => _locale;
			set => _locale = value.Replace('-', '_');
		}

		/// <summary>
		/// Indicates whether the resource manager is built.
		/// </summary>
		public static bool IsBuilded { get; private set; } = false;

		/// <summary>
		/// The resource data as a dictionary.
		/// </summary>
		public static IDictionary<string, object>? ResourceData { get; private set; }

		/// <summary>
		/// Builds the resource manager with specified parameters.
		/// </summary>
		/// <param name="assembly">The assembly containing the resource.</param>
		/// <param name="nameOfSpace">The namespace of the resource.</param>
		/// <param name="folder">The folder containing the resource.</param>
		/// <param name="filename">The filename of the resource.</param>
		public static void Build(Assembly assembly, string nameOfSpace, string folder = DefaultFolder, string filename = DefaultFilename)
		{
			IsBuilded = true;

			AssemblyData = assembly;
			Namespace = nameOfSpace;
			Folder = folder;
			Filename = filename;

			_resourcePathFormat = $"{Namespace}.{Folder}.{Locale}.{Filename}";

			if (!TryLoadResource(out var stream) || stream is null)
			{
				stream?.Dispose();
				throw new Exception($"Default resource '{DefaultLocale}' not found.");
			}

			using (stream)
			{
				var yaml = new DeserializerBuilder()
					.WithNamingConvention(CamelCaseNamingConvention.Instance)
					.Build();

				ResourceData = yaml.Deserialize<IDictionary<string, object>>(new StreamReader(stream));
			}
		}

		/// <summary>
		/// Builds the resource manager with default parameters.
		/// </summary>
		public static void Build() => Build(AssemblyData, Namespace);

		/// <summary>
		/// Sets the current culture locale.
		/// </summary>
		/// <param name="locale">The locale to set.</param>
		public static void SetCulture(string locale) => Locale = locale;

		/// <summary>
		/// Tries to load the resource stream for the current locale.
		/// </summary>
		/// <param name="stream">The output stream containing the resource.</param>
		/// <returns>True if the resource is loaded, otherwise false.</returns>
		private static bool TryLoadResource(out Stream? stream)
		{
			if (TryLoadData(Locale, out stream))
				return true;

			if (Locale == DefaultLocale)
				return false;

			Locale = DefaultLocale;
			return TryLoadData(DefaultLocale, out stream);
		}

		/// <summary>
		/// Tries to load the resource stream for a given locale.
		/// </summary>
		/// <param name="locale">The locale for which to load the resource.</param>
		/// <param name="stream">The output stream containing the resource.</param>
		/// <returns>True if the resource is loaded, otherwise false.</returns>
		private static bool TryLoadData(string locale, out Stream? stream)
		{
			var resourcePath = $"{Namespace}.{Folder}.{locale}.{Filename}";
			stream = AssemblyData.GetManifestResourceStream(resourcePath);
			return stream != null;
		}

		/// <summary>
		/// Retrieves a string value from the resource based on the specified key.
		/// </summary>
		/// <param name="key">The key to look up in the resource.</param>
		/// <returns>The string value associated with the key, or an empty string if not found.</returns>
		public static string GetString(string key)
		{
			var value = GetObject(key);
			return value as string ?? string.Empty;
		}

		/// <summary>
		/// Retrieves an object value from the resource based on the specified key.
		/// </summary>
		/// <param name="key">The key to look up in the resource.</param>
		/// <returns>The object value associated with the key, or null if not found.</returns>
		public static object? GetObject(string key)
		{
			if (!IsBuilded)
				Build();

			if (ResourceData == null)
				return null;

			var keys = key.Split('.');

			if (!ResourceData.TryGetValue(keys[0], out var value))
				return null;

			for (var i = 1; i < keys.Length; i++)
			{
				if (value is IDictionary<object, object> dict)
				{
					if (!dict.TryGetValue(keys[i], out value))
						return null;
				}
				else
					return null;
			}

			return value;
		}
	}
}