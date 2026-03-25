using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using StyleSystem.Api.Abstractions;

namespace StyleSystem.Api.Services;

public class CloudflareAiService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<CloudflareAiService> logger) : IImageAiService
{
    private readonly string model = configuration["Cloudflare:Model"]
        ?? "@cf/stabilityai/stable-diffusion-xl-base-1.0";
    private readonly string apiKey   = configuration["Cloudflare:ApiKey"]!;
    private readonly string accountId = configuration["Cloudflare:AccountId"]!;

    public async Task<List<byte[]>> GenerateImagesAsync(
        string prompt,
        int count = 2,
        CancellationToken cancellationToken = default)
    {
        var tasks = Enumerable.Range(1, count)
            .Select(seed => GenerateSingleImageAsync(prompt, seed, cancellationToken))
            .ToList();

        var results = await Task.WhenAll(tasks);
        return results.ToList();
    }

    private async Task<byte[]> GenerateSingleImageAsync(
        string prompt,
        int seed,
        CancellationToken cancellationToken)
    {
        var url = $"https://api.cloudflare.com/client/v4/accounts/{accountId}/ai/run/{model}";

        var requestBody = new
        {
            prompt          = prompt,
            negative_prompt = "blurry, low quality, distorted, deformed, cropped, unrealistic",
            num_steps       = 20,
            guidance_scale  = 7.5,
            height          = 512,
            width           = 512,
            seed            = seed
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new Exception($"Cloudflare error {response.StatusCode} | Seed: {seed} | {errorContent}");
        }

        var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
        logger.LogInformation("Cloudflare response ContentType: {ContentType}", contentType);

        // Cloudflare SDXL ikki xil format qaytarishi mumkin:
        // 1. image/png  — to'g'ridan-to'g'ri binary bytes (eng keng tarqalgan)
        // 2. application/json — { "result": { "image": "<base64>" } }

        if (contentType.Contains("image/", StringComparison.OrdinalIgnoreCase))
        {
            // To'g'ridan-to'g'ri PNG binary
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            logger.LogInformation("Image received as binary, size: {Size} bytes", bytes.Length);
            return bytes;
        }
        else
        {
            // JSON wrapper — base64 decode qilish kerak
            var jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogInformation("Image received as JSON, length: {Len}", jsonString.Length);

            return ExtractImageBytesFromJson(jsonString);
        }
    }

    private byte[] ExtractImageBytesFromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Cloudflare format: { "result": { "image": "base64..." } }
        if (root.TryGetProperty("result", out var result))
        {
            if (result.TryGetProperty("image", out var imageEl))
            {
                var base64 = imageEl.GetString()
                    ?? throw new Exception("'image' field is null in Cloudflare response");

                // data:image/png;base64,xxx — prefiks bo'lsa olib tashlaymiz
                var cleanBase64 = base64.Contains(',')
                    ? base64.Split(',')[1]
                    : base64;

                return Convert.FromBase64String(cleanBase64);
            }
        }

        // Fallback: butun response binary bo'lishi mumkin edi,
        // lekin JSON deb o'qidik — raw bytes sifatida qaytaramiz
        logger.LogWarning("Could not find 'result.image' in JSON. Raw JSON: {Json}", json[..Math.Min(200, json.Length)]);
        throw new Exception($"Unexpected Cloudflare response format. First 200 chars: {json[..Math.Min(200, json.Length)]}");
    }
}