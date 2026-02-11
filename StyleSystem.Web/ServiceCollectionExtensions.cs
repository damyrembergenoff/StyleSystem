namespace StyleSystem.Web;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApiClients(this IServiceCollection services, IConfiguration configuration)
    {
        var apiUrl = configuration.GetValue<string>("WebApiAddress") ?? string.Empty;

        services.AddHttpClient("Public", client => client.BaseAddress = new Uri(apiUrl));

        services
            .AddHttpClient("Private", client => client.BaseAddress = new Uri(apiUrl))
            .AddHttpMessageHandler<JwtAuthorizationMessageHandler>();
        
        services.AddScoped<JwtAuthorizationMessageHandler>();
        services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Private"));

        return services;
    }

    
}