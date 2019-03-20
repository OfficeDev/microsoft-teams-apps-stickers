//----------------------------------------------------------------------------------------------
// <copyright file="ComposeExtensionQueryOptions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.WebModels
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class used to serialize and deserialize a Compose Extension query options.
    /// </summary>
    public class ComposeExtensionQueryOptions
    {
        /// <summary>
        /// Gets or sets the skip of the query options.
        /// </summary>
        [JsonProperty("skip")]
        public int Skip { get; set; }

        /// <summary>
        /// Gets or sets the count of the query options.
        /// </summary>
        [JsonProperty("count")]
        public int Count { get; set; }
    }
}
