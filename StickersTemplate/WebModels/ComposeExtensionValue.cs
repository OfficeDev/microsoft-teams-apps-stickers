//----------------------------------------------------------------------------------------------
// <copyright file="ComposeExtensionValue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.WebModels
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class used to serialize and deserialize a Compose Extension value.
    /// </summary>
    public class ComposeExtensionValue
    {
        /// <summary>
        /// Gets or sets the command Id of the value.
        /// </summary>
        [JsonProperty("commandId")]
        public string CommandId { get; set; }

        /// <summary>
        /// Gets or sets the parameters of the value.
        /// </summary>
        [JsonProperty("parameters")]
        public ComposeExtensionParameter[] Parameters { get; set; }

        /// <summary>
        /// Gets or sets the query options of the value.
        /// </summary>
        [JsonProperty("queryOptions")]
        public ComposeExtensionQueryOptions QueryOptions { get; set; }
    }
}
