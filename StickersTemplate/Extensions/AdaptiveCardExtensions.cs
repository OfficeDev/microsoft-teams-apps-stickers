//----------------------------------------------------------------------------------------------
// <copyright file="AdaptiveCardExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Extensions
{
    using System;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Extension methods for the <see cref="AdaptiveCard"/> class.
    /// </summary>
    public static class AdaptiveCardExtensions
    {
        /// <summary>
        /// Turns the card into an <see cref="Attachment"/>.
        /// </summary>
        /// <param name="card">The <see cref="AdaptiveCard"/> to convert.</param>
        /// <returns>An <see cref="Attachment"/>.</returns>
        public static Attachment ToAttachment(this AdaptiveCard card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }
    }
}
