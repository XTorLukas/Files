// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Files.App.Utils.RealTimeRM.Managers;
using System.Reflection;

namespace Files.App.Extensions
{
	/// <summary>
	/// Extension methods for <see cref="IResourceManager"/>.
	/// </summary>
	public static class RealTimeResourceManager
	{
		private static IResourceManager? _instance = null;

		/// <summary>
		/// Provides a static instance of <see cref="IResourceManager"/>.
		/// </summary>
		public static IResourceManager Instance => _instance ??= new JsonResourceManager(); 

		/// <summary>
		/// Creates a new instance of <see cref="IResourceManager"/> with default settings.
		/// </summary>
		/// <param name="manager">The instance of <see cref="IResourceManager"/> to extend.</param>
		/// <returns>A new instance of <see cref="IResourceManager"/> with default settings.</returns>
		public static IResourceManager Create(this IResourceManager manager)
		{
			return manager
				.AddOptions(options =>
				{
					options.AssemblyData = Assembly.GetExecutingAssembly();
					options.ParentPath = "Files.App";
					options.DirectoryName = "Strings_Json";
					options.ResourceName = "Resources.json";
					options.CultureName = string.Empty;
				});
		}

		/// <summary>
		/// Creates a new instance of <see cref="IResourceManager"/> with custom settings.
		/// </summary>
		/// <param name="manager">The instance of <see cref="IResourceManager"/> to extend.</param>
		/// <param name="parentPath">The parent path for resource files.</param>
		/// <param name="resourceName">The name of the resource file.</param>
		/// <param name="directoryName">The directory name for resource files.</param>
		/// <returns>A new instance of <see cref="IResourceManager"/> with custom settings.</returns>
		public static IResourceManager Create(this IResourceManager manager, string parentPath, string resourceName, string directoryName)
		{
			return manager
				.AddOptions(options =>
				{
					options.ParentPath = parentPath;
					options.DirectoryName = directoryName;
					options.ResourceName = resourceName;
					options.CultureName = string.Empty;
				});
		}
	}
}
