// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
using WinRT.Interop;

namespace Files.App.Utils.RealTimeRM.Base
{
	/// <inheritdoc/>
	public abstract partial class ResourceManagerBase : IRealTimeCultureAwareManager
	{
		/// <summary>
		/// List of registered windows managed by the resource manager.
		/// </summary>
		private List<Window>? _registeredWindows;

		/// <summary>
		/// List of registered framework elements managed by the resource manager.
		/// </summary>
		private List<FrameworkElement>? _registeredElements;

		private List<IRealTimeDataValueManager>? _registeredDataValues;

		/// <inheritdoc/>
		public IRealTimeCultureAwareManager RealTimeService => this;

		/// <inheritdoc/>
		public IRealTimeCultureAwareManager RegisterUIElement(UIElement element)
		{
			if (element is FrameworkElement frameworkElement)
			{
				_registeredElements ??= [];
				_registeredElements.Add(frameworkElement);
				frameworkElement.Unloaded += Element_Unloaded;
				UpdateSingleElement(frameworkElement);
			}

			return this;
		}

		/// <inheritdoc/>
		public IRealTimeCultureAwareManager RegisterWindow(Window window)
		{
			_registeredWindows ??= [];
			_registeredWindows.Add(window);
			window.Closed += Window_Closed;
			UpdateSingleWindow(window);
			return this;
		}

		/// <inheritdoc/>
		public IRealTimeCultureAwareManager RegisterDataValueProvider(IProvideValueTarget service, IRealTimeDataValueManager dataValue)
		{
			if (service.TargetProperty is ProvideValueTargetProperty property &&
				service.TargetObject is FrameworkElement target &&
				property.DeclaringType.GetProperty(property.Name) is not null)
			{
				target.Loaded += (_, _) => DataValue_Load(dataValue);
				target.Unloaded += (_, _) => DataValue_Unload(dataValue);
				target.DataContext = dataValue;

				dataValue.RealTimeTarget = target;
				dataValue.RealTimeProperty = property;
			}

			return this;
		}

		/// <inheritdoc/>
		public IRealTimeCultureAwareManager UpdateDataValueProvider(IRealTimeDataValueManager dataValue)
		{
			UpdateSingleDataValue(dataValue);
			return this;
		}

		/// <inheritdoc/>
		public IRealTimeCultureAwareManager UnregisterDataValueProvider(IRealTimeDataValueManager dataValue)
		{
			if (dataValue.RealTimeTarget is FrameworkElement target)
			{
				target.Loaded -= (_, _) => DataValue_Load(dataValue);
				target.Unloaded -= (_, _) => DataValue_Unload(dataValue);
			}

			if (_registeredDataValues?.Find(data => ReferenceEquals(data, dataValue)) is not null)
				_ = _registeredDataValues.Remove(dataValue);

			return this;
		}

