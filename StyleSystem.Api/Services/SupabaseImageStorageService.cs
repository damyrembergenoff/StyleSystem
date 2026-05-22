using StyleSystem.Api.Abstractions;

namespace StyleSystem.Api.Services;

public class SupabaseImageStorageService(
    IConfiguration config,
    HttpClient httpClient) : IImageStorageService
{
    public async Task<string> SaveImageAsync(byte[] imageBytes, CancellationToken cancellationToken = default)
    {
        var fileName = $"outfit_{Guid.NewGuid()}.png";
        var supabaseUrl = config["Supabase:Url"];
        var url = $"{supabaseUrl}/storage/v1/object/generated-images/{fileName}";

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("Authorization", $"Bearer {config["Supabase:ServiceKey"]}");
        request.Headers.Add("apikey", config["Supabase:ServiceKey"]);
        request.Content = new ByteArrayContent(imageBytes);
        request.Content.Headers.ContentType = new("image/png");

        var response = await httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Upload status: {response.StatusCode}");
        Console.WriteLine($"Upload response: {responseBody}");

        return $"{supabaseUrl}/storage/v1/object/public/generated-images/{fileName}";
    }
}