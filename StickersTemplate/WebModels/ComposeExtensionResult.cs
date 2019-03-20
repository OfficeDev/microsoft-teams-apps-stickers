//----------------------------------------------------------------------------------------------
// <copyright file="ComposeExtensionResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.WebModels
{
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    /// <summary>
    /// Class used to serialize and deserialize a Compose Extension result.
    /// </summary>
    public class ComposeExtensionResult
    {
        /// <summary>
        /// Gets or sets the type of the result.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the attachment layout of the result.
        /// </summary>
        [JsonProperty("attachmentLayout")]
        public string AttachmentLayout { get; set; }

        /// <summary>
        /// Gets or sets the attachments of the result.
        /// </summary>
        [JsonProperty("attachments")]
        public Attachment[] Attachments { get; set; }
    }
}
