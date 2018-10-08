# Introduction 
A server-side and client-side library for API key authentication with ASP.NET core.

[![Build status](https://ci.appveyor.com/api/projects/status/mrrmn9glibm3484n/branch/master?svg=true)](https://ci.appveyor.com/project/jusbuc2k/aspnetcore-apikeys/branch/master)

# Packages 

| Package | NuGet Stable | NuGet Pre-release | MyGet Dev |
| ------- | ------------ | ----------------- | --------- |
| Csg.AspNetCore.Authentication.ApiKey | n/a | n/a | [Link](https://www.myget.org/feed/csgsolutions-dev/package/nuget/Csg.AspNetCore.Authentication.ApiKey) |
| Csg.ApiKeyGenerator | n/a | n/a | [Link](https://www.myget.org/feed/csgsolutions-dev/package/nuget/Csg.ApiKeyGenerator) |


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
    "Client1": "secret1234"
  }
}
```

Startup.cs example
```csharp
public void ConfigureServices(IServiceCollection services)
{

    services.Configure<Csg.AspNetCore.Authentication.ApiKey.ConfigurationApiKeyStoreOptions>("ApiKeys", this.Configuration);

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

# Example Client Usage 

See the [full example API project](src/ExampleClient) with working calls to the Example API.

```csharp
static void Main(string[] args)
{
    string clientID = "Client1";
    string secret = "secret1234";
    var client = new System.Net.Http.HttpClient();

    Console.WriteLine("Calling an API using a static API key");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("ApiKey", clientID+":"+secret);
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
# Build and Test
 1. build.ps1 / build.cmd should build and run all tests
