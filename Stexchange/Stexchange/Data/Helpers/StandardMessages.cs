using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Data.Helpers
{
    public static class StandardMessages
    {
        /// <summary>
        /// oops, fieldname is a required field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string RequiredField(string fieldName)
        {
            fieldName = fieldName.First().ToString().ToUpper() + fieldName.Substring(1);
            return $"Oops! {fieldName} is een verplicht veld.";
        }

        /// <summary>
        /// the amount of chars is incorrect
        /// </summary>
        /// <returns></returns>
        public static string AmountOfCharacters()
        {
            return $"De hoeveelheid karakters is onjuist.";
        }

        /// <summary>
        /// capitalizes first char of input
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string CapitalizeFirst(string input)
        {
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        /// <summary>
        /// oops, something went wrong
        /// </summary>
        /// <returns></returns>
        public static string SomethingWW(string input)
        {
            if (String.IsNullOrEmpty(input))
            {
                return "Oops, something went wrong";
            }

            return $"Oops, something went wrong with the {input}";
        }
    }
}
