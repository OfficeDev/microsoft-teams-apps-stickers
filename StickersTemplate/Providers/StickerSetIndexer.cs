//----------------------------------------------------------------------------------------------
// <copyright file="StickerSetIndexer.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using StickersTemplate.Interfaces;
    using StickersTemplate.Models;

    /// <summary>
    /// A concrete implementation of the <see cref="IStickerSetIndexer"/> interface.
    /// </summary>
    public class StickerSetIndexer : IStickerSetIndexer
    {
        private readonly List<Sticker> allStickers = new List<Sticker>();
        private readonly Dictionary<string, List<Sticker>> stickerKeywordMap = new Dictionary<string, List<Sticker>>();
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StickerSetIndexer"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public StickerSetIndexer(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Task IndexStickerSetAsync(StickerSet stickerSet, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var scope = this.logger.BeginScope($"{nameof(StickerSetIndexer)}.{nameof(this.IndexStickerSetAsync)}"))
            {
                if (stickerSet == null)
                {
                    throw new ArgumentNullException(nameof(stickerSet));
                }

                this.allStickers.AddRange(stickerSet);

                foreach (var sticker in stickerSet)
                {
                    foreach (var keyword in sticker.Keywords)
                    {
                        var indexedKeyword = keyword.ToLowerInvariant().Trim();
                        if (!this.stickerKeywordMap.TryGetValue(indexedKeyword, out var indexedStickers))
                        {
                            indexedStickers = new List<Sticker>();
                            this.stickerKeywordMap.Add(indexedKeyword, indexedStickers);
                        }

                        indexedStickers.Add(sticker);
                    }
                }

                return Task.CompletedTask;
            }
        }

        /// <inheritdoc />
        public Task<IEnumerable<Sticker>> FindStickersByQuery(string query, int skip = 0, int count = 25, CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var scope = this.logger.BeginScope($"{nameof(StickerSetIndexer)}.{nameof(this.FindStickersByQuery)}"))
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    var stickers = this.allStickers.Skip(skip).Take(count);
                    return Task.FromResult<IEnumerable<Sticker>>(stickers);
                }

                var queryWords = query.Trim().ToLowerInvariant().Split(' ');

                var matchedStickers = this.stickerKeywordMap
                    .Where((keyValuePair) => queryWords.Any((word) => keyValuePair.Key.StartsWith(word)))
                    .SelectMany((keyValuePair) => keyValuePair.Value)
                    .Distinct()
                    .Skip(skip)
                    .Take(count)
                    .ToArray();

                return Task.FromResult<IEnumerable<Sticker>>(matchedStickers);
            }
        }
    }
}
