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
    public static class MessagesHttpFunction
    {
        private static ISettings settings;
        private static IStickerSetRepository stickerSetRepository;
        private static IStickerSetIndexer stickerSetIndexer;
        private static ICredentialProvider credentialProvider;
        private static IChannelProvider channelProvider;

        /// <summary>
        /// Configures the services used by this Azure function for a test run.
        /// This is necessary because of the static nature of Azure functions (and lack of DI).
        /// </summary>
        /// <param name="settings">The <see cref="ISettings"/>.</param>
        /// <param name="stickerSetRepository">The <see cref="IStickerSetRepository"/>.</param>
        /// <param name="stickerSetIndexer">The <see cref="IStickerSetIndexer"/>.</param>
        /// <param name="credentialProvider">The <see cref="ICredentialProvider"/>.</param>
        /// <param name="channelProvider">The <see cref="IChannelProvider"/>.</param>
        [ExcludeFromCodeCoverage]
        public static void ConfigureForTests(
            ISettings settings,
            IStickerSetRepository stickerSetRepository,
            IStickerSetIndexer stickerSetIndexer,
            ICredentialProvider credentialProvider,
            IChannelProvider channelProvider)
        {
            MessagesHttpFunction.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            MessagesHttpFunction.stickerSetRepository = stickerSetRepository ?? throw new ArgumentNullException(nameof(stickerSetRepository));
            MessagesHttpFunction.stickerSetIndexer = stickerSetIndexer ?? throw new ArgumentNullException(nameof(stickerSetIndexer));
            MessagesHttpFunction.credentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider));
            MessagesHttpFunction.channelProvider = channelProvider ?? throw new ArgumentNullException(nameof(channelProvider));
        }

        /// <summary>
        /// Method that is called by the Azure Function framework when this Azure Function is invoked.
        /// </summary>
        /// <param name="req">The <see cref="HttpRequest"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="context">The <see cref="ExecutionContext"/>.</param>
        /// <returns>A <see cref="Task"/> that results in an <see cref="IActionResult"/> when awaited.</returns>
        [FunctionName("messages")]
        public static async Task<IActionResult> Run(
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
                ISettings settings = MessagesHttpFunction.settings ?? new Settings(logger, context);
                IStickerSetRepository stickerSetRepository = MessagesHttpFunction.stickerSetRepository ?? new StickerSetRepository(logger, settings);
                IStickerSetIndexer stickerSetIndexer = MessagesHttpFunction.stickerSetIndexer ?? new StickerSetIndexer(logger);
                ICredentialProvider credentialProvider = MessagesHttpFunction.credentialProvider ?? new SimpleCredentialProvider(settings.MicrosoftAppId, null);
                IChannelProvider channelProvider = MessagesHttpFunction.channelProvider ?? new SimpleChannelProvider();

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
