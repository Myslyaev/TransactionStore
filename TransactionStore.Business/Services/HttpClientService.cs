using RestSharp;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TransactionStore.Business.Services;

public class HttpClientService : IHttpClientService
{
    public async Task<T> Get<T>(string urlForRequest, CancellationToken cancellationToken)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };

        var options = new RestClientOptions()
        {
            ConfigureMessageHandler = _ => handler,
            Timeout = TimeSpan.FromSeconds(380)
        };

        var client = new RestClient(options);
        var request = new RestRequest(urlForRequest);
        var response = await client.ExecuteAsync(request, Method.Get, cancellationToken);

        if (!response.IsSuccessful)
        {
            throw new HttpRequestException($"Request failed with status code {response.StatusCode} and message: {response.ErrorMessage}, Content: {response.Content}");
        }

        try
        {
            var result = JsonSerializer.Deserialize<T>(response.Content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });

            return result == null ? throw new HttpRequestException("Deserialized response is null.") : result;
        }
        catch (JsonException ex)
        {
            throw new HttpRequestException("Failed to deserialize response", ex);
        }
    }
}