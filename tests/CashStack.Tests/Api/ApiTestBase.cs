using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace CashStack.Tests.Api;

// One API instance (and database) per test, so tests never depend on each other
public abstract class ApiTestBase : IDisposable
{
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected CashStackApiFactory Factory { get; } = new();
    protected HttpClient Client { get; }

    protected ApiTestBase()
    {
        Client = Factory.CreateClient();
    }

    protected Task<HttpResponseMessage> PostAsync(string url, object body) => Client.PostAsJsonAsync(url, body, Json);

    protected static async Task<T> ReadAsync<T>(HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<T>(Json))!;

    protected static Task<ProblemDetails> ReadProblemAsync(HttpResponseMessage response) =>
        ReadAsync<ProblemDetails>(response);

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }
}
