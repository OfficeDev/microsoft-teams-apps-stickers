//----------------------------------------------------------------------------------------------
// <copyright file="StickerContentCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using StickersTemplate.Extensions;
    using StickersTemplate.Models;

    /// <summary>
    /// Card for the content of the <see cref="sticker"/> to return in the results for the Compose Extension query.
    /// </summary>
    public class StickerContentCard
    {
        private readonly Sticker sticker;

        /// <summary>
        /// Initializes a new instance of the <see cref="StickerContentCard"/> class.
        /// </summary>
        /// <param name="sticker">The <see cref="sticker"/> for this card.</param>
        public StickerContentCard(Sticker sticker)
        {
            this.sticker = sticker ?? throw new ArgumentNullException(nameof(sticker));
        }

        /// <summary>
        /// Turns the card into an <see cref="Attachment"/>.
        /// </summary>
        /// <returns>An <see cref="Attachment"/>.</returns>
        public Attachment ToAttachment()
        {
            var card = new AdaptiveCard
            {
                Speak = this.sticker.Name,
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveImage
                    {
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                        Url = this.sticker.ImageUri,
                        AltText = this.sticker.Name
                    }
                }
            };

            return card.ToAttachment();
        }
    }
}
