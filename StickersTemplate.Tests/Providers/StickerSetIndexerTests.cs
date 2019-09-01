//----------------------------------------------------------------------------------------------
// <copyright file="StickerSetIndexerTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Tests.Providers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using StickersTemplate.Models;
    using Task = System.Threading.Tasks.Task;
    using StickersTemplate.Providers;
    using System.Linq;

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class StickerSetIndexerTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullLogger()
        {
            // Action
            new StickerSetIndexer(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task IndexStickerSetAsync_NullStickerSet()
        {
            // Setup
            var stickerSetIndexer = new StickerSetIndexer(new Mock<ILogger>().Object);

            // Action
            await stickerSetIndexer.IndexStickerSetAsync(null);
        }

        [TestMethod]
        public async Task Index_and_Find_EmptyStickerSet()
        {
            // Setup
            var stickerSetIndexer = new StickerSetIndexer(new Mock<ILogger>().Object);
            var stickers = new StickerSet("Default stickers", Enumerable.Empty<Sticker>());

            // Action
            await stickerSetIndexer.IndexStickerSetAsync(stickers);

            // Verify
            var indexedStickers = await stickerSetIndexer.FindStickersByQuery(null);
            Assert.IsNotNull(indexedStickers);
            Assert.AreEqual(0, indexedStickers.Count());
        }

        [TestMethod]
        public async Task Index_and_Find_TwoStickers_SameKeyword()
        {
            // Setup
            var stickerSetIndexer = new StickerSetIndexer(new Mock<ILogger>().Object);
            var stickers = new StickerSet("Default stickers", new Sticker[] {
                new Sticker("sticker 1", new Uri("https://microsoft.com"), new string[] { "key" }),
                new Sticker("sticker 2", new Uri("https://microsoft.com"), new string[] { "key" })
            });

            // Action
            await stickerSetIndexer.IndexStickerSetAsync(stickers);

            // Verify
            // no matching stickers.
            var indexedStickers = await stickerSetIndexer.FindStickersByQuery("not found");
            Assert.IsNotNull(indexedStickers);
            Assert.AreEqual(0, indexedStickers.Count());

            // matches both keywords.
            indexedStickers = await stickerSetIndexer.FindStickersByQuery("key");
            Assert.IsNotNull(indexedStickers);
            Assert.AreEqual(2, indexedStickers.Count());
            Assert.AreEqual("sticker 1", indexedStickers.ElementAt(0).Name);
            Assert.AreEqual("sticker 2", indexedStickers.ElementAt(1).Name);
        }

        [TestMethod]
        public async Task Index_and_Find_TwoStickers_DifferentKeyword()
        {
            // Setup
            var stickerSetIndexer = new StickerSetIndexer(new Mock<ILogger>().Object);
            var stickers = new StickerSet("Default stickers", new Sticker[] {
                new Sticker("sticker 1", new Uri("https://microsoft.com"), new string[] { "key1" }),
                new Sticker("sticker 2", new Uri("https://microsoft.com"), new string[] { "key2" })
            });

            // Action
            await stickerSetIndexer.IndexStickerSetAsync(stickers);

            // Verify
            // no matching stickers.
            var indexedStickers = await stickerSetIndexer.FindStickersByQuery("not found");
            Assert.IsNotNull(indexedStickers);
            Assert.AreEqual(0, indexedStickers.Count());

            // matches first keyword.
            indexedStickers = await stickerSetIndexer.FindStickersByQuery("key1");
            Assert.IsNotNull(indexedStickers);
            Assert.AreEqual(1, indexedStickers.Count());
            Assert.AreEqual("sticker 1", indexedStickers.ElementAt(0).Name);

            // matches second keyword.
            indexedStickers = await stickerSetIndexer.FindStickersByQuery("key2");
            Assert.IsNotNull(indexedStickers);
            Assert.AreEqual(1, indexedStickers.Count());
            Assert.AreEqual("sticker 2", indexedStickers.ElementAt(0).Name);

            // matches both keywords
            indexedStickers = await stickerSetIndexer.FindStickersByQuery("key");
            Assert.IsNotNull(indexedStickers);
            Assert.AreEqual(2, indexedStickers.Count());
            Assert.AreEqual("sticker 1", indexedStickers.ElementAt(0).Name);
            Assert.AreEqual("sticker 2", indexedStickers.ElementAt(1).Name);
        }

        [TestMethod]
        public async Task Index_and_Find_TwoStickers_Pagination()
        {
            // Setup
            var stickerSetIndexer = new StickerSetIndexer(new Mock<ILogger>().Object);
            var stickers = new StickerSet("Default stickers", new Sticker[] {
                new Sticker("sticker 1", new Uri("https://microsoft.com"), new string[] { "key1" }),
                new Sticker("sticker 2", new Uri("https://microsoft.com"), new string[] { "key2" })
            });

            // Action
            await stickerSetIndexer.IndexStickerSetAsync(stickers);

            // Verify
            // first sticker.
            var indexedStickers = await stickerSetIndexer.FindStickersByQuery(null, 0, 1);
            Assert.IsNotNull(indexedStickers);
            Assert.AreEqual(1, indexedStickers.Count());
            Assert.AreEqual("sticker 1", indexedStickers.ElementAt(0).Name);

            // second sticker.
            indexedStickers = await stickerSetIndexer.FindStickersByQuery(null, 1, 1);
            Assert.IsNotNull(indexedStickers);
            Assert.AreEqual(1, indexedStickers.Count());
            Assert.AreEqual("sticker 2", indexedStickers.ElementAt(0).Name);

            // both stickers.
            indexedStickers = await stickerSetIndexer.FindStickersByQuery(null, 0, 2);
            Assert.IsNotNull(indexedStickers);
            Assert.AreEqual(2, indexedStickers.Count());
            Assert.AreEqual("sticker 1", indexedStickers.ElementAt(0).Name);
            Assert.AreEqual("sticker 2", indexedStickers.ElementAt(1).Name);

            // more requested than available 1.
            indexedStickers = await stickerSetIndexer.FindStickersByQuery(null, 0, 5);
            Assert.IsNotNull(indexedStickers);
            Assert.AreEqual(2, indexedStickers.Count());
            Assert.AreEqual("sticker 1", indexedStickers.ElementAt(0).Name);
            Assert.AreEqual("sticker 2", indexedStickers.ElementAt(1).Name);

            // more requested than available 2.
            indexedStickers = await stickerSetIndexer.FindStickersByQuery(null, 5, 2);
            Assert.IsNotNull(indexedStickers);
            Assert.AreEqual(0, indexedStickers.Count());
        }
    }
}
