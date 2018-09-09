using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Csg.AspNetCore.Authentication.ApiKey.Tests
{
    [TestClass]
    public class ApiKeyHandlerTests
    {
        private const string HEADER = "Authorization";
        private const string ValidHeaderValue = "key app_name:key_value";
        private static readonly MockClock Clock = new MockClock() { UtcNow = new DateTimeOffset(2015, 09, 25, 00, 00, 00, TimeSpan.Zero) };

        private ApiKeyHandler CreateHandler(HttpContext context)
        {
            var keyStore = new FakeKeyStore();
            var options = new FakeOptionsMonitor<ApiKeyOptions>() { CurrentValue = new ApiKeyOptions() };
            var logger = new FakeLoggerFactory();

            var b = new Microsoft.AspNetCore.Authentication.AuthenticationSchemeBuilder(ApiKeyDefaults.Name);
            b.HandlerType = typeof(ApiKeyHandler);
            b.DisplayName = "API Key";
            
            var handler = new ApiKeyHandler(keyStore, options, logger, System.Text.Encodings.Web.UrlEncoder.Default, Clock);

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

            context.Request.Headers.Add(HEADER, "AuthTypeNotHandled");
            var authResult = handler.AuthenticateAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            Assert.AreEqual(false, authResult.Succeeded);
            Assert.AreEqual(true, authResult.None);
        }

        [TestMethod]
        public void ApiKeyHandler_HandleRequestWithMalformedToken()
        {
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var handler = CreateHandler(context);

            context.Request.Headers.Add(HEADER, "APIKEY ClienIDWithoutKey");
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

            context.Request.Headers.Add(HEADER, "ApiKey ClientID:InvalidSecret");

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
            var gen = new Csg.ApiKeyGenerator.TimeBasedTokenGenerator();

            string token = Microsoft.AspNetCore.WebUtilities.Base64UrlTextEncoder.Encode(gen.ComputeToken("TestName", "TestKey", Clock.UtcNow));

            context.Request.Headers.Add("Authorization", $"TAPIKEY TestName:{token}");

            var authResult = handler.AuthenticateAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            Assert.AreEqual(true, authResult.Succeeded);
            Assert.AreEqual(true, authResult.Principal.Identity.IsAuthenticated);
            Assert.AreEqual("TestName", authResult.Principal.Identity.Name);
        }

        [TestMethod]
        public void ApiKeyHandler_HandleRequestWithOutOfRangeTimeBasedToken()
        {
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var handler = CreateHandler(context);
            var gen = new Csg.ApiKeyGenerator.TimeBasedTokenGenerator();

            string token = Microsoft.AspNetCore.WebUtilities.Base64UrlTextEncoder.Encode(gen.ComputeToken("TestName", "TestKey", Clock.UtcNow.AddSeconds(120)));

            context.Request.Headers.Add("Authorization", $"TAPIKEY TestName:{token}");

            var authResult = handler.AuthenticateAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            Assert.AreEqual(false, authResult.Succeeded);
            Assert.IsNull(authResult.Principal);
        }

        [TestMethod]
        public void ApiKeyHandler_OnRequestEvent()
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

        [TestMethod]
        public void ApiKeyHandler_AuthenticatedEvent()
        {
            var context = new Microsoft.AspNetCore.Http.DefaultHttpContext();
            var handler = CreateHandler(context);

            handler.Options.Events.OnAuthenticatedAsync = async (ctx) =>
            {
                ctx.Identity.AddClaim(new System.Security.Claims.Claim("Foo", "Bar"));
            };

            context.Request.Headers.Add("Authorization", "ApiKey TestName:TestKey");

            var authResult = handler.AuthenticateAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            Assert.IsTrue(authResult.Principal.HasClaim(x => x.Type == "Foo"));
        }

    }
}
