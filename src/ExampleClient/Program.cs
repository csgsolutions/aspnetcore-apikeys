using System;
using System.Net.Http;

namespace ExampleClient
{
    class Program
    {
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
    }
}
