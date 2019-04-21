//----------------------------------------------------------------------------------------------
// <copyright file="IBlobStore.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Configuration.Providers
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Blob Store Interface
    /// </summary>
    public interface IBlobStore
    {
        /// <summary>
        /// Upload Blob Async
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="inputStream">Input Stream</param>
        /// <returns>Task Uri</returns>
        Task<Uri> UploadBlobAsync(string name, Stream inputStream);

        /// <summary>
        /// Delete Blob Async
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns>Task</returns>
        Task DeleteBlobAsync(string name);
    }
}
