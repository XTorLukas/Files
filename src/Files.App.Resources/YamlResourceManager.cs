// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

namespace Files.App.Resources
{
	/// <summary>
	/// Manages YAML resource files for localization and retrieves values based on keys.
	/// </summary>
	public static class ResourceManager
	{
		// Default locale used if no other locale is specified.
		private const string DefaultLocale = "en_US";

		// Default folder where resource files are stored.
		private const string DefaultFolder = "Strings";

		// Default filename of the resource file.
		private const string DefaultFilename = "Resources.yml";

		/// <summary>
		/// Assembly containing the resource.
		/// </summary>
		public static Assembly AssemblyData { get; private set; } = Assembly.GetExecutingAssembly();

		/// <summary>
		/// Namespace of the resource.
		/// </summary>
		public static string Namespace { get; private set; } = typeof(ResourceManager).Namespace!;

		/// <summary>
		/// Folder containing the resource.
		/// </summary>
		public static string Folder { get; private set; } = DefaultFolder;

		/// <summary>
		/// Filename of the resource.
		/// </summary>
		public static string Filename { get; private set; } = DefaultFilename;

		// Current locale.
		private static string _locale = DefaultLocale;

		// Indicates whether caching is enabled.
		private static bool _isCacheEnabled;

		// Cache for storing retrieved values.
		private static ConcurrentDictionary<string, object>? _cache;

		/// <summary>
		/// Current locale, with '-' replaced by '_'.
		/// </summary>
		public static string Locale
		{
			get => _locale;
			set => _locale = value.Replace('-', '_');
		}

		/// <summary>
		/// Indicates whether the resource manager is built.
		/// </summary>
		public static bool IsBuilt { get; private set; } = false;

		/// <summary>
		/// Resource data as a dictionary.
		/// </summary>
		public static FrozenDictionary<string, object>? ResourceData { get; private set; }

		/// <summary>
		/// Builds the resource manager with specified parameters.
		/// </summary>
		/// <param name="assembly">Assembly containing the resource.</param>
		/// <param name="nameOfSpace">Namespace of the resource.</param>
		/// <param name="folder">Folder containing the resource.</param>
		/// <param name="filename">Filename of the resource.</param>
		/// <param name="cache">Indicates whether caching should be enabled.</param>
		public static async Task BuildAsync(Assembly assembly, string nameOfSpace, string folder = DefaultFolder, string filename = DefaultFilename, bool cache = true)
		{
			IsBuilt = true;

			AssemblyData = assembly;
			Namespace = nameOfSpace;
			Folder = folder;
			Filename = filename;

			using (var stream = await TryLoadResourceAsync())
			{
				if (stream is null)
					throw new Exception($"Default resource '{DefaultLocale}' not found.");

				var yaml = new DeserializerBuilder()
				   .WithNamingConvention(CamelCaseNamingConvention.Instance)
				   .Build();

				ResourceData = yaml.Deserialize<IDictionary<string, object>>(new StreamReader(stream, Encoding.UTF8)).ToFrozenDictionary();
			}

			if (_cache is not null)
				DisableCache();

			if (cache)
				EnableCache();
		}

		/// <summary>
		/// Builds the resource manager with default parameters.
		/// </summary>
		public static Task BuildAsync() => BuildAsync(AssemblyData, Namespace);

		/// <summary>
		/// Sets the current culture locale.
		/// </summary>
		/// <param name="locale">Locale to set.</param>
		public static void SetCulture(string locale) => Locale = locale;

		/// <summary>
		/// Enables caching for the resource manager.
		/// </summary>
		public static void EnableCache()
		{
			_isCacheEnabled = true;
			_cache = new();
		}

		/// <summary>
		/// Disables caching for the resource manager.
		/// </summary>
		public static void DisableCache()
		{
			_isCacheEnabled = false;
			_cache = null;
		}

		/// <summary>
		/// Tries to load the resource stream for the current locale.
		/// </summary>
		/// <returns>Stream containing the resource if loaded, otherwise null.</returns>
		private static async Task<Stream?> TryLoadResourceAsync()
		{
			if (await TryLoadDataAsync())
				return AssemblyData.GetManifestResourceStream($"{Namespace}.{Folder}.{Locale}.{Filename}");

			if (Locale == DefaultLocale)
				return null;

			Locale = DefaultLocale;
			return await TryLoadDataAsync() ? AssemblyData.GetManifestResourceStream($"{Namespace}.{Folder}.{DefaultLocale}.{Filename}") : null;
		}

		/// <summary>
		/// Tries to load the resource stream for a given locale.
		/// </summary>
		/// <returns>True if the resource is loaded, otherwise false.</returns>
		private static async Task<bool> TryLoadDataAsync()
		{
			var resourcePath = $"{Namespace}.{Folder}.{Locale}.{Filename}";
			return await Task.Run(() => AssemblyData.GetManifestResourceStream(resourcePath) != null);
		}

		/// <summary>
		/// Retrieves a string value from the resource based on the specified key.
		/// </summary>
		/// <param name="key">Key to look up in the resource.</param>
		/// <returns>String value associated with the key, or an empty string if not found.</returns>
		public static string GetString(string key)
		{
			var value = GetObject(key);
			return value as string ?? string.Empty;
		}

		/// <summary>
		/// Retrieves an object value from the resource based on the specified key.
		/// </summary>
		/// <param name="key">Key to look up in the resource.</param>
		/// <returns>Object value associated with the key, or null if not found.</returns>
		public static object? GetObject(string key)
		{
			if (!IsBuilt)
				BuildAsync().Wait();

			if (ResourceData == null)
				return null;

			if (_isCacheEnabled && _cache is not null && _cache.TryGetValue(key, out var cachedValue))
				return cachedValue;

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

			if (_isCacheEnabled && _cache is not null)
				_cache[key] = value;

			return value;
		}

		/// <summary>
		/// Retrieves all keys from the ResourceData dictionary.
		/// </summary>
		/// <returns>A list of all keys in the ResourceData dictionary.</returns>
		public static async Task<FrozenSet<string>> GetKeysAsync()
		{
			if (!IsBuilt)
				BuildAsync().Wait();

			if (ResourceData == null)
				return new HashSet<string>().ToFrozenSet();

			var keys = new HashSet<string>();
			await Task.Run(() => GetKeysRecursive(ResourceData, string.Empty, keys));
			return keys.ToFrozenSet();
		}

		/// <summary>
		/// Recursively retrieves keys from a nested dictionary and adds them to the keys list.
		/// </summary>
		/// <param name="dict">Current dictionary to process.</param>
		/// <param name="parentKey">Parent key prefix for nested keys.</param>
		/// <param name="keys">List to store the keys.</param>
		private static void GetKeysRecursive(IDictionary<string, object> dict, string parentKey, HashSet<string> keys)
		{
			foreach (var kvp in dict)
			{
				var key = string.IsNullOrEmpty(parentKey) ? kvp.Key : $"{parentKey}.{kvp.Key}";

				if (kvp.Value is not IDictionary<object, object> nestedDict)
				{
					keys.Add(key);
					continue;
				}

				var dictTemp = nestedDict.ToDictionary(
					nestedKvp => nestedKvp.Key.ToString() ?? string.Empty,
					nestedKvp => nestedKvp.Value
				);

				if (dictTemp != null)
					GetKeysRecursive(dictTemp, key, keys);
			}
		}
	}
}
