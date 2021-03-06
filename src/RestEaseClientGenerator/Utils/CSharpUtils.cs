﻿using System.CodeDom.Compiler;
using System.Text.RegularExpressions;
using RestEaseClientGenerator.Extensions;
using RestEaseClientGenerator.Types;

namespace RestEaseClientGenerator.Utils
{
    /// <summary>
    /// See also https://stackoverflow.com/questions/25139734/how-can-i-generate-a-safe-class-name-from-a-file-name
    /// </summary>
    internal static class CSharpUtils
    {
        private static readonly CodeDomProvider CodeProvider = CodeDomProvider.CreateProvider("C#");
        private static readonly Regex Regex = new Regex(@"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]");

        public static string CreateValidIdentifier(string identifier, CasingType casingType = CasingType.None)
        {
            string casedIdentifier;
            switch (casingType)
            {
                case CasingType.Camel:
                    casedIdentifier = identifier.ToCamelCase();
                    break;

                case CasingType.Pascal:
                    casedIdentifier = identifier.ToPascalCase();
                    break;

                default:
                    casedIdentifier = identifier;
                    break;
            }

            bool isValid = CodeProvider.IsValidIdentifier(casedIdentifier);

            if (!isValid)
            {
                // File name contains invalid chars, remove them
                casedIdentifier = Regex.Replace(casedIdentifier, string.Empty);

                // Class name doesn't begin with a letter, insert an underscore
                if (!char.IsLetter(casedIdentifier, 0))
                {
                    casedIdentifier = casedIdentifier.Insert(0, "_");
                }
            }

            return CodeProvider.CreateValidIdentifier(casedIdentifier.Replace(" ", string.Empty));
        }
    }
}