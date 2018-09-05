using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Csg.AspNetCore.Authentication.ApiKey.Tests
{
    [TestClass]
    public class ApiKeyHandlerTests
    {
        private const string ValidHeaderValue = "key app_name:key_value";

        private ApiKeyHandler CreateHandler(HttpContext context)
        {
            var keyStore = new FakeKeyStore();
            var options = new FakeOptionsMonitor<ApiKeyOptions>() { CurrentValue = new ApiKeyOptions() };
            var logger = new FakeLoggerFactory();
            var clock = new Microsoft.AspNetCore.Authentication.SystemClock();

            var b = new Microsoft.AspNetCore.Authentication.AuthenticationSchemeBuilder(ApiKeyDefaults.Name);
            b.HandlerType = typeof(ApiKeyHandler);
            b.DisplayName = "API Key";
            
            var handler = new ApiKeyHandler(keyStore, options, logger, System.Text.Encodings.Web.UrlEncoder.Default, clock);

            handler.InitializeAsync(b.Build(), context).ConfigureAwait(false).GetAwaiter().GetResult();

            return handler;
        }

        [TestMethod]
        public void ApiKeyHandler_HandleRequestWithNoToken()
        {
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var handler = CreateHandler(context);

            var authResult = handler.AuthenticateAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            
            Assert.AreEqual(false, authResult.Succeeded);
            Assert.AreEqual(true, authResult.None);
        }

        [TestMethod]
        public void ApiKeyHandler_HandleRequestWithMalformedHeader()
        {
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var handler = CreateHandler(context);

            context.Request.Headers.Add("Authorization", "TestNameMissingType");
            var authResult = handler.AuthenticateAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            Assert.AreEqual(false, authResult.Succeeded);
            Assert.AreEqual(true, authResult.None);
        }

        [TestMethod]
        public void ApiKeyHandler_HandleRequestWithMalformedToken()
        {
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var handler = CreateHandler(context);

            context.Request.Headers.Add("Authorization", "ApiKey TestNameMissingClient");
            var authResult = handler.AuthenticateAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            Assert.AreEqual(false, authResult.Succeeded);
            Assert.AreEqual(false, authResult.None);
            Assert.IsNotNull(authResult.Failure);
        }

        [TestMethod]
        public void ApiKeyHandler_HandleRequestWithInvalidToken()
        {
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var handler = CreateHandler(context);

            context.Request.Headers.Add("Authorization", "ApiKey TestName:NotValidValue");

            var authResult = handler.AuthenticateAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            Assert.AreEqual(false, authResult.Succeeded);
            Assert.AreEqual(false, authResult.None);
            Assert.IsNotNull(authResult.Failure);
        }

        [TestMethod]
        public void ApiKeyHandler_HandleRequestWithValidStaticToken()
        {
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var handler = CreateHandler(context);

            context.Request.Headers.Add("Authorization", "ApiKey TestName:TestKey");

            var authResult = handler.AuthenticateAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            Assert.AreEqual(true, authResult.Succeeded);
            Assert.AreEqual(true, authResult.Principal.Identity.IsAuthenticated);
            Assert.AreEqual("TestName", authResult.Principal.Identity.Name);
        }

        [TestMethod]
        public void ApiKeyHandler_HandleRequestWithValidTimeBasedToken()
        {
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var handler = CreateHandler(context);

            var gen = new Csg.AspNetCore.Authentication.ApiKey.TimeBasedTokenGenerator();

            var token = System.Net.WebUtility.UrlEncode(gen.GenerateToken("TestName", "TestKey", DateTimeOffset.UtcNow));

            context.Request.Headers.Add("Authorization", $"TApiKey TestName:{token}");

            var authResult = handler.AuthenticateAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            Assert.AreEqual(true, authResult.Succeeded);
            Assert.AreEqual(true, authResult.Principal.Identity.IsAuthenticated);
            Assert.AreEqual("TestName", authResult.Principal.Identity.Name);
        }

        //[TestMethod]
        //public void ApiKeyHandler_HandleRequestWithValidTokenFromAlternateHeader()
        //{
        //    var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        //    var handler = CreateHandler(context);

        //    handler.Options.HeaderName = "API-Key";

        //    context.Request.Headers.Add("API-Key", "TestName:TestKey");

        //    var authResult = handler.AuthenticateAsync().ConfigureAwait(false).GetAwaiter().GetResult();

        //    Assert.AreEqual(true, authResult.Succeeded);
        //    Assert.AreEqual(true, authResult.Principal.Identity.IsAuthenticated);
        //    Assert.AreEqual("TestName", authResult.Principal.Identity.Name);
        //}

        [TestMethod]
        public void ApiKeyHandler_HandleRequestWithCustomTokenProvider()
        {
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var handler = CreateHandler(context);

            handler.Options.Events.OnRequestAsync = async (ctx) =>
            {
                ctx.AuthenticationType = "ApiKey";
                ctx.ClientID = "TestName";
                ctx.Token = "TestKey";
            };

            var authResult = handler.AuthenticateAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            Assert.AreEqual(true, authResult.Succeeded);
            Assert.AreEqual(true, authResult.Principal.Identity.IsAuthenticated);
            Assert.AreEqual("TestName", authResult.Principal.Identity.Name);
        }


    }
}
