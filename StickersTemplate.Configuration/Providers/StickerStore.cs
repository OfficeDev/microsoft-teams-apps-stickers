//----------------------------------------------------------------------------------------------
// <copyright file="StickerStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Configuration.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;
    using StickersTemplate.Configuration.Models;

    /// <summary>
    /// Sticker Store
    /// </summary>
    public class StickerStore : IStickerStore
    {
        private readonly string connectionString;
        private readonly string tableName;
        private readonly Task initializeTask;
        private CloudTable cloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="StickerStore"/> class.
        /// </summary>
        /// <param name="connectionString">Connection String</param>
        /// <param name="tableName">Table Name</param>
        public StickerStore(string connectionString, string tableName)
        {
            this.connectionString = connectionString;
            this.tableName = tableName;
            this.initializeTask = this.InitializeAsync();
        }

        /// <summary>
        /// Get Sticker Async
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>Task Sticker</returns>
        public async Task<Sticker> GetStickerAsync(string id)
        {
            await this.initializeTask;

            TableOperation tableOperation = TableOperation.Retrieve<AzureTableSticker>(AzureTableSticker.DefaultPartitionKey, id);
            TableResult getResult = await this.cloudTable.ExecuteAsync(tableOperation);
            if (IsSuccessCode(getResult.HttpStatusCode))
            {
                return (getResult.Result as AzureTableSticker)?.ToSticker();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Delete Sticker Async
        /// </summary>
        /// <param name="stickerId">Sticker Id</param>
        /// <returns>Task</returns>
        public async Task DeleteStickerAsync(string stickerId)
        {
            await this.initializeTask;

            var deleteOperation = TableOperation.Delete(new AzureTableSticker(stickerId));
            var result = await this.cloudTable.ExecuteAsync(deleteOperation);
            if (!IsSuccessCode(result.HttpStatusCode) && (result.HttpStatusCode != (int)HttpStatusCode.NotFound))
            {
                throw new Exception($"Failed to delete sticker {stickerId}");
            }
        }

        /// <summary>
        /// Get Sticker Async
        /// </summary>
        /// <returns>List of Sticker</returns>
        public async Task<IList<Sticker>> GetStickersAsync()
        {
            await this.initializeTask;

            return this.cloudTable.CreateQuery<AzureTableSticker>()
                .ToList()
                .Select(s => s.ToSticker())
                .OrderBy(s => s.Index)
                .ToList();
        }

        /// <summary>
        /// Update Sticker Async
        /// </summary>
        /// <param name="sticker">Sticker</param>
        /// <returns>Task</returns>
        public async Task UpdateStickerAsync(Sticker sticker)
        {
            await this.initializeTask;

            var saveOperation = TableOperation.InsertOrReplace(new AzureTableSticker(sticker));
            var saveResult = await this.cloudTable.ExecuteAsync(saveOperation);
            if (!IsSuccessCode(saveResult.HttpStatusCode))
            {
                throw new Exception($"Table storage returned errorcode {saveResult.HttpStatusCode} for sticker {sticker.Id}");
            }
        }

        /// <summary>
        /// Create Sticker Async
        /// </summary>
        /// <param name="sticker">Stricker</param>
        /// <returns>Task</returns>
        public async Task CreateStickerAsync(Sticker sticker)
        {
            var count = this.cloudTable.CreateQuery<AzureTableSticker>()
                .Select(s => s.RowKey)
                .ToList()
                .Count;
            sticker.Index = count++;

            var operation = TableOperation.InsertOrReplace(new AzureTableSticker(sticker));
            var saveResult = await this.cloudTable.ExecuteAsync(operation);
            if (!IsSuccessCode(saveResult.HttpStatusCode))
            {
                throw new Exception($"Table storage returned errorcode {saveResult.HttpStatusCode} when adding {sticker.Id}");
            }
        }

        /// <summary>
        /// Update Stickers Async
        /// </summary>
        /// <param name="stickers">List of Sticker</param>
        /// <returns>Task</returns>
        public async Task UpdateStickersAsync(IList<Sticker> stickers)
        {
            await this.initializeTask;

            int index = 0;
            var batchOperation = new TableBatchOperation();
            foreach (var sticker in stickers)
            {
                sticker.Index = index++;    // Set the indexes
                var dto = new AzureTableSticker(sticker);
                batchOperation.Merge(dto);
            }

            await this.cloudTable.ExecuteBatchAsync(batchOperation);
        }

        /// <summary>
        /// Validates if the success code was success or not.
        /// </summary>
        /// <param name="httpStatusCode">The status code to be validated.</param>
        /// <returns>true if the httpStatusCode is successful, false otherwise.</returns>
        private static bool IsSuccessCode(int httpStatusCode)
        {
            return httpStatusCode >= 200 && httpStatusCode <= 299;
        }

        private async Task InitializeAsync()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.connectionString);
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            CloudTable table = tableClient.GetTableReference(this.tableName);
            await table.CreateIfNotExistsAsync();

            this.cloudTable = table;
        }
    }
}
