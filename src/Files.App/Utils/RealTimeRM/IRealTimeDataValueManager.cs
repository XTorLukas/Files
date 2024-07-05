// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace Files.App.Utils.RealTimeRM
{
	/// <summary>
	/// Interface for managing real-time data values in the application.
	/// </summary>
	public interface IRealTimeDataValueManager
	{
		/// <summary>
		/// The property that will be updated with real-time data.
		/// </summary>
		ProvideValueTargetProperty? RealTimeProperty { get; set; }

		/// <summary>
		/// The target object that will be updated with real-time data.
		/// </summary>
		DependencyObject? RealTimeTarget { get; set; }

		/// <summary>
		/// A boolean indicating whether real-time updates are enabled.
		/// </summary>
		bool RealTimeEnable { get; set; }

		/// <summary>
		/// Function that provides the real-time value to be updated.
		/// </summary>
		/// <returns>A function that returns a string representing the real-time value.</returns>
		Func<string> RealTimeValueProvider { get; }
	}
}