using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    public class ApiKeyEvents
    {
        public Func<RequestMessageContext, Task> OnRequestAsync { get; set; } = context => Task.CompletedTask;

        //public Func<AuthenticatedContext, Task> OnAuthenticatedAsync { get; set; } = context => Task.CompletedTask;
    }

    //public class AuthenticatedContext : Microsoft.AspNetCore.Authentication.RemoteAuthenticationContext<ApiKeyOptions>
    //{

    //}

    //public class KeyValidationContext : Microsoft.AspNetCore.Authentication.RemoteAuthenticationContext<ApiKeyOptions>
    //{
    //    public KeyValidationContext() : base()
    //    {
    //    }

    //    public ApiKey KeyFromStore { get; set; }

    //    public ApiKey ProvidedKey { get; set; }
    //}
}