// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Files.App.Utils.RealTimeRM.Base;
using Files.App.Utils.RealTimeRM.Exceptions;
using System.Collections.Frozen;
using System.Text;

namespace Files.App.Utils.RealTimeRM.Managers
{
	/// <summary>
	/// Manages the JSON resources for the application.
	/// </summary>
	public class JsonResourceManager : ResourceManagerBase
	{
		private new IDictionary<string, object>? _data;

		/// <inheritdoc/>
		public override async Task BuildAsync(CancellationToken token = default)
		{
			if (!EnsureManagerOptions.IsBuildable)
				throw new NotBuildableResourceException("No defined valid options for build");

			LoadResource(out var stream);
			using var reader = new SystemIO.StreamReader(stream, Encoding.UTF8);

			var jsonSerializerOptions = new JsonSerializerOptions
			{
				Converters = { new DictionaryObjectConverter() },
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};
			var options = jsonSerializerOptions;

			var deserializeData = await JsonSerializer.DeserializeAsync<IDictionary<string, object>>(reader.BaseStream, options, token).ConfigureAwait(false);
			_data = (deserializeData ?? new Dictionary<string, object>()).ToFrozenDictionary();

			UpdateCurrentCulture();
			SupportedLanguages = EnsureManagerOptions.ManifestCultureNames.ToList();
			EnsureManagerOptions.Dispose();
			UpdateResource(token);
		}

		/// <inheritdoc/>
		public override object? GetObject(string key)
		{
			if (_data == null)
				return null;

			var keySegments = key.Split('.');
			return GetValueFromPath(_data!, keySegments);
		}

		/// <inheritdoc/>
		public override string GetString(string key)
		{
			var value = GetObject(key);
			return value as string ?? string.Empty;
		}

		/// <inheritdoc/>
		public override HashSet<string> GetKeys(CancellationToken token)
		{
			var keys = new HashSet<string>();
			if (_data != null)
				GetKeysRecursive(_data, string.Empty, ref keys, token);
			return keys;
		}

		/// <summary>
		/// Recursively retrieves all keys from a nested dictionary.
		/// </summary>
		/// <param name="dict">The dictionary to retrieve keys from.</param>
		/// <param name="parentKey">The parent key for the current level of recursion.</param>
		/// <param name="keys">The set to store the retrieved keys.</param>
		/// <param name="token">The cancellation token to check for cancellation.</param>
		private static void GetKeysRecursive(in IDictionary<string, object> dict, in string parentKey, ref HashSet<string> keys, CancellationToken token)
		{
			foreach (var kvp in dict)
			{
				if (token.IsCancellationRequested)
				{
					keys.Clear();
					return;
				}

				var key = string.IsNullOrEmpty(parentKey) ? kvp.Key : $"{parentKey}.{kvp.Key}";

				if (kvp.Value is IDictionary<string, object> nestedDict)
					GetKeysRecursive(nestedDict, key, ref keys, token);
				else
					_ = keys.Add(key);
			}
		}

		/// <summary>
		/// Retrieves a value from the dictionary using a path of keys.
		/// </summary>
		/// <param name="data">The dictionary to retrieve the value from.</param>
		/// <param name="path">The path of keys to the desired value.</param>
		/// <returns>The value associated with the specified path, or null if not found.</returns>
		private static object? GetValueFromPath(IDictionary<string, object> data, in string[] path)
		{
			object? current = data;

			foreach (var part in path)
			{
				if (current is IDictionary<string, object> currentDict)
				{
					if (!currentDict.TryGetValue(part, out current))
						return null;
				}
				else
					return null;
			}

			return current;
		}
	}
}
