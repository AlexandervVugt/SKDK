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

        /// <summary>
        /// can be implemented in the future together with containsprofanity
        /// </summary>
        /// <param name="badword"></param>
        /// <returns></returns>
        private static string IsNotAccepted(string badword)
        {
            return $"\'{StandardMessages.CapitalizeFirst(badword)}\' wordt niet geaccepteerd.";
        }

        public static string RewriteTextPlease()
        {
            return $"Sommige woorden worden niet geaccepteerd. Herschrijf uw tekst";
        }

        /// <summary>
        /// returns true if text contains profanity.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool ContainsProfanity(string text)
        {
            string readProfanity = System.IO.File.ReadAllText(@"Data\Helpers\TextFiles\Profanity.txt");
            List<string> profanity = readProfanity.ToLower().Split(',').ToList();

            foreach (string word in profanity)
            {
                if (word.Contains(text))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
