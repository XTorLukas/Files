// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace Files.App.Utils.RealTimeRM
{
	/// <summary>
	/// Interface for a real-time resource manager that handles dynamic updates to UI elements and windows.
	/// </summary>
	public interface IRealTimeCultureAwareManager
	{
		/// <summary>
		/// Adds a window object to be managed by the resource manager.
		/// </summary>
		/// <param name="window">The window to be managed.</param>
		/// <returns>The current instance of the resource manager.</returns>
		IRealTimeCultureAwareManager RegisterWindow(Window window);

		/// <summary>
		/// Adds a UI element to be managed by the resource manager.
		/// </summary>
		/// <param name="element">The UI element to be managed.</param>
		/// <returns>The current instance of the resource manager.</returns>
		IRealTimeCultureAwareManager RegisterUIElement(UIElement element);

		/// <summary>
		/// Registers a data value provider for real-time updates.
		/// </summary>
		/// <param name="service">The service providing the value target.</param>
		/// <param name="dataValue">The real-time data value.</param>
		/// <returns>The current instance of the resource manager.</returns>
		IRealTimeCultureAwareManager RegisterDataValueProvider(IProvideValueTarget service, IRealTimeDataValueManager dataValue);

		/// <summary>
		/// Updates the data value provider with the new data value.
		/// </summary>
		/// <param name="dataValue">The new real-time data value.</param>
		/// <returns>The current instance of the resource manager.</returns>
		IRealTimeCultureAwareManager UpdateDataValueProvider(IRealTimeDataValueManager dataValue);

		/// <summary>
		/// Unregisters a data value provider from the real-time culture aware manager.
		/// </summary>
		/// <param name="dataValue">The real-time data value to be unregistered.</param>
		/// <returns>The current instance of the resource manager.</returns>
		IRealTimeCultureAwareManager UnregisterDataValueProvider(IRealTimeDataValueManager dataValue);
	}
}

