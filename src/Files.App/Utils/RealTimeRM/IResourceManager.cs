// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Files.App.Utils.RealTimeRM.Settings;
using Microsoft.UI.Xaml;
using System.Globalization;

namespace Files.App.Utils.RealTimeRM
{
	/// <summary>
	/// Interface for managing resources, including localization and UI updates.
	/// </summary>
	public interface IResourceManager
	{
		/// <summary>
		/// Event triggered when a resource is updated.
		/// </summary>
		event ResourceManagerEventHandler? OnUpdateResource;

		/// <summary>
		/// Event triggered when the culture changes.
		/// </summary>
		event ResourceManagerEventHandler? OnCultureChanged;

		/// <summary>
		/// Event triggered when the flow direction changes.
		/// </summary>
		event ResourceManagerEventHandler? OnUpdateFlowDirectionChanged;

		/// <summary>
		/// Gets the list of supported languages.
		/// </summary>
		IEnumerable<string> SupportedLanguages { get; }

		/// <summary>
		/// Gets the culture of the preferred language.
		/// </summary>
		CultureInfo Culture { get; }

		/// <summary>
		/// Gets the flow direction based on the text direction of the culture.
		/// </summary>
		FlowDirection FlowDirection { get; }

		/// <summary>
		/// Builds the resource manager by loading and deserializing the JSON data asynchronously.
		/// </summary>
		/// <param name="token">A cancellation token to observe while waiting for the task to complete.</param>
		/// <exception cref="NotBuildableResourceException"/>
		/// <exception cref="NotFoundResourceException"/>
		Task BuildAsync(CancellationToken token = default);

		/// <summary>
		/// Retrieves an object from the resource manager using a specified key.
		/// </summary>
		/// <param name="key">The key of the resource to retrieve.</param>
		/// <returns>The resource object associated with the specified key.</returns>
		object? GetObject(string key);

		/// <summary>
		/// Retrieves a string from the resource manager using a specified key.
		/// </summary>
		/// <param name="key">The key of the resource to retrieve.</param>
		/// <returns>The resource string associated with the specified key.</returns>
		string GetString(string key);

		/// <summary>
		/// Retrieves a set of all keys present in the resource manager.
		/// </summary>
		/// <param name="token">A cancellation token to observe while waiting for the task to complete.</param>
		/// <returns>
		/// An <see cref="IEnumerable{T}"/> of strings representing all keys present in the resource manager.
		/// </returns>
		IEnumerable<string> GetKeys(CancellationToken token = default);

		/// <summary>
		/// Updates the resources and raises the appropriate events.
		/// </summary>
		void UpdateResource(CancellationToken token = default);

		/// <summary>
		/// Adds options to the resource manager.
		/// </summary>
		/// <param name="options">An action that configures the resource manager options.</param>
		/// <returns>The current instance of the resource manager with the added options.</returns>
		IResourceManager AddOptions(Action<ResourceManagerOptions> options);

		/// <summary>
		/// Gets the real-time resource manager service for handling dynamic UI updates.
		/// </summary>
		IRealTimeCultureAwareManager RealTimeService { get; }
	}
}
