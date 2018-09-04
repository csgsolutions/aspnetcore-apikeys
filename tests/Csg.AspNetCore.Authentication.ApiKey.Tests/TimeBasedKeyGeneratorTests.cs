using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Csg.AspNetCore.Authentication.ApiKey.Tests
{
    [TestClass]
    public class TimeBasedKeyGeneratorTests
    {
        [TestMethod]
        public void TimeBasedKeyGenerator_GenerateTokenDefaults()
        {
            string client = "abc";
            string secret = "123";
            string expectedValue = "AOQKS6XgnqDV5lAMvxHAeDwQ6xmJxhdM7OffZ9iFIz5L";

            var gen = new Csg.AspNetCore.Authentication.ApiKey.TimeBasedTokenGenerator();
            var now = new DateTimeOffset(2015, 09, 25, 0, 0, 0,TimeSpan.Zero);
            var token = gen.GenerateToken(client, secret, now);

            Assert.AreEqual(expectedValue, token);
            Assert.IsTrue(gen.ValidateToken(client, secret, token, now));
        }

        [TestMethod]
        public void TimeBasedKeyGenerator_ValidateTokenDefaultDriftTolerance()
        {
            string client = "abc";
            string secret = "123";
            string expectedValue = "AOQKS6XgnqDV5lAMvxHAeDwQ6xmJxhdM7OffZ9iFIz5L";

            var gen = new Csg.AspNetCore.Authentication.ApiKey.TimeBasedTokenGenerator();

            var now = new DateTimeOffset(2015, 09, 25, 0, 0, 0, TimeSpan.Zero);
            var token = gen.GenerateToken(client, secret, now);

            Assert.AreEqual(expectedValue, token);
            Assert.IsFalse(gen.ValidateToken(client, secret, token, now.AddMinutes(-3)));
            Assert.IsTrue(gen.ValidateToken(client, secret, token, now.AddMinutes(-2)));
            Assert.IsTrue(gen.ValidateToken(client, secret, token, now.AddMinutes(-1)));
            Assert.IsTrue(gen.ValidateToken(client, secret, token, now.AddMinutes(0)));
            Assert.IsTrue(gen.ValidateToken(client, secret, token, now.AddMinutes(1)));
            Assert.IsTrue(gen.ValidateToken(client, secret, token, now.AddMinutes(2)));
            Assert.IsFalse(gen.ValidateToken(client, secret, token, now.AddMinutes(3)));
        }


    }
}
