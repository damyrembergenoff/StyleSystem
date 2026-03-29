using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StyleSystem.Api.Abstractions;
using StyleSystem.Api.Data;
using StyleSystem.Api.Dtos.Recommendations;
using StyleSystem.Api.Entities;
using StyleSystem.Shared.Dtos.Recommendations;
using StyleSystem.Shared.DTOs.Recommendations;

namespace StyleSystem.Api.Services;

public class RecommendationService(
    StyleSystemDbContext context,
    ITextAiService textAiService,
    IImageAiService imageAiService,
    IImageStorageService imageStorageService,
    ILogger<RecommendationService> logger
) : IRecommendationService
{

    // ✅ Authenticated user uchun - DB ga saqlanadi
    public async Task<RecommendationResponseDto> CreateAsync(
        CreateRecommendationDto dto, 
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // 1. User ma'lumotlarini olish (Gender, BodyType, Age)
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new Exception("User not found");

        // 2. Entity yaratish
        var recommendation = new Recommendation
        {
            UserId = userId,
            Occasion = dto.Occasion,
            Season = dto.Season,
            Temperature = dto.Temperature,
            AdditionalPreferences = dto.AdditionalPreferences,
        };

        // 3. AI #1 - Text prompt tuzib, recommendation olish
        var textPrompt = BuildTextPrompt(user, dto);
        var rawResponse = await textAiService
            .GenerateRecommendationAsync(textPrompt, cancellationToken);

        var cleaned = rawResponse
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        var fashionResponse = JsonSerializer.Deserialize<FashionAiResponse>(cleaned)
            ?? throw new Exception("Failed to parse Groq response");

        recommendation.RecommendationText = fashionResponse.Recommendation;

        var imageBytes = await imageAiService.GenerateImagesAsync(fashionResponse.ImagePrompt!, count: 2, cancellationToken: cancellationToken);

        // 5. Rasmlarni saqlash
        var images = new List<RecommendationImage>();
        foreach (var (bytes, index) in imageBytes.Select((b, i) => (b, i)))
        {
            var imageUrl = await imageStorageService.SaveImageAsync(bytes, cancellationToken);
            images.Add(new RecommendationImage
            {
                ImageUrl = imageUrl,
                Order = index + 1,
            });
        }

        recommendation.Images = images;

        // 6. DB ga saqlash
        await context.Recommendations.AddAsync(recommendation, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // 7. Response qaytarish
        return MapToResponseDto(recommendation);
    }

    // ❌ Anonymous user uchun - DB ga saqlanmaydi
    public async Task<AnonymousRecommendationResponseDto> CreateAnonymousAsync(
        AnonymousRecommendationDto dto,
        CancellationToken cancellationToken = default)
    {
        // 1. AI #1 - Text prompt tuzib, recommendation olish
        var prompt = BuildAnonymousTextPrompt(dto);
        var rawResponse = await textAiService
            .GenerateRecommendationAsync(prompt, cancellationToken);
        
        logger.LogInformation("Raw AI response: {Response}", rawResponse);

        var cleaned = rawResponse
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();
        
        logger.LogInformation("Cleaned GroqAi response: {cleaned}", cleaned);
        
        var fashionResponse = JsonSerializer.Deserialize<FashionAiResponse>(cleaned)
            ?? throw new Exception("Failed to parse Groq response");

        // 2. AI #2 - Image prompt tuzib, rasmlar olish
        var imageBytes = await imageAiService.GenerateImagesAsync(fashionResponse.ImagePrompt!, count: 2, cancellationToken: cancellationToken);

        // 3. Rasmlarni saqlash (local file - temporary)
        var imageUrls = new List<string>();
        foreach (var bytes in imageBytes)
        {
            var imageUrl = await imageStorageService.SaveImageAsync(bytes, cancellationToken);
            imageUrls.Add(imageUrl);
        }

        // 4. Response qaytarish (DB ga saqlanmaydi)
        return new AnonymousRecommendationResponseDto
        {
            RecommendationText = fashionResponse.Recommendation,
            ImageUrls = imageUrls
        };
    }

    // ✅ User history
    public async Task<IList<RecommendationResponseDto>> GetUserHistoryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var recommendations = await context.Recommendations
            .Where(r => r.UserId == userId)
            .Include(r => r.Images)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10)
            .ToListAsync();

        return recommendations.Select(MapToResponseDto).ToList();
    }

    // ✅ Single recommendation
    public async Task<RecommendationResponseDto> GetByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var recommendation = await context.Recommendations
            .Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId, cancellationToken)
            ?? throw new Exception("Recommendation not found");

        return MapToResponseDto(recommendation);
    }

    // ============ PRIVATE HELPERS ============

    // Authenticated user uchun prompt
    private string BuildTextPrompt(User user, CreateRecommendationDto dto)
    {
        var bodyType = user.Gender == EGender.Male
            ? user.MaleBodyType?.ToString()
            : user.FemaleBodyType?.ToString();

        var jsonFormat = """
        {
            "recommendation": "detailed outfit recommendation text here",
            "imagePrompt": "full body portrait of a [gender], age [age], height [height] cm, weight [weight] kg, with a physique that accurately reflects these measurements (e.g. slim, skinny, athletic, average, or overweight based on height and weight proportion), realistic body proportions, wearing [specific outfit you recommended], full body shot from head to toe, no cropping, standing pose, clean white studio background, professional fashion photography, 4k, photorealistic, highly detailed skin tone [skinTone], natural lighting"
        }
        """;

        return $"""
            You are a professional fashion stylist.

            Provide a detailed outfit recommendation based on the following user information:

            Personal Information:
            - Gender: {user.Gender}
            - Body Type: {bodyType}
            - Age: {user.Age}
            - Height: {user.Height} cm
            - Weight: {user.Weight} kg
            - Skin Tone: {user.SkinTone}

            Occasion Details:
            - Occasion: {dto.Occasion}
            - Season: {dto.Season}
            - Temperature: {dto.Temperature}
            - Additional Preferences: {dto.AdditionalPreferences ?? "None"}

            IMPORTANT FOR IMAGE PROMPT:
            - The generated person MUST visually match the given height and weight.
            - Body shape MUST be derived from height and weight (NOT generic body types).
            - Example:
            • Tall + low weight → slim/skinny physique
            • Tall + high weight → large/broad physique
            • Medium proportions → average/athletic
            - The image MUST look realistic and proportionally correct.

            IMPORTANT FOR RECOMMENDATION TEXT:

            - The outfit recommendation MUST be fully tailored to the person's physical proportions and body type.
            - Fit, cut, layering, and overall styling MUST reflect actual height, weight, and body type.
            - Do NOT generate generic outfit suggestions.

            - Styling guidance based on proportions:
            • Tall & slim → add layering, avoid overly tight fits, add visual volume
            • Slim → use slightly relaxed or structured pieces to add presence
            • Broad/heavy → choose balanced and structured clothing
            • Shorter height → use vertical elements to elongate the silhouette

            - Skin tone MUST guide color selection to complement {user.SkinTone}.
            - Additional Preferences (if provided) MUST influence style, color, and aesthetic, 
            but MUST NOT override physical reality or proper fit.

            - The explanation MUST explicitly state WHY each item is suitable for this specific person,
            considering their body type, proportions, skin tone, and preferences.
            - The recommendation MUST feel highly personalized and professional.

            Please provide:
            1. Complete outfit recommendation (top, bottom, shoes, accessories)
            2. Why this outfit suits their body type AND physical proportions
            3. Color combinations that complement their skin tone
            4. Style tips specific for the occasion
            5. Alternative options if available

            Respond in EXACTLY this JSON format.
            All line breaks in "recommendation" must be escaped as \\n.
            Do not return raw newlines inside JSON strings.

            Respond in EXACTLY this JSON format, nothing else:
            {jsonFormat}
            """;
    }

    // Anonymous user uchun prompt
    private string BuildAnonymousTextPrompt(AnonymousRecommendationDto dto)
    {
        var jsonFormat = """
        {
            "recommendation": "detailed outfit recommendation text here",
            "imagePrompt": "full body portrait of a [gender], age range [ageRange], with a body shape matching [bodyType] AND any additional physical details inferred from the description (such as height, weight, body proportions if mentioned), wearing [specific outfit you recommended], full body shot from head to toe, no cropping, standing pose, clean white studio background, professional fashion photography, 4k, photorealistic, realistic skin tone and features"
        }
        """;
        
        return $"""
            You are a professional fashion stylist.

            Provide a detailed outfit recommendation based on the following information:

            Basic Information:
            - Gender: {dto.Gender}
            - Body Type: {dto.BodyType}
            - Age Range: {dto.AgeRange}

            Occasion Details:
            - Occasion: {dto.Occasion}
            - Season: {dto.Season}
            - Temperature: {dto.Temperature}
            - Additional Preferences: {dto.AdditionalPreferences ?? "None"}

            IMPORTANT INSTRUCTIONS:

            1. Carefully analyze "Additional Preferences".
            - Extract any useful personal details if mentioned, such as:
                • Height (e.g. 180 cm)
                • Weight (e.g. 65 kg)
                • Skin tone (e.g. dark, fair, olive)
                • Style preferences (e.g. black clothes, classic style, minimalism)

            2. If height and weight are mentioned:
            - Infer realistic body shape (e.g. slim, skinny, athletic, overweight)
            - The imagePrompt MUST reflect these proportions visually

            3. If additional physical traits are mentioned:
            - Use them in BOTH recommendation and imagePrompt

            4. If no extra details are provided:
            - Fall back to the given Body Type only

            5. VERY IMPORTANT:
            - The generated person in imagePrompt MUST look realistic
            - Body proportions MUST NOT be generic
            - MUST reflect all available information

            Please provide:
            1. Complete outfit recommendation (top, bottom, shoes, accessories)
            2. Why this outfit suits their body type
            3. Color combinations that work well
            4. Style tips for the occasion
            5. Alternative options if available
            6. VERY IMPORTANT FOR RECOMMENDATION TEXT:

            - The outfit recommendation MUST be tailored to the person's physical proportions, not just general body type.

            - If height and weight are available (explicitly or inferred):
            • You MUST adapt clothing fit, layering, and proportions accordingly
            • Example:
                - Tall and slim → use layering, avoid overly tight fits, add volume
                - Slim → slightly relaxed or structured clothing to add presence
                - Broad/heavy → use balanced and structured pieces

            - If skin tone is mentioned:
            • You MUST adapt color choices accordingly

            - If style preferences are mentioned (e.g. black, classic, minimal):
            • You MUST prioritize those preferences in the outfit

            - The recommendation MUST clearly feel personalized.
            - DO NOT generate generic outfit suggestions.
            7. PERSONALIZATION REQUIREMENT:

            - You MUST explicitly reflect the user's characteristics in your explanation.
            - Explain WHY each item suits their specific physique, not just body type.

            Respond in EXACTLY this JSON format.
            All line breaks in "recommendation" must be escaped as \\n.
            Do not return raw newlines inside JSON strings.

            Respond in EXACTLY this JSON format, nothing else:
            {jsonFormat}
            """;
    }

    // Entity → DTO
    private RecommendationResponseDto MapToResponseDto(Recommendation recommendation)
    {
        return new RecommendationResponseDto
        {
            Id = recommendation.Id,
            CreatedAt = recommendation.CreatedAt,
            Occasion = recommendation.Occasion,
            Season = recommendation.Season,
            Temperature = recommendation.Temperature,
            AdditionalPreferences = recommendation.AdditionalPreferences,
            RecommendationText = recommendation.RecommendationText,
            Images = recommendation.Images
                .OrderBy(i => i.Order)
                .Select(i => new RecommendationImageDto
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl,
                    Order = i.Order
                })
                .ToList(),
        };
    }
}