//----------------------------------------------------------------------------------------------
// <copyright file="StickerDTO.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Providers.Serialization
{
    using Newtonsoft.Json;

    /// <summary>
    /// Model describing the <see cref="StickerDTO"/> object.
    /// </summary>
    public class StickerDTO
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the image uri.
        /// </summary>
        [JsonProperty("imageUri")]
        public string ImageUri { get; set; }

        /// <summary>
        /// Gets or sets the keywords.
        /// </summary>
        [JsonProperty("keywords")]
        public string[] Keywords { get; set; }
    }
}
