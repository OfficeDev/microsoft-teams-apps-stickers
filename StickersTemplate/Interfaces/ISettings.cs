//----------------------------------------------------------------------------------------------
// <copyright file="ISettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Interfaces
{
    using System;

    /// <summary>
    /// Interface describing all of the app settings.
    /// </summary>
    public interface ISettings
    {
        /// <summary>
        /// Gets the Microsoft App Id.
        /// </summary>
        string MicrosoftAppId { get; }

        /// <summary>
        /// Gets the config uri for the Sticker set.
        /// </summary>
        Uri ConfigUri { get; }
    }
}
