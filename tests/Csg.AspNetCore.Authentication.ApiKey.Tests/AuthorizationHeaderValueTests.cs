using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Csg.AspNetCore.Authentication.ApiKey.Tests
{
    [TestClass]
    public class AuthorizationHeaderValueTests
    {
        private const string ValidHeaderValue = "key app_name:key_value";

        [TestMethod]
        public void AuthorizationHeaderValue_TryParseAuthorizationHeader_EmptyHeaderValueThrows()
        {
            Assert.ThrowsException<System.ArgumentException>(() =>
            {
                AuthorizationHeaderValue.TryParse(new Microsoft.Extensions.Primitives.StringValues(), out AuthorizationHeaderValue foo);
            });
        }

        [TestMethod]
        public void AuthorizationHeaderValue_TryParseAuthorizationHeader_InvalidHeaderValueThrows()
        {
            Assert.ThrowsException<System.ArgumentException>(() =>
            {
                AuthorizationHeaderValue.TryParse(new Microsoft.Extensions.Primitives.StringValues(new string[] { "abc", "def" }), out AuthorizationHeaderValue foo);
            });
        }

        [TestMethod]
        public void AuthorizationHeaderValue_TryParseAuthorizationHeader_ValidHeaderValue()
        {
            var result = AuthorizationHeaderValue.TryParse(new Microsoft.Extensions.Primitives.StringValues(ValidHeaderValue), out AuthorizationHeaderValue output);

            Assert.IsTrue(result);
            Assert.AreEqual("key", output.AuthenticationType);
            Assert.AreEqual("app_name:key_value", output.Value);
        }
    }
}
