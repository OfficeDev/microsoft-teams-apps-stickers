//----------------------------------------------------------------------------------------------
// <copyright file="StickerPreviewCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Cards
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Bot.Schema;
    using StickersTemplate.Models;

    /// <summary>
    /// Card for the preview of the <see cref="sticker"/> to return in the results for the Compose Extension query.
    /// </summary>
    public class StickerPreviewCard
    {
        private readonly Sticker sticker;

        /// <summary>
        /// Initializes a new instance of the <see cref="StickerPreviewCard"/> class.
        /// </summary>
        /// <param name="sticker">The <see cref="sticker"/> for this card.</param>
        public StickerPreviewCard(Sticker sticker)
        {
            this.sticker = sticker ?? throw new ArgumentNullException(nameof(sticker));
        }

        /// <summary>
        /// Turns the card into an <see cref="Attachment"/>.
        /// </summary>
        /// <returns>An <see cref="Attachment"/>.</returns>
        public Attachment ToAttachment()
        {
            var card = new ThumbnailCard
            {
                Title = this.sticker.Name,
                Images = new List<CardImage>()
                {
                    new CardImage
                    {
                        Alt = this.sticker.Name,
                        Url = this.sticker.ImageUri.ToString()
                    }
                }
            };

            return card.ToAttachment();
        }
    }
}
