# Introduction 
A server-side and client-side library for API key authentication with ASP.NET core.

[![Build status](https://ci.appveyor.com/api/projects/status/mrrmn9glibm3484n/branch/master?svg=true)](https://ci.appveyor.com/project/jusbuc2k/aspnetcore-apikeys/branch/master)

# Packages 

| Package | NuGet Stable | NuGet Pre-release | MyGet Dev |
| ------- | ------------ | ----------------- | --------- |
| Csg.AspNetCore.Authentication.ApiKey | n/a | n/a | [Link](https://www.myget.org/feed/csgsolutions-dev/package/nuget/Csg.AspNetCore.Authentication.ApiKey) |

# Getting Started
1.	Install nuget package
2.	Configure 
3.	Enjoy!

# Configuration

```csharp
# Startup.cs Excerpt 
public void ConfigureServices(IServiceCollection services)
{

    services.Configure<Csg.AspNetCore.Authentication.ApiKey.ConfigurationApiKeyStoreOptions>("ApiKeys", this.Configuration);

    services.AddConfigurationApiKeyStore();

    services.AddAuthentication(Csg.AspNetCore.Authentication.ApiKey.ApiKeyDefaults.Name).AddApiKey();

    services.AddMvc();
}
```

# Example Project
See the [full example API project](src/ExampleAPI) with a working Open API (Swagger) definition as well.

# Build and Test
TODO: Describe and show how to build your code and run the tests. 

# Contribute
TODO: Explain how other users and developers can contribute to make your code better. 

If you want to learn more about creating good readme files then refer the following [guidelines](https://www.visualstudio.com/en-us/docs/git/create-a-readme). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)