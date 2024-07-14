// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

namespace Files.App.Utils.RealTimeRM.Base
{
	/// <summary>
	/// Base class for managing resources.
	/// </summary>
	public abstract partial class ResourceManagerBase : IResourceManager
	{
		/// <inheritdoc/>
		public abstract Task BuildAsync(CancellationToken token);

		/// <inheritdoc/>
		public abstract object? GetObject(string key);

		/// <inheritdoc/>
		public abstract string GetString(string key);

		/// <inheritdoc/>
		public abstract IEnumerable<string> GetKeys(CancellationToken token);
	}
}
