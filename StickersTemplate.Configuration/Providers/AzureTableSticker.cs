//----------------------------------------------------------------------------------------------
// <copyright file="AzureTableSticker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Configuration.Providers
{
    using System;
    using System.Linq;
    using Microsoft.WindowsAzure.Storage.Table;
    using StickersTemplate.Configuration.Models;

    /// <summary>
    /// Azure Table Sticker
    /// </summary>
    public class AzureTableSticker : TableEntity
    {
        /// <summary>
        /// Default Partition Key
        /// </summary>
        public const string DefaultPartitionKey = "default";

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableSticker"/> class.
        /// </summary>
        public AzureTableSticker()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableSticker"/> class.
        /// </summary>
        /// <param name="id">Id</param>
        public AzureTableSticker(string id)
        {
            this.PartitionKey = DefaultPartitionKey;
            this.RowKey = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureTableSticker"/> class.
        /// </summary>
        /// <param name="sticker">Sticker</param>
        public AzureTableSticker(Sticker sticker)
        {
            this.PartitionKey = DefaultPartitionKey;
            this.RowKey = sticker.Id ?? Guid.NewGuid().ToString("D");
            this.ImageUrl = sticker.ImageUri?.AbsoluteUri;
            this.Name = sticker.Name;
            this.Keywords = string.Join(",", sticker.Keywords ?? Enumerable.Empty<string>());
            this.Index = sticker.Index;
            this.State = sticker.State.ToString();
        }

        /// <summary>
        /// Gets or sets ImageUrl
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets Keywords
        /// </summary>
        public string Keywords { get; set; }

        /// <summary>
        /// Gets or sets Index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets State
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// To Sticker
        /// </summary>
        /// <returns>Sticker</returns>
        public Sticker ToSticker()
        {
            StickerState stickerState;
            if (!Enum.TryParse(this.State, out stickerState))
            {
                stickerState = StickerState.Active;
            }

            return new Sticker
            {
                Id = this.RowKey,
                ImageUri = !string.IsNullOrEmpty(this.ImageUrl) ? new Uri(this.ImageUrl) : null,
                Name = this.Name,
                Keywords = this.Keywords?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries) ?? new string[0],
                Index = this.Index,
                State = stickerState,
            };
        }
    }
}