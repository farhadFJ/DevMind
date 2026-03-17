using System.Text;
using System.Text.Json;
using DevMind.Api.Configurations;
using Microsoft.Extensions.Options;

namespace DevMind.Api.Services;

public class OllamaService
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaService(HttpClient httpClient, IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<string> GenerateReplyAsync(string prompt)
    {
        var requestBody = new
        {
            model = _options.Model,
            prompt = prompt,
            stream = false
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("/api/generate", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseJson);

        if (doc.RootElement.TryGetProperty("response", out var responseElement))
        {
            return responseElement.GetString() ?? "No response from model.";
        }

        return "No response from model.";
    }
}