//----------------------------------------------------------------------------------------------
// <copyright file="MessagesHttpFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.Extensibility;
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
        private readonly TelemetryClient telemetryClient;
        private readonly ISettings settings;
        private readonly IStickerSetRepository stickerSetRepository;
        private readonly IStickerSetIndexer stickerSetIndexer;
        private readonly ICredentialProvider credentialProvider;
        private readonly IChannelProvider channelProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagesHttpFunction"/> class.
        /// </summary>
        /// <param name="telemetryConfiguration">The telemetry configuration</param>
        /// <param name="settings">The <see cref="ISettings"/>.</param>
        /// <param name="stickerSetRepository">The <see cref="IStickerSetRepository"/>.</param>
        /// <param name="stickerSetIndexer">The <see cref="IStickerSetIndexer"/>.</param>
        /// <param name="credentialProvider">The <see cref="ICredentialProvider"/>.</param>
        /// <param name="channelProvider">The <see cref="IChannelProvider"/>.</param>
        [ExcludeFromCodeCoverage]
        public MessagesHttpFunction(
            TelemetryConfiguration telemetryConfiguration,
            ISettings settings = null,
            IStickerSetRepository stickerSetRepository = null,
            IStickerSetIndexer stickerSetIndexer = null,
            ICredentialProvider credentialProvider = null,
            IChannelProvider channelProvider = null)
        {
            this.telemetryClient = new TelemetryClient(telemetryConfiguration);
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

                // Use the configured service for tests or create ones to use.
                ISettings settings = this.settings ?? new Settings(logger, context);
                IStickerSetRepository stickerSetRepository = this.stickerSetRepository ?? new StickerSetRepository(logger, settings);
                IStickerSetIndexer stickerSetIndexer = this.stickerSetIndexer ?? new StickerSetIndexer(logger);
                ICredentialProvider credentialProvider = this.credentialProvider ?? new SimpleCredentialProvider(settings.MicrosoftAppId, null);
                IChannelProvider channelProvider = this.channelProvider ?? new SimpleChannelProvider();

                // Parse the incoming activity and authenticate the request
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

                // Log telemetry about the activity
                try
                {
                    this.LogActivityTelemetry(activity);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error sending user activity telemetry");
                }

                // Reject all activity types other than those related to messaging extensions
                if (!activity.IsComposeExtensionQuery())
                {
                    logger.LogDebug("Request payload was not a messaging extension query.");
                    return new BadRequestObjectResult($"App only supports messaging extension query activity types.");
                }

                // Get the query string. We expect exactly 1 parameter, so we take the first parameter, regardless of the name.
                var skip = 0;
                var query = string.Empty;

                if (activity.Value != null)
                {
                    var queryValue = JObject.FromObject(activity.Value).ToObject<ComposeExtensionValue>();
                    query = queryValue.GetParameterValue();

                    if (queryValue?.QueryOptions != null)
                    {
                        skip = queryValue.QueryOptions.Skip;
                    }
                }

                // Find matching stickers
                var stickerSet = await stickerSetRepository.FetchStickerSetAsync();
                await stickerSetIndexer.IndexStickerSetAsync(stickerSet);
                var stickers = await stickerSetIndexer.FindStickersByQuery(query, skip );

                var result = new ComposeExtensionResponse
                {
                    ComposeExtensionResult = new ComposeExtensionResult
                    {
                        Type = "result",
                        AttachmentLayout = "grid",
                        Attachments = stickers.Select(sticker => new StickerComposeExtensionCard(sticker).ToAttachment()).ToArray(),
                    },
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

        /// <summary>
        /// Log telemetry about the incoming activity.
        /// </summary>
        /// <param name="activity">The activity</param>
        private void LogActivityTelemetry(Activity activity)
        {
            var fromObjectId = activity.From?.Properties["aadObjectId"]?.ToString();
            var clientInfoEntity = activity.Entities?.Where(e => e.Type == "clientInfo")?.FirstOrDefault();
            var channelData = (JObject)activity.ChannelData;

            var properties = new Dictionary<string, string>
            {
                { "ActivityId", activity.Id },
                { "ActivityType", activity.Type },
                { "ActivityName", activity.Name },
                { "UserAadObjectId", fromObjectId },
                {
                    "ConversationType",
                    string.IsNullOrWhiteSpace(activity.Conversation?.ConversationType) ? "personal" : activity.Conversation.ConversationType
                },
                { "TeamId", channelData?["team"]?["id"]?.ToString() },
                { "SourceName", channelData?["source"]?["name"]?.ToString() },
                { "Locale", clientInfoEntity?.Properties["locale"]?.ToString() },
                { "Platform", clientInfoEntity?.Properties["platform"]?.ToString() },
            };
            this.telemetryClient.TrackEvent("UserActivity", properties);
        }
    }
}
