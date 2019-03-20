//----------------------------------------------------------------------------------------------
// <copyright file="ComposeExtensionResponse.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.WebModels
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class used to serialize and deserialize a Compose Extension response.
    /// </summary>
    public class ComposeExtensionResponse
    {
        /// <summary>
        /// Gets or sets the result of the response.
        /// </summary>
        [JsonProperty("composeExtension")]
        public ComposeExtensionResult ComposeExtensionResult { get; set; }
    }
}
