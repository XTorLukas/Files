// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Files.App.Utils.RealTimeRM.Settings;

namespace Files.App.Utils.RealTimeRM.Base
{
	/// <inheritdoc/>
	public abstract partial class ResourceManagerBase : IResourceManager
	{
		private ResourceManagerOptions? _managerOptions = default;

		/// <summary>
		/// Gets the cached instance of <see cref="ResourceManagerOptions"/>, initializing it if necessary.
		/// </summary>
		protected ResourceManagerOptions EnsureManagerOptions => _managerOptions ??= new();

		/// <inheritdoc/>
		public IResourceManager AddOptions(Action<ResourceManagerOptions> options)
		{
			options(EnsureManagerOptions);
			return this;
		}
	}
}
