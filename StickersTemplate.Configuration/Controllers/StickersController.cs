//----------------------------------------------------------------------------------------------
// <copyright file="StickersController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Configuration.Controllers
{
    using System;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using ImageResizer;
    using Microsoft.Azure;
    using Newtonsoft.Json;
    using StickersTemplate.Configuration.Models;
    using StickersTemplate.Configuration.Providers;
    using StickersTemplate.Configuration.Providers.Serialization;

    /// <summary>
    /// Stickers Controller
    /// </summary>
    [Authorize]
    public class StickersController : Controller
    {
        private readonly IStickerStore stickerStore;
        private readonly IBlobStore blobStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="StickersController"/> class.
        /// </summary>
        public StickersController()
        {
            var connectionString = CloudConfigurationManager.GetSetting("StorageConnectionString");
            this.stickerStore = new StickerStore(connectionString, CloudConfigurationManager.GetSetting("StickersTableName"));
            this.blobStore = new BlobStore(connectionString, CloudConfigurationManager.GetSetting("StickersBlobContainerName"));
        }

        /// <summary>
        /// GET: Stickers
        /// </summary>
        /// <returns>Task Action Result</returns>
        public async Task<ActionResult> Index()
        {
            var stickers = await this.stickerStore.GetStickersAsync();
            var activeStickers = stickers.Where(s => s.State == StickerState.Active);
            return this.View(activeStickers.Select(s => new StickerViewModel(s)));
        }

        /// <summary>
        /// GET: Stickers/Create
        /// </summary>
        /// <returns>Action Result</returns>
        public ActionResult Create()
        {
            return this.View();
        }

        /// <summary>
        /// POST: Stickers/Create
        /// To protect from overposting attacks, please enable the specific properties you want to bind to, for
        /// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        /// </summary>
        /// <param name="stickerViewModel">Sticker View Model</param>
        /// <returns>Task Action Result</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "File,Name,Keywords")] StickerViewModel stickerViewModel)
        {
            if (this.ModelState.IsValid)
            {
                var id = Guid.NewGuid().ToString("D");
                Uri imageUri;

                if (Path.GetExtension(stickerViewModel.File.FileName).ToLower() == ".gif")
                {
                    imageUri = await this.blobStore.UploadBlobAsync(id, stickerViewModel.File.InputStream);
                }
                else
                {
                    // Resize the image to fit within the maximum dimensions
                    using (var memoryStream = new MemoryStream())
                    {
                        ImageBuilder.Current.Build(stickerViewModel.File, memoryStream, new ResizeSettings($"maxwidth={Sticker.MaximumDimensionInPixels}&maxheight={Sticker.MaximumDimensionInPixels}"));
                        memoryStream.Seek(0, SeekOrigin.Begin);

                        imageUri = await this.blobStore.UploadBlobAsync(id, memoryStream);
                    }
                }

                await this.stickerStore.CreateStickerAsync(new Sticker
                {
                    Id = id,
                    ImageUri = imageUri,
                    Name = stickerViewModel.Name,
                    Keywords = stickerViewModel?.GetKeywordsList(),
                    State = StickerState.Active,
                });

                return this.RedirectToAction("Index");
            }

            return this.View(stickerViewModel);
        }

        /// <summary>
        /// GET: Stickers/Edit/5
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Task Action Result</returns>
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Sticker sticker = await this.stickerStore.GetStickerAsync(id);
            if (sticker == null)
            {
                return this.HttpNotFound();
            }

            return this.View(new StickerViewModel(sticker));
        }

        /// <summary>
        /// POST: Stickers/Edit/5
        /// To protect from overposting attacks, please enable the specific properties you want to bind to, for
        /// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        /// </summary>
        /// <param name="stickerViewModel">Sticker View Model</param>
        /// <returns>Task Action Result</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Name,Keywords")] StickerViewModel stickerViewModel)
        {
            if (this.ModelState.IsValid)
            {
                Sticker sticker = await this.stickerStore.GetStickerAsync(stickerViewModel.Id);

                sticker.Name = stickerViewModel.Name;
                sticker.Keywords = stickerViewModel?.GetKeywordsList();
                await this.stickerStore.UpdateStickerAsync(sticker);

                return this.RedirectToAction("Index");
            }

            return this.View(stickerViewModel);
        }

        /// <summary>
        /// GET: Stickers/Delete/5
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Task Action Result</returns>
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Sticker sticker = await this.stickerStore.GetStickerAsync(id);
            if (sticker == null)
            {
                return this.HttpNotFound();
            }

            return this.View(new StickerViewModel(sticker));
        }

        /// <summary>
        /// POST: Stickers/Delete/5
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Task Action Result</returns>
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Sticker sticker = await this.stickerStore.GetStickerAsync(id);
            if (sticker == null)
            {
                return this.HttpNotFound();
            }

            sticker.State = StickerState.SoftDeleted;
            await this.stickerStore.UpdateStickerAsync(sticker);

            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// POST: Stickers/DeletePermanently/5
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Task Action Result</returns>
        [HttpPost]
        [ActionName("DeletePermanently")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeletePermanentlyConfirmed(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            await this.stickerStore.DeleteStickerAsync(id);

            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// POST: Stickers/Publish
        /// </summary>
        /// <returns>Task Action Result</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Publish()
        {
            var stickers = await this.stickerStore.GetStickersAsync();
            var activeStickers = stickers.Where(s => s.State == StickerState.Active);

            var configuration = new StickerConfigDTO
            {
                Images = activeStickers
                    .Select(s => new StickerDTO
                    {
                        ImageUri = s.ImageUri.AbsoluteUri,
                        Name = s.Name,
                        Keywords = s.Keywords.ToArray(),
                    })
                    .ToArray()
            };
            var configurationJson = JsonConvert.SerializeObject(configuration, Formatting.None);
            await this.blobStore.UploadBlobAsync("stickers.json", new MemoryStream(Encoding.UTF8.GetBytes(configurationJson)));

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
    }
}
