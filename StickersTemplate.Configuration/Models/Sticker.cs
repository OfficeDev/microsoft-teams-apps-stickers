//----------------------------------------------------------------------------------------------
// <copyright file="Sticker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Configuration.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Sticker
    /// </summary>
    public class Sticker
    {
        /// <summary>
        /// Maximum dimension of a sticker in pixels
        /// </summary>
        public const int MaximumDimensionInPixels = 640;

        /// <summary>
        /// Gets or sets Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets ImageUri
        /// </summary>
        public Uri ImageUri { get; set; }

        /// <summary>
        /// Gets or sets Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Keywords
        /// </summary>
        public IList<string> Keywords { get; set; }

        /// <summary>
        /// Gets or sets Index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets State
        /// </summary>
        public StickerState State { get; set; }
    }
}