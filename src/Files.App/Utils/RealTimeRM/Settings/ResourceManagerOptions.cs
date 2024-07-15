// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Files.App.Utils.RealTimeRM.Helpers;
using System.Globalization;
using System.Reflection;
using Windows.Globalization;

namespace Files.App.Utils.RealTimeRM.Settings
{
	/// <summary>
	/// Represents options for managing resources.
	/// </summary>
	public class ResourceManagerOptions : IDisposable
	{
		private bool _disposed = false;

		/// <summary>
		/// The index of the matched culture in the manifest.
		/// </summary>
		public int CultureIndex { get; private set; } = -1;

		/// <summary>
		/// The assembly where the resources are located.
		/// </summary>
		public Assembly? AssemblyData { get; set; } = default;

		private string? _parentPath = null;

		/// <summary>
		/// The parent path of the resource directory.
		/// </summary>
		public string ParentPath
		{
			get => _parentPath ?? string.Empty;
			set => _parentPath = value ?? string.Empty;
		}

		private string? _directoryName = null;

		/// <summary>
		/// The name of the resource directory.
		/// </summary>
		public string DirectoryName
		{
			get => _directoryName ?? string.Empty;
			set => _directoryName = value ?? string.Empty;
		}

		private string? _resourceName = null;

		/// <summary>
		/// The name of the resource file.
		/// </summary>
		public string ResourceName
		{
			get => _resourceName ?? string.Empty;
			set => _resourceName = value ?? string.Empty;
		}

		private string? _cultureName = null;

		/// <summary>
		/// The name of the culture.
		/// </summary>
		/// <remarks>
		/// Set to an empty string to use the <see cref="ApplicationLanguages.PrimaryLanguageOverride"/> culture.
		/// If not valid, defaults to <see cref="ResourceManagerHelpers.DefaultCultureName"/>.
		/// </remarks>
		public string CultureName
		{
			get
			{
				if (!IsValidated)
					ValidateCultureName();

				return _cultureName ?? string.Empty;
			}
			set
			{
				_cultureName = value is null
					? string.Empty
					: (value.Length == 0 ? ApplicationLanguages.PrimaryLanguageOverride : value)
					.Replace(ResourceManagerHelpers.LocaleSeparatorChar, ResourceManagerHelpers.CultureSeparatorChar);

				CultureIndex = -1;
				ValidateCultureName();
			}
		}

		/// <summary>
		/// Indicates whether the culture name has been validated and found in the manifest.
		/// </summary>
		public bool IsValidated
			=> CultureIndex > -1;

		/// <summary>
		/// Checks if the resource options are buildable.
		/// </summary>
		public bool IsBuildable
			=> _parentPath is not null
			&& _directoryName is not null
			&& _cultureName is not null
			&& _resourceName is not null;

		/// <summary>
		/// Checks if the resource options are for a custom path.
		/// </summary>
		public bool IsCustomPath
			=> AssemblyData is null && IsBuildable;

		/// <summary>
		/// Retrieves names of manifest resources from the associated assembly that meet specific criteria.
		/// </summary>
		/// <remarks>
		/// This property fetches names of manifest resources matching the specified directory and resource name criteria.
		/// If the associated assembly is null and buildable, it provides a default list of manifest languages.
		/// </remarks>
		public IEnumerable<string> ManifestCultureNames
		{
			get
			{
				yield return string.Empty; // Default

				if (!IsBuildable)
					yield break;

				if (IsCustomPath)
					foreach (var lang in ApplicationLanguages.ManifestLanguages)
						yield return lang;

				var resources = AssemblyData?.GetManifestResourceNames();
				if (resources == null)
					yield break;

				var parentPathSegments = _parentPath!.Split(ResourceManagerHelpers.ManifestSeparatorChar).Length;
				var directoryName = $"{_directoryName}{ResourceManagerHelpers.ManifestSeparatorChar}";

				foreach (var resource in resources)
				{
					if (resource.EndsWith(_resourceName!) && resource.Contains(directoryName))
					{
						var parts = resource.Split(ResourceManagerHelpers.ManifestSeparatorChar);
						if (parts.Length > parentPathSegments + 1)
						{
							var langName = parts[parentPathSegments + 1];
							yield return langName.Replace(ResourceManagerHelpers.LocaleSeparatorChar, ResourceManagerHelpers.CultureSeparatorChar);
						}
					}
				}
			}
		}

