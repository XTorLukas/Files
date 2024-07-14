// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

namespace Files.App.Extensions
{
	/// <summary>
	/// Extension method for Strings class to provide localization functionality.
	/// </summary>
	public static class StringsExtensions
	{
		private static IResourceManager ResourceManagerService => Ioc.Default.GetRequiredService<IResourceManager>();

		public static string ToLocalized(this string key)
			=> ResourceManagerService.GetString(key.Replace('/', '_'));

		/// <summary>
		/// Updates the localization data for the provided Strings instance.
		/// </summary>
		/// <param name="dataValue">The Strings instance to update.</param>
		/// <param name="key">The key for the localized string.</param>
		/// <param name="args">Optional arguments for formatting the localized string.</param>
		public static void ToLocalized(this Strings dataValue, string key, List<object>? args = null)
		{
			if (dataValue is null || (dataValue.KeyValue == key &&
				((dataValue.IsArgsNull && args is null) ||
				(!dataValue.IsArgsNull && args is not null && dataValue.ArgsValue!.SequenceEqual(args)))))
				return;

			dataValue.KeyValue = key;
			dataValue.ArgsValue = args;

			_ = ResourceManagerService.RealTimeService.UpdateDataValueProvider(dataValue);
		}
	}
}
