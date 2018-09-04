using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.Authentication.ApiKey.Tests
{
    public class MockClock : Microsoft.AspNetCore.Authentication.ISystemClock
    {
        private readonly DateTimeOffset _utcNow;

        public MockClock(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public DateTimeOffset UtcNow => _utcNow;
    }
}
