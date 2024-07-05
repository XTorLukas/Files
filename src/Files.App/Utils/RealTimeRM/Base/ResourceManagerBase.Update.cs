// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Files.App.Utils.RealTimeRM.Settings;
using System.Globalization;

namespace Files.App.Utils.RealTimeRM.Base
{
	/// <inheritdoc/>
	public abstract partial class ResourceManagerBase : IResourceManager
	{
		private bool _isCultureChanged = false;
		private bool _isFlowDirectionChanged = false;

		/// <inheritdoc/>
		public event ResourceManagerEventHandler? OnUpdateResource;

		/// <inheritdoc/>
		public event ResourceManagerEventHandler? OnCultureChanged;

		/// <inheritdoc/>
		public event ResourceManagerEventHandler? OnUpdateFlowDirectionChanged;

		/// <inheritdoc/>
		public void UpdateResource(CancellationToken token = default)
		{
			var args = new ResourceManagerEventArgs()
			{
				Culture = Culture,
				FlowDirection = FlowDirection,
				IsCultureChanged = _isCultureChanged,
				IsFlowDirectionChanged = _isFlowDirectionChanged,
			};

			if (token.IsCancellationRequested)
				return;

			if (_isCultureChanged)
			{
				_isCultureChanged = false;
				OnCultureChanged?.Invoke(this, args);
				UpdateAllDataValues(token);
			}

			if (_isFlowDirectionChanged)
			{
				_isFlowDirectionChanged = false;
				OnUpdateFlowDirectionChanged?.Invoke(this, args);

				UpdateAllWindows(token);
				UpdateAllElements(token);
			}

			OnUpdateResource?.Invoke(this, args);
		}

		/// <summary>
		/// Updates the current culture and raises the appropriate flags.
		/// </summary>
		protected void UpdateCurrentCulture()
		{
			var culture = new CultureInfo(EnsureManagerOptions.CultureName);
			var isCultureChanged = CultureInfo.CurrentCulture.Name != culture.Name;
			var isFlowDirectionChanged = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft != culture.TextInfo.IsRightToLeft;

			if (isCultureChanged)
			{
				_isCultureChanged = true;

				Thread.CurrentThread.CurrentCulture = culture;
				Thread.CurrentThread.CurrentUICulture = culture;
				CultureInfo.DefaultThreadCurrentCulture = culture;
				CultureInfo.DefaultThreadCurrentUICulture = culture;
				CultureInfo.CurrentCulture = culture;
				CultureInfo.CurrentUICulture = culture;
			}

			if (isFlowDirectionChanged)
				_isFlowDirectionChanged = true;
		}
	}
}
