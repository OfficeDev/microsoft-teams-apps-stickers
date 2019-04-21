//----------------------------------------------------------------------------------------------
// <copyright file="StickerViewModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Configuration.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Web;

    /// <summary>
    /// Sticker View Model
    /// </summary>
    public class StickerViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StickerViewModel"/> class.
        /// </summary>
        public StickerViewModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StickerViewModel"/> class.
        /// </summary>
        /// <param name="sticker">Sticker</param>
        public StickerViewModel(Sticker sticker)
        {
            this.Id = sticker.Id;
            this.ImageUri = sticker.ImageUri;
            this.Name = sticker.Name;
            this.Keywords = string.Join(", ", sticker.Keywords ?? Enumerable.Empty<string>());
            this.Index = sticker.Index;
            this.State = sticker.State;
        }

        /// <summary>
        /// Gets or sets Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets ImageUri
        /// </summary>
        [Display(Name = "Image")]
        public Uri ImageUri { get; set; }

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
        public StickerState State { get; set; }

        /// <summary>
        /// Gets or sets File
        /// </summary>
        public HttpPostedFileBase File { get; set; }

        /// <summary>
        /// Get Keywords List
        /// </summary>
        /// <returns>Keywords List</returns>
        public List<string> GetKeywordsList()
        {
            return this.Keywords?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
        }
    }
}