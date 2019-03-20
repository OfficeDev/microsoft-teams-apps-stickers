//----------------------------------------------------------------------------------------------
// <copyright file="AttachmentExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Extensions
{
    using System;
    using Microsoft.Bot.Schema;
    using Newtonsoft.Json;

    /// <summary>
    /// Extension methods for the <see cref="Attachment"/> class.
    /// </summary>
    public static class AttachmentExtensions
    {
        /// <summary>
        /// Converts the <see cref="Attachment"/> into a Compose Extension query result.
        /// </summary>
        /// <param name="content">The content <see cref="Attachment"/>.</param>
        /// <param name="preview">The preview <see cref="Attachment"/>.</param>
        /// <returns>The Compose Extension query result <see cref="Attachment"/>.</returns>
        public static Attachment ToComposeExtensionResult(this Attachment content, Attachment preview)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (preview == null)
            {
                throw new ArgumentNullException(nameof(preview));
            }

            return new ComposeExtensionResult
            {
                ContentType = content.ContentType,
                Content = content.Content,
                ContentUrl = content.ContentUrl,
                Name = content.Name,
                Properties = content.Properties,
                ThumbnailUrl = content.ThumbnailUrl,
                Preview = preview
            };
        }

        /// <summary>
        /// Private class to format the Compose Extension result <see cref="Attachment"/> properly,
        /// </summary>
        private class ComposeExtensionResult : Attachment
        {
            /// <summary>
            /// Gets or sets the preview <see cref="Attachment"/>.
            /// </summary>
            [JsonProperty("preview")]
            public Attachment Preview { get; set; }
        }
    }
}