		/// <summary>
		/// Updates the title bar layout of the specified window based on the current culture.
		/// </summary>
		/// <param name="window">The window to be updated.</param>
		/// <returns>True if the update was successful; otherwise, false.</returns>
		protected bool UpdateTitleBar(Window window)
		{
			try
			{
				var hwnd = WindowNative.GetWindowHandle(window);
				var exStyle = Win32PInvoke.GetWindowLongPtr(hwnd, Win32PInvoke.GWL_EXSTYLE);

				exStyle = Culture.TextInfo.IsRightToLeft
					? new IntPtr(exStyle.ToInt64() | Win32PInvoke.WS_EX_LAYOUTRTL) // Set RTL layout
					: new IntPtr(exStyle.ToInt64() & ~Win32PInvoke.WS_EX_LAYOUTRTL); // Set LTR layout

				if (Win32PInvoke.SetWindowLongPtr(hwnd, Win32PInvoke.GWL_EXSTYLE, exStyle) == 0)
					return false;
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Updates the FlowDirection of all registered elements.
		/// </summary>
		/// /// <param name="token">A cancellation token that can be used to cancel the operation.</param>
		protected void UpdateAllElements(CancellationToken token = default)
		{
			if (_registeredElements is null)
				return;

			foreach (var element in _registeredElements)
			{
				if (token.IsCancellationRequested)
					break;

				UpdateSingleElement(element);
			}
		}

		/// <summary>
		/// Updates the title bars of all registered windows.
		/// </summary>
		/// <param name="token">A cancellation token that can be used to cancel the operation.</param>
		protected void UpdateAllWindows(CancellationToken token = default)
		{
			if (_registeredWindows is null)
				return;

			foreach (var window in _registeredWindows)
			{
				if (token.IsCancellationRequested)
					break;

				UpdateSingleWindow(window);
			}
		}

		/// <summary>
		/// Updates the value of all registered data values.
		/// </summary>
		/// <param name="token">A cancellation token that can be used to cancel the operation.</param>
		protected void UpdateAllDataValues(CancellationToken token = default)
		{
			if (_registeredDataValues is null)
				return;

			foreach (var dataValue in _registeredDataValues)
			{
				if (token.IsCancellationRequested)
					break;

				UpdateSingleDataValue(dataValue);
			}
		}

		/// <summary>
		/// Updates the FlowDirection of a single registered element.
		/// </summary>
		/// <param name="element">The FrameworkElement to be updated.</param>
		private void UpdateSingleElement(FrameworkElement element)
			=> element.FlowDirection = FlowDirection;

		/// <summary>
		/// Updates the title bar layout of a single registered window.
		/// </summary>
		/// <param name="window">The Window to be updated.</param>
		private void UpdateSingleWindow(Window window)
			=> _ = UpdateTitleBar(window);

		/// <summary>
		/// Updates the value of a single registered data value.
		/// </summary>
		/// <param name="dataValue">The data value manager that provides the value and target property.</param>
		private void UpdateSingleDataValue(IRealTimeDataValueManager dataValue)
		{
			if (dataValue.RealTimeTarget is null || dataValue.RealTimeProperty is null)
				return;

			dataValue.RealTimeProperty.DeclaringType.GetProperty(dataValue.RealTimeProperty.Name)?.SetValue(dataValue.RealTimeTarget, dataValue.RealTimeValueProvider());
		}

		/// <summary>
		/// Handles the Window.Closed event to remove the closed window from the list of registered windows.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="args">Event arguments associated with the Window.Closed event.</param>
		private void Window_Closed(object sender, WindowEventArgs args)
		{
			if (sender is Window window && _registeredWindows != null)
				_ = _registeredWindows.Remove(window);
		}

		/// <summary>
		/// Handles the FrameworkElement.Unloaded event to remove the unloaded element from the list of registered elements.
		/// </summary>
		/// <param name="sender">The object that triggered the event.</param>
		/// <param name="e">Event arguments associated with the FrameworkElement.Unloaded event.</param>
		private void Element_Unloaded(object sender, RoutedEventArgs e)
		{
			if (sender is FrameworkElement element && _registeredElements != null)
				_ = _registeredElements.Remove(element);
		}

		/// <summary>
		/// Handles the data value load event. Adds the data value to the list of registered data values and updates the value.
		/// </summary>
		/// <param name="dataValue">The data value manager that provides the value and target property.</param>
		private void DataValue_Load(IRealTimeDataValueManager dataValue)
		{
			_registeredDataValues ??= [];
			_registeredDataValues.Add(dataValue);
			UpdateSingleDataValue(dataValue);
		}

		/// <summary>
		/// Handles the data value unload event. Removes the data value from the list of registered data values.
		/// </summary>
		/// <param name="dataValue">The data value manager that provides the value and target property.</param>
		private void DataValue_Unload(IRealTimeDataValueManager dataValue) => _registeredDataValues?.Remove(dataValue);
	}
}
