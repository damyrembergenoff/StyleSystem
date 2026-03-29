namespace StyleSystem.Web;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        var apiUrl = configuration.GetValue<string>("WebApiAddress") ?? string.Empty;

        services.AddHttpClient("Public", client =>
        {
            client.BaseAddress = new Uri(apiUrl);
            client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "true");
        });

        services
            .AddHttpClient("Private", client =>
            {
                client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "true");
            })
            .AddHttpMessageHandler<JwtAuthorizationMessageHandler>();
        
        services.AddScoped<JwtAuthorizationMessageHandler>();
        services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Private"));

        return services;
    }

    
}