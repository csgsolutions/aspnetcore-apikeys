using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.ApiKeyAuthentication.Tests
{
    public class FakeOptionsMonitor<T> : Microsoft.Extensions.Options.IOptionsMonitor<T>
    {
        public FakeOptionsMonitor()
        {

        }

        public FakeOptionsMonitor(T options)
        {
            this.CurrentValue = options;
        }

        public T CurrentValue { get; set; }

        public T Get(string name)
        {
            return this.CurrentValue;
        }

        public IDisposable OnChange(Action<T, string> listener)
        {
            throw new NotImplementedException();
        }
    }
}
