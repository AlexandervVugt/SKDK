using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stexchange.Data.Helpers
{
    public static class StandardMessages
    {
        /// <summary>
        /// fieldname is a required field
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string RequiredField(string fieldName)
        {
            fieldName = fieldName.First().ToString().ToUpper() + fieldName.Substring(1);
            return $"{fieldName} is een verplicht veld.";
        }

        /// <summary>
        /// the amount of chars is incorrect
        /// </summary>
        /// <returns></returns>
        public static string AmountOfCharacters(string fieldName)
        {
            return $"De hoeveelheid karakters in de {fieldName} veld is onjuist.";
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
                return "Er is iets misgegaan";
            }

            return $"Er is iets misgegaan met de {input}";
        }

        /// <summary>
        /// your input does not match the options given
        /// </summary>
        /// <returns></returns>
        public static string InvalidOptionUsed(string choice)
        {
            return $"{choice} keuze is geen geldige optie.";
        }

        /// <summary>
        /// can be implemented in the future together with containsprofanity
        /// </summary>
        /// <param name="badword"></param>
        /// <returns></returns>
        public static string IsNotAccepted(string badword)
        {
            return $"\'{StandardMessages.CapitalizeFirst(badword)}\' wordt niet geaccepteerd.";
        }

        /// <summary>
        /// $"De {fieldName} moet minstens 1 letter bevatten";
        /// </summary>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static string NoMatch(string fieldName)
        {
            return $"De {fieldName} moet minstens 1 letter bevatten";
        }

        public static string ValueBetween(string num1, string num2)
        {
            return $"De waarde moet tussen de {num1} en {num2} liggen.";
        }

        public static bool ContainsProfanity(string text, out string badword)
        {
            string readProfanity = System.IO.File.ReadAllText(@"wwwroot\txt\Profanity.txt");
            List<string> profanity = readProfanity.ToLower().Trim().Split(',').ToList();
            badword = "";

            foreach (string word in profanity)
            {
                if (text.Contains(word))
                {
                    badword = word;
                    return true;
                }
            }
            return false;
        }



    }
}
