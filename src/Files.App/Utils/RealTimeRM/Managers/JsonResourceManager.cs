// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Files.App.Utils.RealTimeRM.Base;
using System.Globalization;
using System.Text;

namespace Files.App.Utils.RealTimeRM.Managers
{
	/// <summary>
	/// Manages the JSON resources for the application.
	/// </summary>
	public class JsonResourceManager : ResourceManagerBase
	{
		private new IDictionary<string, string>? _data;

		/// <inheritdoc/>
		public override async Task BuildAsync(CancellationToken token)
		{
			if (!EnsureManagerOptions.IsBuildable)
				throw new NotBuildableResourceException("No defined valid options for build");

			LoadResource(out var stream);
			using var reader = new SystemIO.StreamReader(stream, Encoding.UTF8);

			_data = await ResourceManagerJsonParser.GetKeysAsync(reader, token).ConfigureAwait(false);

			UpdateCurrentCulture();
			SupportedLanguages = EnsureManagerOptions.SortedManifestCultureNames(out var index);
			CurrentCultureIndex = index;
			EnsureManagerOptions.Dispose();
			UpdateResource(token);
		}

		/// <inheritdoc/>
		public override object? GetObject(string key)
		{
			return _data is null
				? null
				: (_data.TryGetValue(key, out var value) ? value : null);
		}

		/// <inheritdoc/>
		public override string GetString(string key)
		{
			var value = GetObject(key);
			return value as string ?? string.Empty;
		}

		/// <inheritdoc/>
		public override IEnumerable<string> GetKeys(CancellationToken token)
			=> _data is null ? [] : _data.Keys.ToHashSet();
	}
}
