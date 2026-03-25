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
        var jsonFormat = """
        {
            "recommendation": "detailed outfit recommendation text here",
            "imagePrompt": "full body portrait of a [gender] with [bodyType] body type, wearing [specific outfit you recommended], full body shot from head to toe, no cropping, clean white studio background, professional fashion photography, 4k, photorealistic"
        }
        """;

        var bodyType = user.Gender == EGender.Male
            ? user.MaleBodyType?.ToString()
            : user.FemaleBodyType?.ToString();

        return $"""
            You are a professional fashion stylist.
            Provide a detailed outfit recommendation based on the following information:

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

            Please provide:
            1. Complete outfit recommendation (top, bottom, shoes, accessories)
            2. Why this outfit suits their body type
            3. Color combinations that work well with their skin tone
            4. Specific style tips for the occasion
            5. Alternative options if available

            Keep the response detailed, professional, and helpful.

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
            "imagePrompt": "full body portrait of a [gender] with [bodyType] body type, wearing [specific outfit you recommended], full body shot from head to toe, no cropping, clean white studio background, professional fashion photography, 4k, photorealistic"
        }
        """;
        
        return $"""
            You are a professional fashion stylist.
            Provide a detailed outfit recommendation based on the following information:

            CLIENT PROFILE:
            - Gender: {dto.Gender}
            - Body Type: {dto.BodyType}
            - Age Range: {dto.AgeRange}
            - Occasion: {dto.Occasion}
            - Season: {dto.Season}
            - Temperature: {dto.Temperature}°C
            - Preferences: {dto.AdditionalPreferences ?? "None"}

            Respond in EXACTLY this JSON format, nothing else:
            {jsonFormat}
            """;
    }

    // Text bo'yicha image prompt
    private string BuildImagePrompt(string recommendationText)
    {
        return $"""
            Create a professional fashion outfit image based on this recommendation:
            {recommendationText}

            Requirements:
            - Full body shot
            - Clean white or neutral background
            - Professional fashion photography style
            - High quality and realistic
            - Show the complete outfit clearly
            """;
    }

    // Entity → DTO
    private RecommendationResponseDto MapToResponseDto(Recommendation recommendation)
    {
        return new RecommendationResponseDto
        {
            Id = recommendation.Id,
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