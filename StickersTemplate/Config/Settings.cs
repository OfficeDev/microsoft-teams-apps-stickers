//----------------------------------------------------------------------------------------------
// <copyright file="Settings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Config
{
    using System;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using StickersTemplate.Interfaces;

    /// <summary>
    /// Concrete implementation of the <see cref="ISettings"/> interface that uses the <see cref="ConfigurationBuilder"/> class to get the config.
    /// </summary>
    public class Settings : ISettings
    {
        private readonly IConfigurationRoot config;
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Settings"/> class.
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="context">The <see cref="ExecutionContext"/>.</param>
        public Settings(ILogger logger, ExecutionContext context)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            this.config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        /// <inheritdoc/>
        public string MicrosoftAppId => this.StringValue("MicrosoftAppId");

        /// <inheritdoc/>
        public Uri ConfigUri => this.UriValue("ConfigUri", true);

        /// <summary>
        /// Parses a config value into a <see cref="string"/>.
        /// </summary>
        /// <param name="configName">The name of the config value.</param>
        /// <param name="optional">Whether this parameter is optional or not.</param>
        /// <returns>A parsed <see cref="string"/>.</returns>
        private string StringValue(string configName, bool optional = false)
        {
            var value = this.config[configName];
            if (value == null)
            {
                if (!optional)
                {
                    this.logger.LogError($"Config parameter '{configName}' not provided.");
                    throw new InvalidOperationException($"Config parameter '{configName}' is required.");
                }
                else
                {
                    this.logger.LogInformation($"Config parameter '{configName}' not provided.");
                }
            }

            return value;
        }

        /// <summary>
        /// Parses a config value into a <see cref="Uri"/>.
        /// </summary>
        /// <param name="configName">The name of the config value.</param>
        /// <param name="optional">Whether this parameter is optional or not.</param>
        /// <returns>A parsed <see cref="Uri"/>.</returns>
        private Uri UriValue(string configName, bool optional = false)
        {
            var value = this.config[configName];
            if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            {
                if (!optional)
                {
                    this.logger.LogError($"Config parameter '{configName}' not provided or is not a valid absolute URI.");
                    throw new InvalidOperationException($"Config parameter '{configName}' is required and must be a valid absolute URI.");
                }
                else
                {
                    this.logger.LogInformation($"Config parameter '{configName}' not provided or is not a valid absolute URI.");
                }
            }

            return uri;
        }
    }
}
