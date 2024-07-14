// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using LightJson;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.IO;

namespace Files.App.Utils.RealTimeRM.Managers
{
	internal static class ResourceManagerJsonParser
	{
		internal static async Task<FrozenDictionary<string, string>> GetKeysAsync(StreamReader streamReader, CancellationToken token = default)
		{
			var dataText = await streamReader.ReadToEndAsync(token);
			if (string.IsNullOrEmpty(dataText))
				return FrozenDictionary<string, string>.Empty;

			var json = JsonValue.Deserialize(dataText);
			var result = new HashSet<(string key, string value)>();

			await Task.Run(() => ProcessJsonObject(json, string.Empty, result), token);
			return result.ToFrozenDictionary(data => data.key, data => data.value);
		}

		private static void ProcessJsonObject(JsonValue json, string prefix, ISet<(string key, string value)> result)
		{
			if (json.Type is not JsonValueType.Object)
				return;

			var obj = json.GetJsonObject();

			if (obj.TryGetValue("text", out var text) && obj.ContainsKey("crowdinContext"))
			{
				if (string.IsNullOrEmpty(prefix))
					return;

				_ = result.Add(new()
				{
					key = KeyNameValidator(prefix),
					value = text.GetString()
				});

				return;
			}

			foreach (var kvp in obj)
			{
				var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}_{kvp.Key}";

				switch (kvp.Value.Type)
				{
					case JsonValueType.Boolean:
					case JsonValueType.Number:
					case JsonValueType.String:
						_ = result.Add(new()
						{
							key = KeyNameValidator(key),
							value = kvp.Value.ToValueString() ?? string.Empty
						});
						break;

					case JsonValueType.Object:
						ProcessJsonObject(kvp.Value, key, result);
						break;

					default:
						break;
				}
			}
		}

		/// <summary>
		/// Validates and returns a valid C# identifier name for the given key.
		/// </summary>
		/// <param name="key">The key to validate.</param>
		/// <returns>A valid C# identifier based on the key.</returns>
		private static string KeyNameValidator(string key)
		{
			Span<char> resultSpan = key.Length <= 256 ? stackalloc char[key.Length] : new char[key.Length];
			var keySpan = key.AsSpan();

			for (var i = 0; i < keySpan.Length; i++)
			{
				resultSpan[i] = keySpan[i] switch
				{
					'+' => 'P',
					' ' or '.' => '_',
					_ => keySpan[i],
				};
			}

			return resultSpan.ToString();
		}
	}
}
