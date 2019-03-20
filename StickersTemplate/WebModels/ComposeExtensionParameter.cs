//----------------------------------------------------------------------------------------------
// <copyright file="ComposeExtensionParameter.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.WebModels
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class used to serialize and deserialize a Compose Extension parameter.
    /// </summary>
    public class ComposeExtensionParameter
    {
        /// <summary>
        /// Gets or sets the name of the parameter.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value of the parameter.
        /// </summary>
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
