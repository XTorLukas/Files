// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace Files.App.Helpers
{
	/// <summary>
	/// A MarkupExtension class that provides localized strings with real-time updates.
	/// </summary>
	[MarkupExtensionReturnType(ReturnType = typeof(string))]
	public sealed partial class Strings : MarkupExtension, IRealTimeDataValueManager
	{
		/// <summary>
		/// The key for the string resource in the resource file.
		/// </summary>
		public string KeyValue { get; set; } = string.Empty;

		private List<object>? _argsValue = null;

		/// <summary>
		/// Gets or sets the arguments for the string formatting.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public List<object>? ArgsValue
		{
			get => _argsValue ??= [];
			set => _argsValue = value;
		}

		/// <summary>
		/// Checks if ArgsValue is null.
		/// </summary>
		public bool IsArgsNull => _argsValue is null;

		/// <summary>
		/// Returns the localized string with or without formatting arguments.
		/// </summary>
		/// <returns>The localized string.</returns>
		public string ToLocalized()
		{
			return IsArgsNull
				? RealTimeResourceManager.Instance.GetString(KeyValue)
				: string.Format(RealTimeResourceManager.Instance.GetString(KeyValue), ArgsValue!.ToArray());
		}

		/// <summary>
		/// Provides the value for the markup extension.
		/// </summary>
		/// <param name="serviceProvider">The service provider.</param>
		/// <returns>The localized string.</returns>
		protected override string ProvideValue(IXamlServiceProvider serviceProvider)
		{
			if (RealTimeEnable && serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget service)
				_ = RealTimeResourceManager.Instance.RealTimeService.RegisterDataValueProvider(service, this);

			return ToLocalized();
		}

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ProvideValueTargetProperty? RealTimeProperty { get; set; }

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public DependencyObject? RealTimeTarget { get; set; }

		/// <inheritdoc/>
		public bool RealTimeEnable { get; set; } = true;

		/// <inheritdoc/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public Func<string> RealTimeValueProvider => ToLocalized;

		/// <summary>
		/// Unregisters the data value provider when the object is being garbage collected.
		/// </summary>
		~Strings() => RealTimeResourceManager.Instance.RealTimeService.UnregisterDataValueProvider(this);
	}
}
