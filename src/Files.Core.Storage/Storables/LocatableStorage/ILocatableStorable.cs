﻿// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

namespace Files.Core.Storage.Storables
{
	/// <summary>
	/// Represents a file or folder that resides within a folder structure.
	/// </summary>
	public interface ILocatableStorable : IStorable
	{
		/// <summary>
		/// Gets the path where the item resides.
		/// </summary>
		string Path { get; }
	}
}
