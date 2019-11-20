# Introduction 
A server-side and client-side library for API key authentication with ASP.NET core.

[![Build status](https://ci.appveyor.com/api/projects/status/mrrmn9glibm3484n/branch/master?svg=true)](https://ci.appveyor.com/project/jusbuc2k/aspnetcore-apikeys/branch/master)

# Packages 

| Package | NuGet Stable | MyGet Pre-release | MyGet Dev |
| ------- | ------------ | ----------------- | --------- |
| Csg.AspNetCore.Authentication.ApiKey | [Link](https://www.nuget.org/packages/Csg.AspNetCore.Authentication.ApiKey/) | [Link](https://www.myget.org/feed/csgsolutions/package/nuget/Csg.AspNetCore.Authentication.ApiKey) | [Link](https://www.myget.org/feed/csgsolutions-dev/package/nuget/Csg.AspNetCore.Authentication.ApiKey) |
| Csg.ApiKeyGenerator | [Link](https://www.nuget.org/packages/Csg.ApiKeyGenerator/) | [Link](https://www.myget.org/feed/csgsolutions/package/nuget/Csg.ApiKeyGenerator) | [Link](https://www.myget.org/feed/csgsolutions-dev/package/nuget/Csg.ApiKeyGenerator) |

# Getting Started
1.	Install nuget package
2.	Configure API for Authentication
3.	Enjoy!

# API Configuration

See the [full example API project](src/ExampleAPI) with a working Open API (Swagger) definition as well.

appsettings.json example
```json
{
  "ApiKeys": {
	"Keys": {
		"Client1": "secret1234"
	}
  }
}
```

Startup.cs example
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<Csg.AspNetCore.Authentication.ApiKey.ConfigurationApiKeyStoreOptions>(this.Configuration.GetSection("ApiKeys"));

    services.AddConfigurationApiKeyStore();

    services.AddAuthentication(Csg.AspNetCore.Authentication.ApiKey.ApiKeyDefaults.Name).AddApiKey();

    services.AddMvc();
}
```

Remember to require authentication on your controller, or [require auth for all requests](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/secure-data?view=aspnetcore-2.1#require-authenticated-users)
```csharp
[Authorize]
public class EchoController : Controller
```

API ClientID/Secret pairs can be passed in the Authorization Header via Basic or ApiKey/TApiKey authentication, a custom header, or via the query string.

## Authorization Header Format
```
Authorization: Basic <base64 http basic auth string>
Authorization: ApiKey ClientID:ClientSecret
Authorization: TApiKey ClientID:<time-based-token>
```

## Custom Header Format
```
ApiKey: ApiKey ClientID:ClientSecret
ApiKey: TApiKey ClientID:<time-based-token>
```

## QueryString Format
```
https://example.com/api/widget/?_apikey=ClientID:ClientSecret
```

## Configuration Options

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication(Csg.AspNetCore.Authentication.ApiKey.ApiKeyDefaults.Name)
        .AddApiKey(conf => {
            // enable or disable static keys
            conf.StaticKeyEnabled = true;

            // enable or disable time-based tokens
            conf.TimeBasedKeyEnabled = true;

            // enable or disable HTTP Basic Authentication
            conf.HttpBasicEnabled = true;
            
            // Change the custom header name
            conf.HeaderName = "HeaderName";
            
            // Disable the custom header
            conf.HeaderName = null;

            // Change the custom query string parameter
            conf.QueryString = "param_name";

            // Disable the query string parameter
            conf.QueryString = null;

            // Override the default IApiKeyValidator
            conf.KeyValidator = new MyCustomValidator();
        });
}
```

## Using the C# Client Library

[Install the NuGet Package](https://www.nuget.org/packages/Csg.ApiKeyGenerator/).

The ```AddApiKeyAuthorizationHeader``` extension method can be used to generate static and 
time-based request headers.

See the [full example API project](src/ExampleClient) with working calls to the Example API.


```csharp
static void Main(string[] args)
{
    string clientID = "Client1";
    string secret = "secret1234";
    var client = new System.Net.Http.HttpClient();

    Console.WriteLine("Calling an API using a static API key");
    client.AddApiKeyAuthorizationHeader(clientID, secret);
    var response = client.GetAsync("http://localhost:5001/api/echo/HelloWorld").ConfigureAwait(false).GetAwaiter().GetResult();
    Console.WriteLine($"Response Code: {response.StatusCode}");
    var content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    Console.WriteLine($"Response Content: {content}");

    Console.WriteLine("Calling an API using a time-based token generated from the API key");
    client.AddApiKeyAuthorizationHeader(clientID, secret, DateTimeOffset.UtcNow);
    response = client.GetAsync("http://localhost:5001/api/echo/HelloWorld").ConfigureAwait(false).GetAwaiter().GetResult();
    Console.WriteLine($"Response Code: {response.StatusCode}");
    content = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    Console.WriteLine($"Response Content: {content}");

    Console.WriteLine("Press any key to end...");
    Console.ReadKey();
}
```

# Time-Based Token
The time-based token provides the most basic of replay-protection.  It prevents requests that may 
have been captured from being replayed after a short time has elapsed. This is not a robus
or exhaustive replay protection and is still really only designed with internal (behind a firewall)
use in mind.

The token is, by default, an HMAC 256 hash of the clientID, using a key
derived using PBKDF2/RFC2898 with the secret as the password, and the number of 60 second
intervals since epoch (1970-01-01) as the counter. The hash is then URL-safe Base64 encoded.

```
counter = epoch_seconds / 60
key = PBKDF2(secret, counter)
token = BASE64URL(HMAC256(key, clientID))
```

This method be changed with a custom algorithm by implementing
```IKeyValidator``` and setting the ```KeyValidator``` option in startup.

# Build and Test
 1. build.ps1 / build.cmd will build and run all tests

