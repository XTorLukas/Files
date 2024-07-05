// Copyright (c) 2024 Files Community
// Licensed under the MIT License. See the LICENSE.

namespace Files.App.Utils.RealTimeRM.Exceptions
{
	/// <summary>
	/// Exception that is thrown when a requested resource is not found.
	/// </summary>
	public class NotFoundResourceException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="NotFoundResourceException"/> class.
		/// </summary>
		public NotFoundResourceException() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="NotFoundResourceException"/> class with a specified error message.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public NotFoundResourceException(string message) : base(message) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="NotFoundResourceException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="inner">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
		public NotFoundResourceException(string message, Exception inner) : base(message, inner) { }
	}
}
