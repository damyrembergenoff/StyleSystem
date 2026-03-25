using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StyleSystem.Api.Abstractions;
using StyleSystem.Api.Data;
using StyleSystem.Api.Entities;
using StyleSystem.Shared.Dtos;
using StyleSystem.Web.Dtos.CompleteProfile;

namespace StyleSystem.Api.Services;

public class UserService(
    StyleSystemDbContext context,
    JwtService jwtService,
    ILogger<UserService> logger) : IUserService
{
    public async ValueTask<UserDto> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userEntity = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        
        if(userEntity is null)
            return null!;

        UserDto userDto = new UserDto
        {
            Username = userEntity.Username,
            FullName = userEntity.FullName,
            Age = userEntity.Age,
            Height = userEntity.Height,
            Weight = userEntity.Weight,

            Gender = userEntity.Gender switch
            {
                Entities.EGender.Male => Shared.Dtos.EGender.Male,
                Entities.EGender.Female => Shared.Dtos.EGender.Female,
                _ => throw new ArgumentOutOfRangeException(nameof(userEntity.Gender), userEntity.Gender, null)
            },
            MaleBodyType = userEntity.MaleBodyType.HasValue ? userEntity.MaleBodyType.Value switch
            {
                Entities.EMaleBodyType.Trapezoid => Shared.Dtos.EMaleBodyType.Trapezoid,
                Entities.EMaleBodyType.Rectangle => Shared.Dtos.EMaleBodyType.Rectangle,
                Entities.EMaleBodyType.Triangle => Shared.Dtos.EMaleBodyType.Triangle,
                Entities.EMaleBodyType.Oval => Shared.Dtos.EMaleBodyType.Oval,
                Entities.EMaleBodyType.InvertedTriangle => Shared.Dtos.EMaleBodyType.InvertedTriangle,
                _ => throw new ArgumentOutOfRangeException(nameof(userEntity.MaleBodyType), userEntity.MaleBodyType, null)
            } : null,
            FemaleBodyType = userEntity.FemaleBodyType.HasValue ? userEntity.FemaleBodyType.Value switch
            {
                Entities.EFemaleBodyType.Hourglass => Shared.Dtos.EFemaleBodyType.Hourglass,
                Entities.EFemaleBodyType.Pear => Shared.Dtos.EFemaleBodyType.Pear,
                Entities.EFemaleBodyType.Apple => Shared.Dtos.EFemaleBodyType.Apple,
                Entities.EFemaleBodyType.Rectangle => Shared.Dtos.EFemaleBodyType.Rectangle,
                Entities.EFemaleBodyType.InvertedTriangle => Shared.Dtos.EFemaleBodyType.InvertedTriangle,
                Entities.EFemaleBodyType.Diamond => Shared.Dtos.EFemaleBodyType.Diamond,
                _ => throw new ArgumentOutOfRangeException(nameof(userEntity.FemaleBodyType), userEntity.FemaleBodyType, null)
            } : null,
            SkinTone = userEntity.SkinTone switch
            {
                Entities.ESkinTone.Light => Shared.Dtos.ESkinTone.Light,
                Entities.ESkinTone.Medium => Shared.Dtos.ESkinTone.Medium,
                Entities.ESkinTone.Dark => Shared.Dtos.ESkinTone.Dark,
                Entities.ESkinTone.Tan => Shared.Dtos.ESkinTone.Tan,
                Entities.ESkinTone.Fair => Shared.Dtos.ESkinTone.Fair,
                Entities.ESkinTone.Olive => Shared.Dtos.ESkinTone.Olive,
                _ => throw new ArgumentOutOfRangeException(nameof(userEntity.SkinTone), userEntity.SkinTone, null)
            }
        };

        return userDto;
    }

    public async ValueTask<LoginResponse> LoginAsync(LoginUserDto user, CancellationToken cancellationToken = default)
    {
        var userEntity = await context.Users
            .FirstOrDefaultAsync(u => u.Username == user.Username, cancellationToken);

        if(userEntity is null)
        {
            return new LoginResponse { Token = string.Empty };
        }

        if(new PasswordHasher<User>().VerifyHashedPassword(null!, userEntity.PasswordHash!, user.Password!) == PasswordVerificationResult.Failed)
        {
            return new LoginResponse { Token = string.Empty };
        }

        var token = jwtService.GenerateToken(userEntity);
        return new LoginResponse { Token = token };
    }

    public async ValueTask<LoginResponse> RegisterAsync(RegisterUserDto user, CancellationToken cancellationToken = default)
    {
        var usernameExists = await context.Users
            .AnyAsync(u => u.Username == user.Username);

        if (usernameExists)
        {
            return new LoginResponse { Token = string.Empty };
        }

        var userEntity = new User();

        userEntity.Username = user.Username;
        userEntity.PasswordHash = new PasswordHasher<User>().HashPassword(null!, user.Password!);
        userEntity.FullName = user.FullName;

        await context.Users.AddAsync(userEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        var createdUser = await context.Users
            .FirstAsync(u => u.Username == user.Username, cancellationToken);

        var token = jwtService.GenerateToken(createdUser);

        return new LoginResponse { Token = token };
    }

    public async ValueTask UpdateUserAsync(Guid userId, ProfileModel model, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("UpdateUserAsync called for UserId: {UserId}", userId);
        logger.LogInformation("Received model - FullName: {FullName}, Gender: {Gender}, BodyType: {BodyType}, Height: {Height}, Weight: {Weight}, SkinTone: {SkinTone}",
            model?.FullName, model?.Gender, model?.BodyType, model?.Height, model?.Weight, model?.SkinTone);

        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            logger.LogWarning("User not found for UserId: {UserId}", userId);
            throw new InvalidOperationException("User not found");
        }

        logger.LogInformation("User found, attempting to map profile data for UserId: {UserId}", userId);

        try
        {
            // Update user properties based on the provided profile model
            user.FullName = model!.FullName;
            user.Height = model.Height;
            user.Weight = model.Weight;
            user.Age = model.Age;

            logger.LogInformation("Mapping Gender: {Gender}", model.Gender);
            user.Gender = model.Gender switch
            {
                "Male" => Entities.EGender.Male,
                "Female" => Entities.EGender.Female,
                _ => throw new ArgumentOutOfRangeException(nameof(model.Gender), model.Gender, $"Invalid gender value: '{model.Gender}'. Expected 'Male' or 'Female'")
            };

            logger.LogInformation("Mapping BodyType for Gender: {Gender}, BodyType: {BodyType}", model.Gender, model.BodyType);
            user.MaleBodyType = model.Gender == "Male" ? model.BodyType switch
            {
                "Rectangle" => Entities.EMaleBodyType.Rectangle,
                "Triangle" => Entities.EMaleBodyType.Triangle,
                "Oval" => Entities.EMaleBodyType.Oval,
                "Trapezoid" => Entities.EMaleBodyType.Trapezoid,
                "InvertedTriangle" => Entities.EMaleBodyType.InvertedTriangle,
                _ => throw new ArgumentOutOfRangeException(nameof(model.BodyType), model.BodyType, $"Invalid male body type: '{model.BodyType}'")
            } : null;

            user.FemaleBodyType = model.Gender == "Female" ? model.BodyType switch
            {
                "Pear" => Entities.EFemaleBodyType.Pear,
                "Hourglass" => Entities.EFemaleBodyType.Hourglass,
                "Rectangle" => Entities.EFemaleBodyType.Rectangle,
                "Apple" => Entities.EFemaleBodyType.Apple,
                "InvertedTriangle" => Entities.EFemaleBodyType.InvertedTriangle,
                "Diamond" => Entities.EFemaleBodyType.Diamond,
                _ => throw new ArgumentOutOfRangeException(nameof(model.BodyType), model.BodyType, $"Invalid female body type: '{model.BodyType}'")
            } : null;

            logger.LogInformation("Mapping SkinTone: {SkinTone}", model.SkinTone);
            user.SkinTone = model.SkinTone switch
            {
                "Fair" => Entities.ESkinTone.Fair,
                "Light" => Entities.ESkinTone.Light,
                "Medium" => Entities.ESkinTone.Medium,
                "Olive" => Entities.ESkinTone.Olive,
                "Tan" => Entities.ESkinTone.Tan,
                "Dark" => Entities.ESkinTone.Dark,
                _ => throw new ArgumentOutOfRangeException(nameof(model.SkinTone), model.SkinTone, $"Invalid skin tone: '{model.SkinTone}'")
            };

            logger.LogInformation("All mappings successful for UserId: {UserId}, saving changes", userId);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("User profile successfully updated and saved for UserId: {UserId}", userId);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            logger.LogError(ex, "ArgumentOutOfRangeException during mapping for UserId: {UserId}, Field: {FieldName}, Value: {Value}",
                userId, ex.ParamName, ex.ActualValue);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected exception in UpdateUserAsync for UserId: {UserId}", userId);
            throw;
        }
    }

    public async ValueTask DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("User not found");
        }

        context.Users.Remove(user);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask ChangePasswordAsync(Guid userId, ChangePasswordDto changePassword, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user is null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Verify current password
        var passwordHasher = new PasswordHasher<User>();
        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash!, changePassword.CurrentPassword);

        if (result == PasswordVerificationResult.Failed)
        {
            throw new InvalidOperationException("Current password is incorrect");
        }

        // Update password
        user.PasswordHash = passwordHasher.HashPassword(user, changePassword.NewPassword);
        await context.SaveChangesAsync(cancellationToken);
    }
}