using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StyleSystem.Api.Abstractions;
using StyleSystem.Api.Data;
using StyleSystem.Api.Entities;
using StyleSystem.Shared.Dtos;

namespace StyleSystem.Api.Services;

public class UserService(
    StyleSystemDbContext context,
    JwtService jwtService) : IUserService
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
            Height = userEntity.Height,
            Weight = userEntity.Weight,

            Gender = userEntity.Gender switch
            {
                Entities.EGender.Male => Shared.Dtos.EGender.Male,
                Entities.EGender.Female => Shared.Dtos.EGender.Female,
                _ => throw new ArgumentOutOfRangeException(nameof(userEntity.Gender), userEntity.Gender, null)
            },
            MaleBodyType = userEntity.MaleBodyType switch
            {
                Entities.EMaleBodyType.Ectomorph => Shared.Dtos.EMaleBodyType.Ectomorph,
                Entities.EMaleBodyType.Mesomorph => Shared.Dtos.EMaleBodyType.Mesomorph,
                Entities.EMaleBodyType.Endormoph => Shared.Dtos.EMaleBodyType.Endormoph,
                Entities.EMaleBodyType.Rectangle => Shared.Dtos.EMaleBodyType.Rectangle,
                Entities.EMaleBodyType.Triangle => Shared.Dtos.EMaleBodyType.Triangle,
                Entities.EMaleBodyType.Oval => Shared.Dtos.EMaleBodyType.Oval,
                Entities.EMaleBodyType.InvertedTriangle => Shared.Dtos.EMaleBodyType.InvertedTriangle,
                _ => throw new ArgumentOutOfRangeException(nameof(userEntity.MaleBodyType), userEntity.MaleBodyType, null)
            },
            FemaleBodyType = userEntity.FemaleBodyType switch
            {
                Entities.EFemaleBodyType.Pear => Shared.Dtos.EFemaleBodyType.Pear,
                Entities.EFemaleBodyType.Hourglass => Shared.Dtos.EFemaleBodyType.Hourglass,
                Entities.EFemaleBodyType.Rectangle => Shared.Dtos.EFemaleBodyType.Rectangle,
                Entities.EFemaleBodyType.Apple => Shared.Dtos.EFemaleBodyType.Apple,
                Entities.EFemaleBodyType.InvertedTriangle => Shared.Dtos.EFemaleBodyType.InvertedTriangle,
                Entities.EFemaleBodyType.Diamond => Shared.Dtos.EFemaleBodyType.Diamond,
                Entities.EFemaleBodyType.Petite => Shared.Dtos.EFemaleBodyType.Petite,
                Entities.EFemaleBodyType.PlusSize => Shared.Dtos.EFemaleBodyType.PlusSize,
                _ => throw new ArgumentOutOfRangeException(nameof(userEntity.FemaleBodyType), userEntity.FemaleBodyType, null)
            },
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

        // userEntity.Height = user.Height;
        // userEntity.Weight = user.Weight;
        // userEntity.Gender = user.Gender switch
        // {
        //     Shared.Dtos.EGender.Male => Entities.EGender.Male,
        //     Shared.Dtos.EGender.Female => Entities.EGender.Female,
        //     _ => throw new ArgumentOutOfRangeException(nameof(user.Gender), user.Gender, null)
        // };
        // userEntity.MaleBodyType = user.MaleBodyType switch
        // {
        //     Shared.Dtos.EMaleBodyType.Ectomorph => Entities.EMaleBodyType.Ectomorph,
        //     Shared.Dtos.EMaleBodyType.Mesomorph => Entities.EMaleBodyType.Mesomorph,
        //     Shared.Dtos.EMaleBodyType.Endormoph => Entities.EMaleBodyType.Endormoph,
        //     Shared.Dtos.EMaleBodyType.Rectangle => Entities.EMaleBodyType.Rectangle,
        //     Shared.Dtos.EMaleBodyType.Triangle => Entities.EMaleBodyType.Triangle,
        //     Shared.Dtos.EMaleBodyType.Oval => Entities.EMaleBodyType.Oval,
        //     Shared.Dtos.EMaleBodyType.InvertedTriangle => Entities.EMaleBodyType.InvertedTriangle,
        //     _ => throw new ArgumentOutOfRangeException(nameof(user.MaleBodyType), user.MaleBodyType, null)
        // };
        // userEntity.FemaleBodyType = user.FemaleBodyType switch
        // {
        //     Shared.Dtos.EFemaleBodyType.Pear => Entities.EFemaleBodyType.Pear,
        //     Shared.Dtos.EFemaleBodyType.Hourglass => Entities.EFemaleBodyType.Hourglass,
        //     Shared.Dtos.EFemaleBodyType.Rectangle => Entities.EFemaleBodyType.Rectangle,
        //     Shared.Dtos.EFemaleBodyType.Apple => Entities.EFemaleBodyType.Apple,
        //     Shared.Dtos.EFemaleBodyType.InvertedTriangle => Entities.EFemaleBodyType.InvertedTriangle,
        //     Shared.Dtos.EFemaleBodyType.Diamond => Entities.EFemaleBodyType.Diamond,
        //     Shared.Dtos.EFemaleBodyType.Petite => Entities.EFemaleBodyType.Petite,
        //     Shared.Dtos.EFemaleBodyType.PlusSize => Entities.EFemaleBodyType.PlusSize,
        //     _ => throw new ArgumentOutOfRangeException(nameof(user.FemaleBodyType), user.FemaleBodyType, null)
        // };
        // userEntity.SkinTone = user.SkinTone switch
        // {
        //     Shared.Dtos.ESkinTone.Light => Entities.ESkinTone.Light,
        //     Shared.Dtos.ESkinTone.Medium => Entities.ESkinTone.Medium,
        //     Shared.Dtos.ESkinTone.Dark => Entities.ESkinTone.Dark,
        //     Shared.Dtos.ESkinTone.Tan => Entities.ESkinTone.Tan,
        //     Shared.Dtos.ESkinTone.Fair => Entities.ESkinTone.Fair,
        //     Shared.Dtos.ESkinTone.Olive => Entities.ESkinTone.Olive,
        //     _ => throw new ArgumentOutOfRangeException(nameof(user.SkinTone), user.SkinTone, null)
        // };

        await context.Users.AddAsync(userEntity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        var createdUser = await context.Users
            .FirstAsync(u => u.Username == user.Username, cancellationToken);

        var token = jwtService.GenerateToken(createdUser);

        return new LoginResponse { Token = token };
    }
}