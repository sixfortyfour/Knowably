using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using RagQnA.Contracts.Interfaces;
using RagQnA.Contracts.Models;
using RagQnA.Contracts.Options;

namespace RagQnA.Infrastructure.Clients;

public sealed class QStashClient : IQStashClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public QStashClient(HttpClient http, IOptions<QStashOptions> options)
    {
        _http = http;
        _http.BaseAddress = new Uri("https://qstash.upstash.io/v2/");
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", options.Value.Token);
    }

    public async Task<string> PublishAsync(string destinationUrl, object body, QStashPublishOptions? options = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"publish/{Uri.EscapeDataString(destinationUrl)}")
        {
            Content = JsonContent.Create(body, options: JsonOptions)
        };

        if (options?.Retries.HasValue == true)
            request.Headers.Add("Upstash-Retries", options.Retries.Value.ToString());
        if (options?.Delay.HasValue == true)
            request.Headers.Add("Upstash-Delay", $"{(long)options.Delay.Value.TotalSeconds}s");
        if (options?.DeduplicationId is not null)
            request.Headers.Add("Upstash-Deduplication-Id", options.DeduplicationId);

        var response = await _http.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"QStash publish failed ({response.StatusCode}): {responseBody}");

        using var doc = JsonDocument.Parse(responseBody);
        return doc.RootElement.GetProperty("messageId").GetString()!;
    }

    public async Task<QStashMessage> GetMessageAsync(string messageId)
    {
        var response = await _http.GetAsync($"messages/{messageId}");
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"QStash getmessage failed ({response.StatusCode}): {body}");

        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;

        return new QStashMessage
        {
            MessageId = root.TryGetProperty("messageId", out var mid) ? mid.GetString() ?? "" : "",
            State = root.TryGetProperty("state", out var state) ? state.GetString() ?? "" : "",
            Url = root.TryGetProperty("url", out var url) ? url.GetString() ?? "" : "",
            DeliveredCount = root.TryGetProperty("deliveredCount", out var dc) ? dc.GetInt32() : 0,
            CreatedAt = root.TryGetProperty("createdAt", out var ca)
                ? DateTimeOffset.FromUnixTimeMilliseconds(ca.GetInt64())
                : default
        };
    }
}
