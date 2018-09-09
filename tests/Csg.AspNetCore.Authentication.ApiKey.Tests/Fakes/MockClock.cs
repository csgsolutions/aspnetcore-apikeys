using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.Authentication.ApiKey.Tests
{
    public class MockClock : Microsoft.AspNetCore.Authentication.ISystemClock
    {
        public MockClock()
        {
            this.UtcNow = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset UtcNow { get; set; }
    }
}
