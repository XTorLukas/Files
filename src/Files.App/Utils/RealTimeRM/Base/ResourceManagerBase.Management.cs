// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

using Files.App.Utils.RealTimeRM.Exceptions;
using Microsoft.UI.Xaml;
using System.Collections.Frozen;
using System.Globalization;

namespace Files.App.Utils.RealTimeRM.Base
{
	/// <inheritdoc/>
	public abstract partial class ResourceManagerBase : IResourceManager
	{
		private IEnumerable<string>? _supportedLanguages = null;

		/// <inheritdoc/>
		public IEnumerable<string> SupportedLanguages
		{
			get => _supportedLanguages ??= [string.Empty];
			protected set => _supportedLanguages = value;
		}

		/// <inheritdoc/>
		public CultureInfo Culture => CultureInfo.CurrentUICulture;

		/// <inheritdoc/>
		public FlowDirection FlowDirection => Culture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;

		/// <summary>
		/// Dictionary containing resource data.
		/// </summary>
		protected FrozenDictionary<string, object>? _data = null;

		/// <summary>
		/// Loads the resource.
		/// </summary>
		/// <exception cref="NotFoundResourceException"/>
		protected void LoadResource(out SystemIO.Stream stream)
		{
			if (TryLoadData(out stream!))
				return;

			if (EnsureManagerOptions.CultureIndex == 0)
				goto ERR;

			EnsureManagerOptions.SetApplicationCultureName();

			if (TryLoadData(out stream!))
				return;

			ERR:
			throw new NotFoundResourceException($"Default culture '{EnsureManagerOptions.CultureName}' not found.");
		}

		/// <summary>
		/// Attempts to load data from the specified resource path.
		/// </summary>
		/// <returns>True if data loading was successful; otherwise, false.</returns>
		/// <exception cref="NotFoundResourceException"/>
		private bool TryLoadData(out SystemIO.Stream? stream)
		{
			if (EnsureManagerOptions.IsCustomPath)
			{
				if (!SystemIO.Path.Exists(EnsureManagerOptions.ResourceFullPath))
					throw new NotFoundResourceException($"Path resource '{EnsureManagerOptions.ResourceFullPath}' not found.");

				stream = new SystemIO.StreamReader(EnsureManagerOptions.ResourceFullPath).BaseStream;
				return stream != null;
			}

			var assembly = EnsureManagerOptions.AssemblyData;
			stream = assembly?.GetManifestResourceStream(EnsureManagerOptions.ResourceFullPath);
			return stream != null;
		}
	}
}
