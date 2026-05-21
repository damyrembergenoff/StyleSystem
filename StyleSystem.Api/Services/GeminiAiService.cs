using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using StyleSystem.Api.Abstractions;
using StyleSystem.Api.Dtos.Recommendations;
using StyleSystem.Shared.Dtos.Recommendations;

namespace StyleSystem.Api.Services;

public class GeminiAiService(
    HttpClient httpClient,
    IConfiguration configuration,
    ILogger<GeminiAiService> logger) : ITranslateService
{
    private readonly string apiKey = configuration["Gemini:ApiKey"]
        ?? throw new ArgumentNullException("Gemini:ApiKey");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async ValueTask<AnonymousRecommendationDto> TranslateInputAsync(
    AnonymousRecommendationDto dto,
    CancellationToken cancellationToken = default)
    {
        var fieldsToTranslate = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(dto.Occasion))
            fieldsToTranslate["Occasion"] = dto.Occasion;

        if (!string.IsNullOrWhiteSpace(dto.AdditionalPreferences))
            fieldsToTranslate["AdditionalPreferences"] = dto.AdditionalPreferences;

        if (fieldsToTranslate.Count == 0)
            return dto;

        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

        var bodyJson = JsonSerializer.Serialize(new GeminiRequest
        {
            SystemInstruction = new GeminiSystemInstruction
            {
                Parts = [new GeminiPart
                {
                    Text =
                        """
                        You are a translator. The user sends a JSON object with field names and values.
                        Translate ONLY the values into English. Keep the field names exactly as they are.
                        The values may be in Karakalpak, Uzbek, or Russian — translate all of them to English.
                        Return ONLY a valid JSON object. No explanations, no markdown, no extra text.
                        """
                }]
            },
            Contents = [new GeminiContent
            {
                Role = "user",
                Parts = [new GeminiPart { Text = JsonSerializer.Serialize(fieldsToTranslate) }]
            }],
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature = 0.1,
                MaxOutputTokens = 512
            }
        }, JsonOptions);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
        };

        var response = await httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Gemini translate input error [{StatusCode}]: {ErrorContent}", (int)response.StatusCode, errorContent);
            return dto;
        }

        var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogInformation("Gemini translate input response: {RawJson}", rawJson);

        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(rawJson, JsonOptions);
        var translatedJson = geminiResponse?
            .Candidates?[0]
            .Content?
            .Parts?[0]
            .Text?
            .Trim();

        if (string.IsNullOrWhiteSpace(translatedJson))
            return dto;

        try
        {
            translatedJson = translatedJson
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var translated = JsonSerializer.Deserialize<Dictionary<string, string>>(translatedJson, JsonOptions);

            if (translated is null)
                return dto;
            
            dto.Occasion = translated.GetValueOrDefault("Occasion", dto.Occasion);
            dto.AdditionalPreferences = translated!.GetValueOrDefault("AdditionalPreferences", dto.AdditionalPreferences);

            return dto;
        }
        catch (JsonException ex)
        {
            logger.LogWarning("Failed to parse translated input JSON: {Error}", ex.Message);
            return dto;
        }
    }

    public async ValueTask<string> TranslateAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        // Paragraflar bo'yicha bo'lish
        var paragraphs = text
            .Split(["\n\n", "\r\n\r\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        // Agar qisqa bo'lsa, to'g'ridan-to'g'ri tarjima
        if (paragraphs.Count <= 1)
            return await TranslateSingleChunkAsync(text, cancellationToken);

        // Har bir paragrafni tarjima qilish
        var translatedParts = new List<string>();
        foreach (var paragraph in paragraphs)
        {
            var translated = await TranslateSingleChunkAsync(paragraph, cancellationToken);
            translatedParts.Add(translated);
        }

        return string.Join("\n\n", translatedParts);
    }

    private async ValueTask<string> TranslateSingleChunkAsync(string text, CancellationToken cancellationToken)
    {
        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

        var bodyJson = JsonSerializer.Serialize(new GeminiRequest
        {
            SystemInstruction = new GeminiSystemInstruction
            {
                Parts = [new GeminiPart
                {
                    Text =
                        """
                        Follow these strict guidelines:
                        1. Use the modern Karakalpak Latin alphabet.
                        2. Preserve the original meaning, tone, emotion, and context of the English text.
                        3. Avoid literal, word-for-word translations. Adapt idioms and phrasing so they sound completely natural to a native Karakalpak speaker.
                        4. Ensure perfect grammar, syntax, and spelling.
                        5. Output ONLY the translated Karakalpak text. Do not include greetings, explanations, notes, or any conversational filler.
                        """
                }]
            },
            Contents = [new GeminiContent
            {
                Role = "user",
                Parts = [new GeminiPart { Text = text }]
            }],
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature = 0.1,
                MaxOutputTokens = 8192  
            }
        }, JsonOptions);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
        };

        var response = await httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Gemini AI error [{StatusCode}]: {ErrorContent}", (int)response.StatusCode, errorContent);
            throw new Exception($"Translation failed [{response.StatusCode}]: {errorContent}");
        }

        var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);

        // finishReason tekshirish
        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(rawJson, JsonOptions);
        
        var candidate = geminiResponse?.Candidates?[0];
        
        if (candidate?.FinishReason == "MAX_TOKENS")
        {
            logger.LogWarning("Translation truncated due to MAX_TOKENS for text: {Text}", text[..Math.Min(100, text.Length)]);
        }

        var translated = candidate?.Content?.Parts?[0].Text?.Trim();
        return translated ?? text;
    }

    public async ValueTask<CreateRecommendationDto> TranslateRecommendationInputAsync(
    CreateRecommendationDto dto,
    CancellationToken cancellationToken = default)
    {
        var fieldsToTranslate = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(dto.Occasion))
            fieldsToTranslate["Occasion"] = dto.Occasion;

        if (!string.IsNullOrWhiteSpace(dto.AdditionalPreferences))
            fieldsToTranslate["AdditionalPreferences"] = dto.AdditionalPreferences;

        if (fieldsToTranslate.Count == 0)
            return dto;

        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

        var inputJson = JsonSerializer.Serialize(fieldsToTranslate);

        var bodyJson = JsonSerializer.Serialize(new GeminiRequest
        {
            SystemInstruction = new GeminiSystemInstruction
            {
                Parts = [new GeminiPart
                {
                    Text =
                        """
                        You are a translator. The user will send a JSON object with field names and values.
                        Translate ONLY the values into English. Keep the field names exactly as they are.
                        The values may be in Karakalpak, Uzbek, or Russian — translate all of them to English.
                        Return ONLY a valid JSON object. No explanations, no markdown, no extra text.
                        """
                }]
            },
            Contents = [new GeminiContent
            {
                Role = "user",
                Parts = [new GeminiPart { Text = inputJson }]
            }],
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature = 0.1,
                MaxOutputTokens = 512
            }
        }, JsonOptions);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(bodyJson, Encoding.UTF8, "application/json")
        };

        var response = await httpClient.SendAsync(httpRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogError("Gemini translate input error [{StatusCode}]: {ErrorContent}", (int)response.StatusCode, errorContent);
            return dto; // xato bo'lsa original dto ni qaytaramiz
        }

        var rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        logger.LogInformation("Gemini translate input response: {RawJson}", rawJson);

        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(rawJson, JsonOptions);
        var translatedJson = geminiResponse?
            .Candidates?[0]
            .Content?
            .Parts?[0]
            .Text?
            .Trim();

        if (string.IsNullOrWhiteSpace(translatedJson))
            return dto;

        try
        {
            // markdown code block bo'lsa tozalash
            translatedJson = translatedJson
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var translated = JsonSerializer.Deserialize<Dictionary<string, string>>(translatedJson, JsonOptions);

            if (translated == null)
                return dto;
            
            dto.Occasion = translated.GetValueOrDefault("Occasion", dto.Occasion);
            dto.AdditionalPreferences = translated!.GetValueOrDefault("AdditionalPreferences", dto.AdditionalPreferences);

            return dto;
        }
        catch (JsonException ex)
        {
            logger.LogWarning("Failed to parse translated input JSON: {Error}", ex.Message);
            return dto;
        }
    }

    // ── Request models ────────────────────────────────────────────────────────

    public sealed class GeminiRequest
    {
        [JsonPropertyName("system_instruction")]
        public GeminiSystemInstruction SystemInstruction { get; set; } = default!;

        [JsonPropertyName("contents")]
        public GeminiContent[] Contents { get; set; } = default!;

        [JsonPropertyName("generationConfig")]
        public GeminiGenerationConfig GenerationConfig { get; set; } = default!;
    }

    public sealed class GeminiSystemInstruction
    {
        [JsonPropertyName("parts")]
        public GeminiPart[] Parts { get; set; } = default!;
    }

    public sealed class GeminiContent
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = default!;

        [JsonPropertyName("parts")]
        public GeminiPart[] Parts { get; set; } = default!;
    }

    public sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = default!;
    }

    public sealed class GeminiGenerationConfig
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("maxOutputTokens")]
        public int MaxOutputTokens { get; set; }
    }

    // ── Response models ───────────────────────────────────────────────────────

    public sealed class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public GeminiCandidate[]? Candidates { get; set; }
    }

    public sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }

        [JsonPropertyName("finishReason")]
        public string? FinishReason { get; set; }
    }
}