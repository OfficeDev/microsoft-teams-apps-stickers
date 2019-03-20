//----------------------------------------------------------------------------------------------
// <copyright file="StickerSetRepository.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Providers
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using StickersTemplate.Interfaces;
    using StickersTemplate.Models;
    using StickersTemplate.Providers.Serialization;

    /// <summary>
    /// A concrete implementation of the <see cref="IStickerSetRepository"/> interface.
    /// </summary>
    public class StickerSetRepository : IStickerSetRepository
    {
        private static readonly StickerSet DefaultStickerSet = new StickerSet("default", new Sticker[0]);

        private readonly ILogger logger;
        private readonly ISettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="StickerSetRepository"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="settings">The <see cref="ISettings"/>.</param>
        public StickerSetRepository(ILogger logger, ISettings settings)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <inheritdoc />
        public async Task<StickerSet> FetchStickerSetAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            using (var scope = this.logger.BeginScope($"{nameof(StickerSetRepository)}.{nameof(this.FetchStickerSetAsync)}"))
            {
                var configUri = this.settings.ConfigUri;
                if (configUri == null)
                {
                    this.logger.LogInformation($"{nameof(this.settings.ConfigUri)} was not a valid absolute URI; default sticker set will be used.");
                    return DefaultStickerSet;
                }

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(configUri);
                    if (!response.IsSuccessStatusCode)
                    {
                        this.logger.LogError($"GET {configUri} returned {response.StatusCode}: {response.ReasonPhrase}; default sticker set will be used.");
                        return DefaultStickerSet;
                    }

                    try
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var stickerConfig = JsonConvert.DeserializeObject<StickerConfigDTO>(responseContent);
                        var stickers = stickerConfig?.Images?.Select(image => new Sticker(image.Name, new Uri(image.ImageUri), image.Keywords)).ToArray();
                        return new StickerSet("Stickers", stickers);
                    }
                    catch (JsonException e)
                    {
                        this.logger.LogError(e, $"Response from GET {configUri} could not be parsed properly; default sticker set will be used.");
                        return DefaultStickerSet;
                    }
                }
            }
        }
    }
}
