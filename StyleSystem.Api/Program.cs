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
builder.Services.AddScoped<IGroqService, GroqService>();
builder.Services.AddScoped<SeedService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPollinationService, PollinationService>();
builder.Services.AddScoped<IAuthenticationStateService, AuthenticationStateService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<JwtService>();
builder.Configuration.AddUserSecrets<Program>();

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

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<SeedService>();
    await seeder.StartSeedingAsync();
}

app.UseHttpsRedirection();
app.UseCors("AllowedOrigins");

app.MapControllers();

app.Run();