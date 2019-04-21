//----------------------------------------------------------------------------------------------
// <copyright file="StickerConfigDTO.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Configuration.Providers.Serialization
{
    using Newtonsoft.Json;

    /// <summary>
    /// Model describing the <see cref="StickerConfigDTO"/> object.
    /// </summary>
    public class StickerConfigDTO
    {
        /// <summary>
        /// Gets or sets the images in this app.
        /// </summary>
        [JsonProperty("images")]
        public StickerDTO[] Images { get; set; }
    }
}
