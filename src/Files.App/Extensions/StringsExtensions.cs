// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

namespace Files.App.Extensions
{
	public static class StringsExtensions
	{
		private static readonly IResourceManager _manager = Ioc.Default.GetRequiredService<IResourceManager>();

		public static string ToLocalized_NEW(this string key, List<object>? args = null)
		{
			return args == null
				? new Strings() { KeyValue = key }.ToLocalized()
				: new Strings() { KeyValue = key, ArgsValue = args }.ToLocalized();
		}

		public static void ToLocalized(this Strings dataValue, string key, List<object>? args = null)
		{
			if (dataValue is null || (dataValue.KeyValue == key &&
				((dataValue.IsArgsNull && args is null) ||
				(!dataValue.IsArgsNull && args is not null && dataValue.ArgsValue!.SequenceEqual(args)))))
				return;

			dataValue.KeyValue = key;
			dataValue.ArgsValue = args;

			_ = _manager.RealTimeService.UpdateDataValueProvider(dataValue);
		}
	}
}
