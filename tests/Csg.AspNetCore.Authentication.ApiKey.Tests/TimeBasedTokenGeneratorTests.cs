using Csg.ApiKeyGenerator;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.AspNetCore.WebUtilities;

namespace Csg.AspNetCore.Authentication.ApiKey.Tests
{
    [TestClass]
    public class TimeBasedTokenGeneratorTests
    {
        private int IntervalSeconds = 60;
        private const string ClientID = "Client1";
        private const string ClientSecret = "secret";
        private static DateTimeOffset TokenDateTime = new DateTimeOffset(2015, 09, 25, 0, 0, 00, TimeSpan.Zero);
        private const string TokenString = "isJf90AzSz6SazgnXO-NduZLXQxAbHbHMG6lo5wp-H8";
        private static readonly byte[] TokenBytes = Base64UrlTextEncoder.Decode(TokenString);
        private static readonly DateTimeOffset EPOCH = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

        [TestMethod]
        public void TimeBasedTokenGenerator_Defaults_GenerateTokenWithDefaults()
        {
            var gen = new Csg.ApiKeyGenerator.TimeBasedTokenGenerator();
            var token = Base64UrlTextEncoder.Encode(gen.ComputeToken(ClientID, ClientSecret, TokenDateTime));

            Assert.AreEqual(TokenString, token);
        }

        [TestMethod]
        public void TimeBasedTokenGenerator_Defaults_ValidateToken()
        {
            var gen = new Csg.ApiKeyGenerator.TimeBasedTokenGenerator();

            Assert.IsTrue(gen.ValidateToken(ClientID, ClientSecret, TokenBytes, TokenDateTime));
        }

        [TestMethod]
        public void TimeBasedTokenGenerator_Defaults_ValidateTokenWithDriftTolerance()
        {
            var gen = new Csg.ApiKeyGenerator.TimeBasedTokenGenerator();

            // minus 1 interval is 23:59:00
            // zero is 00:00:00

            // 00:00:00 minus 61 seconds is 23:58:59, which is interval -2
            Assert.IsFalse(gen.ValidateToken(ClientID, ClientSecret, TokenBytes, TokenDateTime.AddSeconds(-61)));

            // 00:00:00 minus 60 seconds is 23:59:00, which is in interval -1
            Assert.IsTrue(gen.ValidateToken(ClientID, ClientSecret, TokenBytes, TokenDateTime.AddSeconds(-60)));

            // 00:01:00 is within 1 interval of tolerance
            Assert.IsTrue(gen.ValidateToken(ClientID, ClientSecret, TokenBytes, TokenDateTime.AddSeconds(60)));

            // 00:00:59 is the last tick of the zero interval, 00:01:59 is the last tick of the first interval away from zero
            Assert.IsTrue(gen.ValidateToken(ClientID, ClientSecret, TokenBytes, TokenDateTime.AddSeconds(119)));

            // should fail, since it's past 00:01:59
            Assert.IsFalse(gen.ValidateToken(ClientID, ClientSecret, TokenBytes, TokenDateTime.AddSeconds(120)));

        }

        [TestMethod]
        public void TimeBasedTokenGenerator_NoTolerance_ValidateToken()
        {
            var gen = new Csg.ApiKeyGenerator.TimeBasedTokenGenerator() { AllowedNumberOfDriftIntervals = 0 };

            Assert.IsFalse(gen.ValidateToken(ClientID, ClientSecret, TokenBytes, TokenDateTime.AddSeconds(-61)));
            Assert.IsFalse(gen.ValidateToken(ClientID, ClientSecret, TokenBytes, TokenDateTime.AddSeconds(-60)));
            Assert.IsTrue(gen.ValidateToken(ClientID, ClientSecret, TokenBytes, TokenDateTime));
            Assert.IsFalse(gen.ValidateToken(ClientID, ClientSecret, TokenBytes, TokenDateTime.AddSeconds(+60)));
            Assert.IsFalse(gen.ValidateToken(ClientID, ClientSecret, TokenBytes, TokenDateTime.AddSeconds(+61)));
        }      


    }
}
