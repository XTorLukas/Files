// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Microsoft.UI.Xaml;
using System.Globalization;

namespace Files.App.Utils.RealTimeRM.Settings
{
	/// <summary>
	/// Event arguments for resource manager events.
	/// </summary>
	public class ResourceManagerEventArgs : EventArgs
	{
		/// <summary>
		/// The culture information associated with the event.
		/// </summary>
		public required CultureInfo Culture;

		/// <summary>
		/// The flow direction associated with the event.
		/// </summary>
		public required FlowDirection FlowDirection;

		/// <summary>
		/// Value indicating whether the culture has changed.
		/// </summary>
		public required bool IsCultureChanged;

		/// <summary>
		/// Value indicating whether the flow direction has changed.
		/// </summary>
		public required bool IsFlowDirectionChanged;
	}

	/// <summary>
	/// Delegate for resource manager events.
	/// </summary>
	/// <param name="sender">The source of the event, which is the resource manager.</param>
	/// <param name="e">The event arguments containing information about the event.</param>
	public delegate void ResourceManagerEventHandler(IResourceManager sender, ResourceManagerEventArgs e);
}
