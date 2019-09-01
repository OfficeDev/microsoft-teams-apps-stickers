//----------------------------------------------------------------------------------------------
// <copyright file="IStickerStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Configuration.Providers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using StickersTemplate.Configuration.Models;

    /// <summary>
    /// Sticker Store Interface
    /// </summary>
    public interface IStickerStore
    {
        /// <summary>
        /// Get Stickers Async
        /// </summary>
        /// <returns>List of Sticker</returns>
        Task<IList<Sticker>> GetStickersAsync();

        /// <summary>
        /// Get Sticker Async
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Sticker</returns>
        Task<Sticker> GetStickerAsync(string id);

        /// <summary>
        /// Update Stickers Async
        /// </summary>
        /// <param name="stickers">List of Sticker</param>
        /// <returns>Task</returns>
        Task UpdateStickersAsync(IList<Sticker> stickers);

        /// <summary>
        /// Update Sticker Async
        /// </summary>
        /// <param name="sticker">Sticker</param>
        /// <returns>Task</returns>
        Task UpdateStickerAsync(Sticker sticker);

        /// <summary>
        /// Delete Sticker Async
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Task</returns>
        Task DeleteStickerAsync(string id);

        /// <summary>
        /// Create Sticker Async
        /// </summary>
        /// <param name="sticker">Sticker</param>
        /// <returns>Task</returns>
        Task CreateStickerAsync(Sticker sticker);
    }
}
