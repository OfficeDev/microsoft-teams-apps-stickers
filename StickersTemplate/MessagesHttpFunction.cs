//----------------------------------------------------------------------------------------------
// <copyright file="MessagesHttpFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using StickersTemplate.Cards;
    using StickersTemplate.Config;
    using StickersTemplate.Extensions;
    using StickersTemplate.Interfaces;
    using StickersTemplate.Providers;
    using StickersTemplate.WebModels;

    /// <summary>
    /// Azure Function that handles HTTP messages from BotFramework.
    /// </summary>
    public class MessagesHttpFunction
    {
        private readonly ISettings settings;
        private readonly IStickerSetRepository stickerSetRepository;
        private readonly IStickerSetIndexer stickerSetIndexer;
        private readonly ICredentialProvider credentialProvider;
        private readonly IChannelProvider channelProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagesHttpFunction"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="ISettings"/>The settins provider</param>
        /// <param name="stickerSetRepository">The <see cref="IStickerSetRepository"/>The sticker set repository</param>
        /// <param name="stickerSetIndexer">The <see cref="IStickerSetIndexer"/>The sticker set indexer</param>
        /// <param name="credentialProvider">The <see cref="ICredentialProvider"/>The credential provider</param>
        /// <param name="channelProvider">The <see cref="IChannelProvider"/>The channel provider</param>
        [ExcludeFromCodeCoverage]
        public MessagesHttpFunction(
            ISettings settings = null,
            IStickerSetRepository stickerSetRepository = null,
            IStickerSetIndexer stickerSetIndexer = null,
            ICredentialProvider credentialProvider = null,
            IChannelProvider channelProvider = null)
        {
            this.settings = settings;
            this.stickerSetRepository = stickerSetRepository;
            this.stickerSetIndexer = stickerSetIndexer;
            this.credentialProvider = credentialProvider;
            this.channelProvider = channelProvider;
        }

        /// <summary>
        /// Method that is called by the Azure Function framework when this Azure Function is invoked.
        /// </summary>
        /// <param name="req">The <see cref="HttpRequest"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="context">The <see cref="ExecutionContext"/>.</param>
        /// <returns>A <see cref="Task"/> that results in an <see cref="IActionResult"/> when awaited.</returns>
        [FunctionName("messages")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger logger,
            ExecutionContext context)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }

            using (var scope = logger.BeginScope($"{nameof(MessagesHttpFunction.Run)}"))
            {
                if (req == null)
                {
                    throw new ArgumentNullException(nameof(req));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                logger.LogInformation("Messages function received a request.");

                // Use the configured service for tests or create the real one to use.
                ISettings settings = this.settings ?? new Settings(logger, context);
                IStickerSetRepository stickerSetRepository = this.stickerSetRepository ?? new StickerSetRepository(logger, settings);
                IStickerSetIndexer stickerSetIndexer = this.stickerSetIndexer ?? new StickerSetIndexer(logger);
                ICredentialProvider credentialProvider = this.credentialProvider ?? new SimpleCredentialProvider(settings.MicrosoftAppId, null);
                IChannelProvider channelProvider = this.channelProvider ?? new SimpleChannelProvider();

                Activity activity;
                try
                {
                    var authorizationHeader = GetAuthorizationHeader(req);
                    activity = await ParseRequestBody(req);
                    await JwtTokenValidation.AuthenticateRequest(activity, authorizationHeader, credentialProvider, channelProvider);
                }
                catch (JsonReaderException e)
                {
                    logger.LogDebug(e, "JSON parser failed to parse request payload.");
                    return new BadRequestResult();
                }
                catch (UnauthorizedAccessException e)
                {
                    logger.LogDebug(e, "Request was not propertly authorized.");
                    return new UnauthorizedResult();
                }

                if (!activity.IsComposeExtensionQuery())
                {
                    logger.LogDebug("Request payload was not a compose extension query.");
                    return new BadRequestObjectResult($"App only supports compose extension query activity types.");
                }

                var query = string.Empty;
                if (activity.Value != null)
                {
                    var queryValue = JObject.FromObject(activity.Value).ToObject<ComposeExtensionValue>();
                    query = queryValue.GetParameterValue();
                }

                var stickerSet = await stickerSetRepository.FetchStickerSetAsync();
                await stickerSetIndexer.IndexStickerSetAsync(stickerSet);
                var stickers = await stickerSetIndexer.FindStickersByQuery(query);

                var result = new ComposeExtensionResponse
                {
                    ComposeExtensionResult = new ComposeExtensionResult
                    {
                        Type = "result",
                        AttachmentLayout = "grid",
                        Attachments = stickers.Select(sticker => new StickerComposeExtensionCard(sticker).ToAttachment()).ToArray()
                    }
                };

                return new OkObjectResult(result);
            }
        }

        /// <summary>
        /// Gets the authorization header from the <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="req">The <see cref="HttpRequest"/>/</param>
        /// <exception cref="UnauthorizedAccessException">If there is no authorization header.</exception>
        /// <returns>The authorization header.</returns>
        private static string GetAuthorizationHeader(HttpRequest req)
        {
            if (req.Headers == null || !req.Headers.TryGetValue("Authorization", out var authHeaders) || authHeaders.Count < 1)
            {
                throw new UnauthorizedAccessException();
            }

            return authHeaders[0];
        }

        /// <summary>
        /// Attempts to parse the <see cref="HttpRequest"/> body into a <see cref="Activity"/> object.
        /// </summary>
        /// <param name="req">The <see cref="HttpRequest"/>.</param>
        /// <exception cref="JsonReaderException">If the <see cref="HttpRequest"/> body could not be parsed properly.</exception>
        /// <returns>The <see cref="Activity"/> object.</returns>
        private static async Task<Activity> ParseRequestBody(HttpRequest req)
        {
            if (req.Body == null)
            {
                throw new InvalidOperationException($"{nameof(req)}.{nameof(req.Body)} cannot be null");
            }

            using (var streamReader = new StreamReader(req.Body))
            using (var jsonReader = new JsonTextReader(streamReader))
            {
                var activityJObject = await JObject.LoadAsync(jsonReader);
                return activityJObject.ToObject<Activity>();
            }
        }
    }
}
