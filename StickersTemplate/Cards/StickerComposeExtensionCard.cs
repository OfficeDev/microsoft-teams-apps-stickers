//----------------------------------------------------------------------------------------------
// <copyright file="StickerComposeExtensionCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Cards
{
    using System;
    using Microsoft.Bot.Schema;
    using StickersTemplate.Extensions;
    using StickersTemplate.Models;

    /// <summary>
    /// Card to <see cref="sticker"/> return in the results for the Compose Extension query.
    /// </summary>
    public class StickerComposeExtensionCard
    {
        private readonly Sticker sticker;

        /// <summary>
        /// Initializes a new instance of the <see cref="StickerComposeExtensionCard"/> class.
        /// </summary>
        /// <param name="sticker">The <see cref="sticker"/> for this card.</param>
        public StickerComposeExtensionCard(Sticker sticker)
        {
            this.sticker = sticker ?? throw new ArgumentNullException(nameof(sticker));
        }

        /// <summary>
        /// Turns the card into an <see cref="Attachment"/>.
        /// </summary>
        /// <returns>An <see cref="Attachment"/>.</returns>
        public Attachment ToAttachment()
        {
            var content = new StickerContentCard(this.sticker);
            var preview = new StickerPreviewCard(this.sticker);

            return content.ToAttachment().ToComposeExtensionResult(preview.ToAttachment());
        }
    }
}
