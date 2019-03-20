//----------------------------------------------------------------------------------------------
// <copyright file="Sticker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Models
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Model describing the <see cref="Sticker"/> object.
    /// </summary>
    public sealed class Sticker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Sticker"/> class.
        /// </summary>
        /// <param name="name">The name of the <see cref="Sticker"/>.</param>
        /// <param name="imageUri">The image <see cref="Uri"/> of the <see cref="Sticker"/>.</param>
        /// <param name="keywords">The keywords associated with the <see cref="Sticker"/>.</param>
        public Sticker(string name, Uri imageUri, IEnumerable<string> keywords)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.ImageUri = imageUri ?? throw new ArgumentNullException(nameof(imageUri));
            this.Keywords = keywords ?? throw new ArgumentNullException(nameof(keywords));
        }

        /// <summary>
        /// Gets the name of the <see cref="Sticker"/>.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the image <see cref="Uri"/> of the <see cref="Sticker"/>.
        /// </summary>
        public Uri ImageUri { get; private set; }

        /// <summary>
        /// Gets the keywords of the <see cref="Sticker"/>.
        /// </summary>
        public IEnumerable<string> Keywords { get; private set; }
    }
}