		public IList<string> SortedManifestCultureNames(out int sortedIndex)
		{
			var sortedManifestCultureNames = ManifestCultureNames.ToList();
			var selectedItem = sortedManifestCultureNames[CultureIndex];

			sortedManifestCultureNames = sortedManifestCultureNames
				.OrderBy(cultureName => cultureName is not "")
				.ThenBy(cultureName => new CultureInfo(cultureName).NativeName)
				.ToList();

			sortedIndex = sortedManifestCultureNames.IndexOf(selectedItem);
			return sortedManifestCultureNames;
		}

		/// <summary>
		/// The full path of the resource directory.
		/// </summary>
		public string DirectoryFullPath
		{
			get
			{
				if (!IsBuildable)
					return string.Empty;

				var maxLength = _parentPath!.Length + 1 + _directoryName!.Length;

				Span<char> pathBuffer = stackalloc char[maxLength];
				var pathLength = ResourceManagerHelpers
					.CombinePath(pathBuffer, _parentPath!, _directoryName!, IsCustomPath
					? SystemIO.Path.DirectorySeparatorChar
					: ResourceManagerHelpers.ManifestSeparatorChar);

				return new string(pathBuffer[..pathLength]);
			}
		}

		/// <summary>
		/// The full path of the resource file.
		/// </summary>
		public string ResourceFullPath
		{
			get
			{
				if (!IsBuildable)
					return string.Empty;

				var maxLength = DirectoryFullPath.Length + 1 + CultureName.Length + 1 + _resourceName!.Length;

				Span<char> pathBuffer = stackalloc char[maxLength];
				var directoryLength = ResourceManagerHelpers
					.CombinePath(pathBuffer, DirectoryFullPath, CultureName
					.Replace(ResourceManagerHelpers.CultureSeparatorChar, ResourceManagerHelpers.LocaleSeparatorChar), SystemIO.Path.DirectorySeparatorChar);

				directoryLength = ResourceManagerHelpers
					.CombinePath(pathBuffer, pathBuffer[..directoryLength], _resourceName, SystemIO.Path.DirectorySeparatorChar);

				if (!IsCustomPath)
					directoryLength = ResourceManagerHelpers
						.ReplacePathSeparator(pathBuffer[..directoryLength], ResourceManagerHelpers.ManifestSeparatorChar);

				return new string(pathBuffer[..directoryLength]);
			}
		}

		/// <summary>
		/// Validates the current culture name against the manifest resources.
		/// </summary>
		/// <param name="recursive">Indicates whether to perform recursive validation.</param>
		public void ValidateCultureName(bool recursive = true, int lastIndex = -1)
		{
			if (IsValidated)
				return;

			if (!IsBuildable)
			{
				CultureIndex = -1;
				return;
			}

			var index = 0;
			foreach (var item in ManifestCultureNames)
			{
				if (item.Equals(_cultureName, StringComparison.OrdinalIgnoreCase) ||
					item.StartsWith(_cultureName + ResourceManagerHelpers.CultureSeparatorChar, StringComparison.OrdinalIgnoreCase))
				{
					_cultureName = item;
					CultureIndex = lastIndex == 0 ? lastIndex : index;

					if (index == 0)
						break;

					ApplicationLanguages.PrimaryLanguageOverride = lastIndex == 0 ? string.Empty : _cultureName;
					return;
				}

				++index;
			}

			if (recursive && index == 0)
			{
				_cultureName = CultureInfo.InstalledUICulture.Name; // system culture name
				CultureIndex = -1;
				ValidateCultureName(false, index);
				return;
			}


			_cultureName = ResourceManagerHelpers.DefaultCultureName;
			ApplicationLanguages.PrimaryLanguageOverride = string.Empty;
		}

		public void SetApplicationCultureName(string cultureName = "")
		{
			cultureName ??= string.Empty;
			ApplicationLanguages.PrimaryLanguageOverride = cultureName;
			CultureName = cultureName;
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="ResourceManagerOptions"/> class.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				_disposed = true;

				CultureIndex = -1;
				AssemblyData = null;
				_parentPath = null;
				_directoryName = null;
				_resourceName = null;
				_cultureName = null;
			}
		}

		/// <summary>
		/// Releases all resources used by the <see cref="ResourceManagerOptions"/> class.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Finalizes an instance of the <see cref="ResourceManagerOptions"/> class.
		/// </summary>
		~ResourceManagerOptions() => Dispose(false);
	}
}
