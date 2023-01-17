//----------------------------------------------------------------------------------------------
// <copyright file="IStickerSetIndexer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Interfaces
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using StickersTemplate.Models;

    /// <summary>
    /// Interface describing all the methods of a class that can index a <see cref="StickerSet"/>.
    /// </summary>
    public interface IStickerSetIndexer
    {
        /// <summary>
        /// Indexes a given <see cref="StickerSet"/> for future querying.
        /// </summary>
        /// <param name="stickerSet">The <see cref="StickerSet"/> to index.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="Task"/> to await for the result.</returns>
        Task IndexStickerSetAsync(StickerSet stickerSet, CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Finds a non-duplicate set of <see cref="Sticker"/> objects given an input query.
        /// </summary>
        /// <param name="query">The input query.</param>
        /// <param name="skip">How many stickers to skip; useful for pagination.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A non-duplicate set of <see cref="Sticker"/> objects.</returns>
        Task<IEnumerable<Sticker>> FindStickersByQuery(string query, int skip = 0, CancellationToken cancellationToken = default(CancellationToken));
    }
}
