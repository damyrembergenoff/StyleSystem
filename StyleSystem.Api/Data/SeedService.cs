using StyleSystem.Api.Entities;

namespace StyleSystem.Api.Data;

public class SeedService(
    StyleSystemDbContext context,
    ILogger<SeedService> logger,
    IConfiguration configuration)
{
    public async Task StartSeedingAsync(CancellationToken cancellationToken = default)
    {
        await SeedUsersAsync(cancellationToken);
    }

    private async Task SeedUsersAsync(CancellationToken cancellationToken)
    {
        if(configuration.GetValue("Seeding:Users:IsEnabled", false) is false)
        {
            logger.LogInformation("Seeding Users is not enabled. Skipping...");
            return;
        }

        if(context.Users.Count() > 1)
        {
            logger.LogInformation("Seeding Users is skipped. Users table has entities.");
            return;
        }

        await Task.CompletedTask;
        
        // List<User> users =
        // [
        //     new() { FullName = "Sanjar Abdullaev", Username = "sanjar", Height = 178, Weight = 65, Gender = EGender.Male, MaleBodyType = EMaleBodyType.Ectomorph, SkinTone = ESkinTone.Light },
        //     new() { FullName = "Hasan Sanjarbekov", Username = "hasan", Height = 182, Weight = 75, Gender = EGender.Male, MaleBodyType = EMaleBodyType.Mesomorph, SkinTone = ESkinTone.Dark },
        //     new() { FullName = "Hulkar Abdullaeva", Username = "hulkar", Height = 168, Weight = 55, Gender = EGender.Female, FemaleBodyType = EFemaleBodyType.Hourglass, SkinTone = ESkinTone.Medium },
        //     new() { FullName = "Temur Mirzaahmedov", Username = "temur", Height = 184, Weight = 65, Gender = EGender.Male, MaleBodyType = EMaleBodyType.Triangle, SkinTone = ESkinTone.Dark },
        // ];

        // await context.Users.AddRangeAsync(users, cancellationToken);
        // await context.SaveChangesAsync(cancellationToken);
    }
}