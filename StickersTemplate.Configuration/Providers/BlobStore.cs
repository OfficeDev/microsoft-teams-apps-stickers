//----------------------------------------------------------------------------------------------
// <copyright file="BlobStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Configuration.Providers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;

    /// <summary>
    /// Blob Storage
    /// </summary>
    public class BlobStore : IBlobStore
    {
        private readonly Task initializeTask;
        private CloudBlobContainer blobContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlobStore"/> class.
        /// </summary>
        /// <param name="connectionString">Connection String</param>
        /// <param name="containerName">Container Name</param>
        public BlobStore(string connectionString, string containerName)
        {
            this.initializeTask = this.InitializeAsync(connectionString, containerName);
        }

        /// <summary>
        /// Upload Blob Async
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="inputStream">Input Stream</param>
        /// <returns>Task Uri</returns>
        public async Task<Uri> UploadBlobAsync(string name, Stream inputStream)
        {
            await this.initializeTask;

            var blob = this.blobContainer.GetBlockBlobReference(name);
            await blob.UploadFromStreamAsync(inputStream);

            return blob.Uri;
        }

        /// <summary>
        /// Delete Blob Async
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Task</returns>
        public async Task DeleteBlobAsync(string name)
        {
            await this.initializeTask;

            var blob = this.blobContainer.GetBlockBlobReference(name);
            await blob.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Initialize Async
        /// </summary>
        /// <param name="connectionString">Connection String</param>
        /// <param name="containerName">Container Name</param>
        /// <returns>Task</returns>
        public async Task InitializeAsync(string connectionString, string containerName)
        {
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();

            this.blobContainer = cloudBlobClient.GetContainerReference(containerName);
            if (await this.blobContainer.CreateIfNotExistsAsync())
            {
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                await this.blobContainer.SetPermissionsAsync(permissions);
            }
        }
    }
}
