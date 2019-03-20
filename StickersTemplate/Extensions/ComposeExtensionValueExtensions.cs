//----------------------------------------------------------------------------------------------
// <copyright file="ComposeExtensionValueExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Extensions
{
    using System.Linq;
    using StickersTemplate.WebModels;

    /// <summary>
    /// Extension methods for the <see cref="ComposeExtensionValue"/> class.
    /// </summary>
    public static class ComposeExtensionValueExtensions
    {
        /// <summary>
        /// Returns the first and only parameter value from the Compose Extesnion query value or null if one was not present.
        /// </summary>
        /// <param name="composeExtensionValue">The Compose Extension query value.</param>
        /// <returns>The parameter value if one was present. Null otherwise.</returns>
        public static string GetParameterValue(this ComposeExtensionValue composeExtensionValue)
        {
            if (composeExtensionValue?.Parameters != null && composeExtensionValue.Parameters.Any() && !"initialRun".Equals(composeExtensionValue.Parameters[0].Name))
            {
                return composeExtensionValue.Parameters[0].Value;
            }

            return null;
        }
    }
}
