// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

namespace Files.App.Extensions
{
	/// <summary>
	/// Extension method for Strings class to provide localization functionality.
	/// </summary>
	public static class StringsExtensions
	{
		private static IRealTimeCultureAwareManager ResourceManagerRTService => Ioc.Default.GetRequiredService<IResourceManager>().RealTimeService;

		/// <summary>
		/// Returns a localized string based on the provided key and optional arguments.
		/// </summary>
		/// <param name="key">The key for the localized string.</param>
		/// <param name="args">Optional arguments for formatting the localized string.</param>
		/// <returns>The localized string.</returns>
		public static string ToLocalized_NEW(this string key, List<object>? args = null)
		{
			return args == null
				? new Strings() { KeyValue = key }.ToLocalized()
				: new Strings() { KeyValue = key, ArgsValue = args }.ToLocalized();
		}

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

			_ = ResourceManagerRTService.UpdateDataValueProvider(dataValue);
		}
	}
}
