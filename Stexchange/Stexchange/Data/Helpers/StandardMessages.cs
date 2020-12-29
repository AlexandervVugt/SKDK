﻿using System;
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

        public static string NoMatch(string fieldName)
        {
            return $"De {fieldName} moet minstens 1 letter bevatten";
        }
    }
}
