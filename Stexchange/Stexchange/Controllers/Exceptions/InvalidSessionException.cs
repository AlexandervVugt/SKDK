using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Controllers.Exceptions
{
    /// <summary>
    /// Exception to indicate that the login session of the user
    /// that made the request is not valid (anymore).
    /// </summary>
    public class InvalidSessionException : Exception
    {
        public bool? CookieExists { get; }
        public bool? SessionExists { get; }

        /// <summary>
        /// Overrides the base constructor.
        /// Usage is discouraged.
        /// </summary>
        public InvalidSessionException() : base() { }

        /// <summary>
        /// Overrides the base constructor.
        /// Usage is discouraged.
        /// </summary>
        /// <param name="message"></param>
        public InvalidSessionException(string message) : base(message) { }

        /// <summary>
        /// Overrides the base constructor.
        /// Usage is discouraged.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public InvalidSessionException(string message, Exception innerException) : base(message, innerException) { }

        public InvalidSessionException(string message, bool? cookieExists, bool? sessionExists) : base(message)
        {
            CookieExists = cookieExists;
            SessionExists = sessionExists;
        }

        public InvalidSessionException(string message, Exception innerException, bool? cookieExists, bool? sessionExists)
            : base(message, innerException)
        {
            CookieExists = cookieExists;
            SessionExists = sessionExists;
        }
    }
}
