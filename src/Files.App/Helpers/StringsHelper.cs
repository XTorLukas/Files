// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace Files.App.Helpers
{
	[MarkupExtensionReturnType(ReturnType = typeof(string))]
	public sealed partial class Strings : MarkupExtension, IRealTimeDataValueManager
	{
		public string KeyValue { get; set; } = string.Empty;

		private List<object>? _argsValue = null;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public List<object>? ArgsValue
		{
			get => _argsValue ??= [];
			set => _argsValue = value;
		}

		public bool IsArgsNull => _argsValue is null;

		public string ToLocalized()
		{
			return IsArgsNull
				? RealTimeResourceManager.Instance.GetString(KeyValue)
				: string.Format(RealTimeResourceManager.Instance.GetString(KeyValue), ArgsValue!.ToArray());
		}

		protected override string ProvideValue(IXamlServiceProvider serviceProvider)
		{
			if (RealTimeEnable && serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service)
				_ = RealTimeResourceManager.Instance.RealTimeService.RegisterDataValueProvider(service, this);

			return ToLocalized();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ProvideValueTargetProperty? RealTimeProperty { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public DependencyObject? RealTimeTarget { get; set; }

		public bool RealTimeEnable { get; set; } = true;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public Func<string> RealTimeValueProvider => ToLocalized;

		~Strings() => RealTimeResourceManager.Instance.RealTimeService.UnregisterDataValueProvider(this);
	}
}
