//----------------------------------------------------------------------------------------------
// <copyright file="ActivityExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Extensions
{
    using System;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Extension methods for the <see cref="Activity"/> class.
    /// </summary>
    public static class ActivityExtensions
    {
        /// <summary>
        /// Determines if this <see cref="Activity"/> describes a Compose Extension query or not.
        /// </summary>
        /// <param name="activity">The <see cref="Activity"/>.</param>
        /// <returns>True if this <see cref="Activity"/> is a Compose Extension query. False otherwise.</returns>
        public static bool IsComposeExtensionQuery(this Activity activity)
        {
            return activity.Type == ActivityTypes.Invoke && "composeExtension/query".Equals(activity.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
