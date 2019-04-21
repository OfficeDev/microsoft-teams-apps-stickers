//----------------------------------------------------------------------------------------------
// <copyright file="StickerState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Configuration.Models
{
    /// <summary>
    /// Sticker State
    /// </summary>
    public enum StickerState
    {
        /// <summary>
        /// Default
        /// </summary>
        Default,

        /// <summary>
        /// Active
        /// </summary>
        Active = Default,

        /// <summary>
        /// SoftDeleted
        /// </summary>
        SoftDeleted,
    }
}