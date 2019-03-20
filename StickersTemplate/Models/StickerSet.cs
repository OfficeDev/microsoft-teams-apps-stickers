//----------------------------------------------------------------------------------------------
// <copyright file="StickerSet.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Model describing a set of <see cref="Sticker"/> object.
    /// </summary>
    public sealed class StickerSet : IEnumerable<Sticker>
    {
        private readonly IEnumerable<Sticker> stickers;

        /// <summary>
        /// Initializes a new instance of the <see cref="StickerSet"/> class.
        /// </summary>
        /// <param name="name">The name of the <see cref="StickerSet"/>.</param>
        /// <param name="stickers">The <see cref="Sticker"/> objects in the set.</param>
        public StickerSet(string name, IEnumerable<Sticker> stickers)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.stickers = stickers ?? throw new ArgumentNullException(nameof(stickers));
        }

        /// <summary>
        /// Gets the name of the <see cref="StickerSet"/>.
        /// </summary>
        public string Name { get; private set; }

        /// <inheritdoc />
        public IEnumerator<Sticker> GetEnumerator()
        {
            return this.stickers.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.stickers.GetEnumerator();
        }
    }
}
