﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Stexchange.Controllers.Exceptions;

namespace Stexchange.Controllers
{
    public abstract class StexChangeController : Controller
    {
        public static class Cookies
        {
            public const string SessionToken = "SessionToken";
        }

        /// <summary>
        /// Dictionary that contains all currently active session tokens.
        /// Value Tuple contains user id and username.
        /// </summary>
        private static readonly Dictionary<long, Tuple<int, string>> sessions = new Dictionary<long, Tuple<int, string>>();

        /// <summary>
        /// Checks if the session exists (and is active).
        /// </summary>
        /// <param name="token">Session token</param>
        /// <returns>True iff the session token exists in the session dictionary</returns>
        public static bool SessionExists(long token)
        {
            return sessions.Keys.Contains(token);
        }

        /// <summary>
        /// Retrieves the data associated to the session token if it exists
        /// </summary>
        /// <param name="token">session token</param>
        /// <param name="data">variable to store retrieved data in</param>
        /// <returns>True iff a key, value pair exists for the session token.</returns>
        public static bool GetSessionData(long token, out Tuple<int, string> data)
        {
            return sessions.TryGetValue(token, out data);
        }

        /// <summary>
        /// Retrieves the id of the user that is logged in from the Request cookie.
        /// </summary>
        /// <exception cref="InvalidSessionException">If the user is not logged in.</exception>
        /// <returns>The id of the user.</returns>
        public int GetUserId()
        {
            Request.Cookies.TryGetValue(Cookies.SessionToken, out string cookieVal);
            long token = Convert.ToInt64(cookieVal ?? throw new InvalidSessionException("Cookie does not exist", false, null));
            if (!GetSessionData(token, out Tuple<int, string> session))
            {
                Response.Cookies.Delete(Cookies.SessionToken);
                throw new InvalidSessionException("Session does not exist", true, false);
            }
            return session.Item1;
        }
        
        /// <summary>
        /// Creates a key, value pair for the session in the session dictionary.
        /// If a user already has a session, the old session is deleted and a new one is created.
        /// </summary>
        /// <param name="user">The value of the session (user id and postal code)</param>
        /// <exception cref="ArgumentNullException">When user or postal code is null</exception>
        /// <excpetion cref="ArgumentException">When the user id or postal code are an invalid value.</excpetion>
        public static long CreateSession(Tuple<int, string> user)
        {
            if(user is null || string.IsNullOrWhiteSpace(user.Item2))
            {
                throw new ArgumentNullException();
            }
            if (user.Item1 < 0 || !CheckPostalCode(user.Item2))
            {
                throw new ArgumentException("exception thrown from createsession");
            }
            ClearSessions(user.Item1);
            long token = generateToken(user);
            sessions.Add(token, user);
            return token;
        }

        /// <summary>
        /// Removes the key, value pair associated with the token.
        /// </summary>
        /// <param name="token">session token</param>
        /// <returns>True iff the key was found and removed.</returns>
        public static bool TerminateSession(long token)
        {
            return sessions.Remove(token);
        }

        /// <summary>
        /// Removes the sessions that exist for the given user id.
        /// </summary>
        /// <param name="id">the user id</param>
        private static void ClearSessions(int id)
        {
            var tokens = (from value in sessions
                         where value.Value.Item1 == id
                         select value.Key).ToList();
            foreach(long token in tokens)
            {
                sessions.Remove(token);
            }
        }

        /// <summary>
        /// Checks if the input string is a valid postal code.
        /// </summary>
        /// <param name="postalCode"></param>
        /// <returns></returns>
        private static bool CheckPostalCode(string postalCode)
        {
            return postalCode.Length == 6 && new Regex(@"\d{4}[A-Z]{2}", RegexOptions.IgnoreCase).IsMatch(postalCode);
        }

        /// <summary>
        /// Generates a 64 bit token for a user.
        /// </summary>
        /// <param name="data">user data</param>
        /// <returns>64 bit unique token</returns>
        private static long generateToken(Tuple<int, string> data)
        {
            return data.GetHashCode() * DateTime.Now.Ticks;
        }
    }
}
