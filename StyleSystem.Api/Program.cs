using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StyleSystem.Api.Abstractions;
using StyleSystem.Api.Data;
using StyleSystem.Api.Options;
using StyleSystem.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StyleSystemDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("StyleSystem")));

builder.Services.Configure<GroqOptions>(
    builder.Configuration.GetSection("Groq")
);

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowedOrigins",
        policy  =>
        {
            policy.WithOrigins("http://localhost:5074")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IImageStorageService, LocalImageStorageService>();
builder.Services.AddScoped<SeedService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthenticationStateService, AuthenticationStateService>();
builder.Services.AddScoped<ITextAiService, GroqAiService>();
builder.Services.AddScoped<IImageAiService, CloudflareAiService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<JwtService>();
builder.Configuration.AddUserSecrets<Program>();
builder.Services.AddDirectoryBrowser();
builder.Services.AddHttpClient<IImageAiService, CloudflareAiService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(2);
});
builder.Services.AddLogging();


builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"]!,

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"]!,

            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
if (!Directory.Exists(webRootPath))
    Directory.CreateDirectory(webRootPath);

var generatedImagesPath = Path.Combine(webRootPath, "generated-images");
if (!Directory.Exists(generatedImagesPath))
    Directory.CreateDirectory(generatedImagesPath);

builder.Environment.WebRootPath = webRootPath;

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<SeedService>();
    await seeder.StartSeedingAsync();
}

app.UseHttpsRedirection();
app.UseCors("AllowedOrigins");

app.UseStaticFiles();

app.MapControllers();

app.Run();