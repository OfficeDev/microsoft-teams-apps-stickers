//----------------------------------------------------------------------------------------------
// <copyright file="MessagesHttpFunctionTests.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace StickersTemplate.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Newtonsoft.Json;
    using StickersTemplate.Interfaces;
    using StickersTemplate.Models;
    using StickersTemplate.WebModels;

    using Mutex = System.Threading.Mutex;
    using Task = System.Threading.Tasks.Task;
    using CancellationToken = System.Threading.CancellationToken;

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class MessagesHttpFunctionTests
    {
        private readonly Mutex sequentialMutex = new Mutex(true, "ForceSequentialTests");

        private readonly StickerSet stickerSet = new StickerSet("stickers", new Sticker[] {
            new Sticker("sticker1", new Uri("http://localhost"), new string[] { "sticker1" } ),
            new Sticker("sticker2", new Uri("http://localhost"), new string[] { "sticker2" } ),
            new Sticker("sticker3", new Uri("http://localhost"), new string[] { "sticker3" } ),
        });

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Run_NullRequest()
        {
            // Setup
            var logger = new Mock<ILogger>();
            var context = new ExecutionContext();

            // Action
            var result = await MessagesHttpFunction.Run(null, logger.Object, context);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Run_NullLogger()
        {
            // Setup
            var request = new Mock<HttpRequest>();
            var context = new ExecutionContext();

            // Action
            var result = await MessagesHttpFunction.Run(request.Object, null, context);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task Run_NullExecutionContext()
        {
            // Setup
            var request = new Mock<HttpRequest>();
            var logger = new Mock<ILogger>();

            // Action
            var result = await MessagesHttpFunction.Run(request.Object, logger.Object, null);
        }

        [TestMethod]
        public async Task Run_Request_MissingAllHeaders()
        {
            // Setup
            var request = new Mock<HttpRequest>();
            request.Setup((r) => r.Headers).Returns<IHeaderDictionary>(null);
            var logger = new Mock<ILogger>();
            var context = new ExecutionContext
            {
                FunctionAppDirectory = "localhost"
            };

            // Action
            var result = await MessagesHttpFunction.Run(request.Object, logger.Object, context);

            // Validation
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task Run_Request_MissingAuthHeader1()
        {
            // Setup
            var request = new Mock<HttpRequest>();
            var headers = new Mock<IHeaderDictionary>();
            request.Setup((r) => r.Headers).Returns(headers.Object);
            var logger = new Mock<ILogger>();
            var context = new ExecutionContext
            {
                FunctionAppDirectory = "localhost"
            };

            // Action
            var result = await MessagesHttpFunction.Run(request.Object, logger.Object, context);

            // Validation
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task Run_Request_MissingAuthHeader2()
        {
            // Setup
            var request = new Mock<HttpRequest>();
            var headers = new Mock<IHeaderDictionary>();
            StringValues authHeaders = new StringValues();
            headers.Setup((h) => h.TryGetValue(It.Is<string>((s) => "Authorization".Equals(s)), out authHeaders)).Returns(true);
            request.Setup((r) => r.Headers).Returns(headers.Object);
            var logger = new Mock<ILogger>();
            var context = new ExecutionContext
            {
                FunctionAppDirectory = "localhost"
            };

            // Action
            var result = await MessagesHttpFunction.Run(request.Object, logger.Object, context);

            // Validation
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public async Task Run_Request_MissingBody()
        {
            // Setup
            var request = new Mock<HttpRequest>();
            var headers = new Mock<IHeaderDictionary>();
            StringValues authHeaders = new StringValues("Bearer adsf");
            headers.Setup((h) => h.TryGetValue(It.Is<string>((s) => "Authorization".Equals(s)), out authHeaders)).Returns(true);
            request.Setup((r) => r.Headers).Returns(headers.Object);
            var logger = new Mock<ILogger>();
            var context = new ExecutionContext
            {
                FunctionAppDirectory = "localhost"
            };

            // Action
            var result = await MessagesHttpFunction.Run(request.Object, logger.Object, context);

            // Validation
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(UnauthorizedResult));
        }

        [TestMethod]
        public async Task Run_Request_Body_NotJSON()
        {
            // Setup
            var request = new Mock<HttpRequest>();
            var headers = new Mock<IHeaderDictionary>();
            StringValues authHeaders = new StringValues("");
            headers.Setup((h) => h.TryGetValue(It.Is<string>((s) => "Authorization".Equals(s)), out authHeaders)).Returns(true);
            request.Setup((r) => r.Headers).Returns(headers.Object);
            var body = new MemoryStream();
            request.Setup((r) => r.Body).Returns(body);

            var logger = new Mock<ILogger>();
            var context = new ExecutionContext
            {
                FunctionAppDirectory = "localhost"
            };

            // Action
            var result = await MessagesHttpFunction.Run(request.Object, logger.Object, context);

            // Validation
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestResult));
        }

        [TestMethod]
        public async Task Run_Request_Body_NotComposeExtensionQuery1()
        {
            // Setup
            var request = new Mock<HttpRequest>();
            var headers = new Mock<IHeaderDictionary>();
            StringValues authHeaders = new StringValues("");
            headers.Setup((h) => h.TryGetValue(It.Is<string>((s) => "Authorization".Equals(s)), out authHeaders)).Returns(true);
            request.Setup((r) => r.Headers).Returns(headers.Object);

            var activity = new Activity()
            {
                Type = ActivityTypes.Message
            };
            var body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activity)));
            request.Setup((r) => r.Body).Returns(body);

            var logger = new Mock<ILogger>();
            var context = new ExecutionContext
            {
                FunctionAppDirectory = "localhost"
            };

            // Action
            var result = await MessagesHttpFunction.Run(request.Object, logger.Object, context);

            // Validation
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Run_Request_Body_NoValue()
        {
            // Setup
            var request = new Mock<HttpRequest>();
            var headers = new Mock<IHeaderDictionary>();
            StringValues authHeaders = new StringValues("");
            headers.Setup((h) => h.TryGetValue(It.Is<string>((s) => "Authorization".Equals(s)), out authHeaders)).Returns(true);
            request.Setup((r) => r.Headers).Returns(headers.Object);

            var activity = new Activity()
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/query"
            };
            var body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activity)));
            request.Setup((r) => r.Body).Returns(body);

            var logger = new Mock<ILogger>();
            var context = new ExecutionContext
            {
                FunctionAppDirectory = "localhost"
            };

            // Action
            var result = await MessagesHttpFunction.Run(request.Object, logger.Object, context);

            // Validation
            Assert.IsNotNull(result);
            ValidateResponse(result, stickerSet.Count());
        }

        [TestMethod]
        public async Task Run_Request_Body_QueryValue_NoResults()
        {
            // Setup
            var request = new Mock<HttpRequest>();
            var headers = new Mock<IHeaderDictionary>();
            StringValues authHeaders = new StringValues("");
            headers.Setup((h) => h.TryGetValue(It.Is<string>((s) => "Authorization".Equals(s)), out authHeaders)).Returns(true);
            request.Setup((r) => r.Headers).Returns(headers.Object);

            var activity = new Activity()
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/query",
                Value = new ComposeExtensionValue
                {
                    CommandId = "commandId",
                    Parameters = new ComposeExtensionParameter[]
                    {
                        new ComposeExtensionParameter
                        {
                            Name = "search",
                            Value = "search"
                        }
                    },
                    QueryOptions = new ComposeExtensionQueryOptions
                    {
                        Count = 10,
                        Skip = 0
                    }
                }
            };
            var body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activity)));
            request.Setup((r) => r.Body).Returns(body);

            var logger = new Mock<ILogger>();
            var context = new ExecutionContext
            {
                FunctionAppDirectory = "localhost"
            };

            // Action
            var result = await MessagesHttpFunction.Run(request.Object, logger.Object, context);

            // Validation
            Assert.IsNotNull(result);
            ValidateResponse(result, 0);
        }

        [TestMethod]
        public async Task Run_Request_Body_QueryValue_WithResults()
        {
            // Setup
            var request = new Mock<HttpRequest>();
            var headers = new Mock<IHeaderDictionary>();
            StringValues authHeaders = new StringValues("");
            headers.Setup((h) => h.TryGetValue(It.Is<string>((s) => "Authorization".Equals(s)), out authHeaders)).Returns(true);
            request.Setup((r) => r.Headers).Returns(headers.Object);

            var activity = new Activity()
            {
                Type = ActivityTypes.Invoke,
                Name = "composeExtension/query",
                Value = new ComposeExtensionValue
                {
                    CommandId = "commandId",
                    Parameters = new ComposeExtensionParameter[]
                    {
                        new ComposeExtensionParameter
                        {
                            Name = "search",
                            Value = stickerSet.First().Name
                        }
                    },
                    QueryOptions = new ComposeExtensionQueryOptions
                    {
                        Count = 10,
                        Skip = 0
                    }
                }
            };
            var body = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(activity)));
            request.Setup((r) => r.Body).Returns(body);

            var logger = new Mock<ILogger>();
            var context = new ExecutionContext
            {
                FunctionAppDirectory = "localhost"
            };

            // Action
            var result = await MessagesHttpFunction.Run(request.Object, logger.Object, context);

            // Validation
            Assert.IsNotNull(result);
            ValidateResponse(result, 1);
        }

        private void ValidateResponse(IActionResult result, int numExpectedAttachments)
        {
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var response = okResult.Value;

            Assert.IsNotNull(response);
            Assert.IsInstanceOfType(response, typeof(ComposeExtensionResponse));
            var composeExtensionResponse = response as ComposeExtensionResponse;
            var composeExtensionResult = composeExtensionResponse.ComposeExtensionResult;

            Assert.IsNotNull(composeExtensionResult);
            Assert.AreEqual(composeExtensionResult.Type, "result");
            Assert.AreEqual(composeExtensionResult.AttachmentLayout, "grid");
            Assert.IsNotNull(composeExtensionResult.Attachments);
            Assert.AreEqual(numExpectedAttachments, composeExtensionResult.Attachments.Length);
        }


        [TestInitialize]
        public void Initialize()
        {
            sequentialMutex.WaitOne(TimeSpan.FromSeconds(1));

            var settings = new Mock<ISettings>();
            settings.Setup((s) => s.ConfigUri).Returns(new Uri("http://localhost"));
            settings.Setup((s) => s.MicrosoftAppId).Returns(Guid.NewGuid().ToString());

            var stickerSetRepository = new Mock<IStickerSetRepository>();
            stickerSetRepository.Setup((s) => s.FetchStickerSetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(stickerSet);

            var stickerSetIndexer = new Mock<IStickerSetIndexer>();
            stickerSetIndexer.Setup((s) => s.IndexStickerSetAsync(It.IsAny<StickerSet>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            stickerSetIndexer.Setup((s) => s.FindStickersByQuery(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string query, CancellationToken token) => stickerSet.Where(s => string.IsNullOrWhiteSpace(query) || s.Name.Equals(query)));

            var credentialProvider = new Mock<ICredentialProvider>();
            credentialProvider.Setup((c) => c.IsAuthenticationDisabledAsync()).ReturnsAsync(true);
            var channelProvider = new Mock<IChannelProvider>();
            MessagesHttpFunction.ConfigureForTests(settings.Object, stickerSetRepository.Object, stickerSetIndexer.Object, credentialProvider.Object, channelProvider.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            sequentialMutex.ReleaseMutex();
        }
    }
}
