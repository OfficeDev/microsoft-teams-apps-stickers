//----------------------------------------------------------------------------------------------
// <copyright file="IStickerSetRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Interfaces
{
    using System.Threading;
    using System.Threading.Tasks;
    using StickersTemplate.Models;

    /// <summary>
    /// Interface describing all the methods of a class that can retrieve <see cref="StickerSet"/> objects.
    /// </summary>
    public interface IStickerSetRepository
    {
        /// <summary>
        /// Returns a <see cref="StickerSet"/> object.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>The resulting <see cref="StickerSet"/> if one was found. Null otherwise.</returns>
        Task<StickerSet> FetchStickerSetAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
