using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Abstractions.Services;
using RESTAuth.Domain.Dtos;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Application.Services;

public class UserService(IUserRepository userRepository, IConfiguration configuration): IUserService
{
    public async Task<Result> RegisterUser(UserDtoRequest dto)
    {
        try
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password,
                CreatedDate = DateTime.Now,
            };
            user.Role = dto.Email == configuration["AdminEmail"] ? "Admin" : "User";
            var result = await userRepository.Add(user);
            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error(ErrorType.ServerError, ex.Message));
        }
    }

    public async Task<Result> EditUser(Guid id, UserDtoRequest dto)
    {
        var result = await userRepository.GetEntityById(id);
        if (!result.IsSuccess)
        {
            return result;
        }
        var oldUser = result.Value;
        var user = new User
        {
            Id = id,
            Email = dto.Email,
            CreatedDate = oldUser.CreatedDate,
            UpdatedDate = DateTime.Now,
            Password = dto.Password,
            Name = dto.Name,
            Role = oldUser.Role
        };
        var updateResult = await userRepository.Update(user);
        return updateResult;
    }

    public async Task<Result> DeleteUser(Guid id)
    {
        var result = await userRepository.Delete(id);
        return result;
    }

    public async Task<Result<List<UserDtoResponse>>> GetUsersForPeriodByRegistrationDate(DateTime startDate, DateTime endDate)
    {
        var result = await userRepository.GetUsersForPeriodByRegistrationDate(startDate, endDate);
        if (!result.IsSuccess)
        {
            return Result<List<UserDtoResponse>>.Failure(result.Error);
        }
        var users = result.Value!
            .Select(u => new UserDtoResponse
            {
                CreatedDate = u.CreatedDate,
                UpdatedDate = u.UpdatedDate,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role
            })
            .ToList();
        return Result<List<UserDtoResponse>>.Success(users);
    }

    public async Task<Result<List<UserDtoResponse>>> GetUsersForPeriodByUpdatingDate(DateTime startDate, DateTime endDate)
    {
        var result = await userRepository.GetUsersForPeriodByUpdatedDate(startDate, endDate);
        if (!result.IsSuccess)
        {
            return Result<List<UserDtoResponse>>.Failure(result.Error);
        }
        var users = result.Value!
            .Select(u => new UserDtoResponse
            {
                CreatedDate = u.CreatedDate,
                UpdatedDate = u.UpdatedDate,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role
            })
            .ToList();
        return Result<List<UserDtoResponse>>.Success(users);
    }
}