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

        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";

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
        var paragraphs = text
            .Split(["\n\n", "\\n\\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList();

        if (paragraphs.Count <= 1)
            return await TranslateChunkAsync(text, cancellationToken);

        var results = new List<string>();

        foreach (var paragraph in paragraphs)
        {
            var translated = await TranslateChunkAsync(paragraph, cancellationToken);
            results.Add(translated);
            await Task.Delay(300, cancellationToken);
        }

        return string.Join("\n\n", results);
    }

    private async Task<string> TranslateChunkAsync(string text, CancellationToken cancellationToken, int attempt = 0)
    {
        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";

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
                        5. Do NOT repeat any word or phrase consecutively. Each sentence must be unique and complete.
                        6. Output ONLY the translated Karakalpak text. Do not include greetings, explanations, notes, or any conversational filler.
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
                Temperature = attempt == 0 ? 0.1 : 0.4, // retry da temperature oshirish
                MaxOutputTokens = 2048
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
        logger.LogInformation("Gemini AI raw response [{StatusCode}]: {RawJson}", (int)response.StatusCode, rawJson);

        var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(rawJson, JsonOptions);
        var translated = geminiResponse?
            .Candidates?[0]
            .Content?
            .Parts?[0]
            .Text?
            .Trim();

        if (translated != null && HasRepetition(translated))
        {
            if (attempt < 2)
            {
                logger.LogWarning("Repetition detected, retrying chunk (attempt {Attempt})...", attempt + 1);
                await Task.Delay(500, cancellationToken);
                return await TranslateChunkAsync(text, cancellationToken, attempt + 1);
            }

            // 2 marta urinib ham loop bo'lsa — loop qismini kesib tashlash
            logger.LogWarning("Repetition persists after {Attempt} attempts, trimming result", attempt);
            return TrimRepetition(translated);
        }

        return translated ?? text;
    }

    private static string TrimRepetition(string text)
    {
        var words = text.Split(' ');
        var result = new List<string>();

        for (int i = 0; i < words.Length; i++)
        {
            result.Add(words[i]);

            // Oxirgi 5 so'z bir xil bo'lsa — to'xtat
            if (result.Count >= 5)
            {
                var last5 = result.TakeLast(5).Select(w => w.ToLower()).ToList();
                if (last5.Distinct().Count() == 1)
                {
                    // Loop boshlanishidan oldingi qismni qaytarish
                    return string.Join(" ", result.Take(result.Count - 5)).TrimEnd(',', '.') + ".";
                }
            }
        }

        return string.Join(" ", result);
    }

    private static bool HasRepetition(string text)
    {
        var words = text.Split(' ');
        if (words.Length < 10) return false;

        var last20 = words.TakeLast(20).ToList();
        return last20
            .GroupBy(w => w.ToLower())
            .Any(g => g.Count() >= 5);
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

        var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";

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
    }
}