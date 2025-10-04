using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Abstractions.Services;
using RESTAuth.Domain.Dtos;
using RESTAuth.Domain.Entities;
using RESTAuth.Domain.Models;

namespace RESTAuth.Application.Services;

public class UserService(
    IUserRepository userRepository, 
    ICursorPaginationService<User,Guid> paginationService, 
    IQueryBuilder<User,Guid> queryBuilder,
    IConfiguration configuration): IUserService
{
    public async Task<Result> RegisterUser(UserDtoRequest dto)
    {
        try
        {
            var possibleUserResult = await userRepository.GetUserByEmail(dto.Email);
            if (possibleUserResult.IsSuccess)
            {
                return Result.Failure(new Error(ErrorType.BadRequest, "User already exists"));
            }
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password,
                CreatedDate = DateTime.Now,
                Role = dto.Email == configuration["AdminEmail"] ? "Admin" : "User"
            };
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

    public async Task<Result<CursorPaginationResponse<UserDtoResponse>>> GetUsersPageForPeriodByRegistrationDate(
        CursorPaginationRequest request, DateTime startDate, DateTime endDate)
    {
        queryBuilder.Where(u => u.CreatedDate >= startDate && u.CreatedDate <= endDate);
        var result = await paginationService.GetPageAsync(queryBuilder, request);
        return HandleUsersPaginationResult(result);
    }

    public async Task<Result<CursorPaginationResponse<UserDtoResponse>>> GetUsersPageForPeriodByUpdatingDate(
        CursorPaginationRequest request, DateTime startDate, DateTime endDate)
    {
        queryBuilder.Where(u => u.UpdatedDate >= startDate && u.UpdatedDate <= endDate);
        var result = await paginationService.GetPageAsync(queryBuilder, request);
        return HandleUsersPaginationResult(result);
    }

    public async Task<Result<List<DepartmentAverageSalaryDto>>> GetUserDepartmentAverageSalaries()
    {
        var result = await userRepository.GetUserAverageSalariesByDepartment();
        if (!result.IsSuccess)
        {
            return Result<List<DepartmentAverageSalaryDto>>.Failure(result.Error!);
        }
        return Result<List<DepartmentAverageSalaryDto>>.Success(SuccessType.Ok, 
            result.Value!
                .Select(pair => new DepartmentAverageSalaryDto 
                {
                    Department = pair.Key, 
                    Salary = pair.Value 
                })
                .ToList());
    }

    private Result<CursorPaginationResponse<UserDtoResponse>> HandleUsersPaginationResult(
        Result<CursorPaginationResponse<User>> result)
    {
        if (!result.IsSuccess)
        {
            return Result<CursorPaginationResponse<UserDtoResponse>>.Failure(result.Error!);
        }
        var users = result.Value!.Items
            .Select(u => new UserDtoResponse
            {
                CreatedDate = u.CreatedDate,
                UpdatedDate = u.UpdatedDate,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role
            })
            .ToList();
        var paginationResult = new CursorPaginationResponse<UserDtoResponse>()
        {
            Items = users,
            HasNext = result.Value.HasNext,
            HasPrevious = result.Value.HasPrevious,
            NextCursor = result.Value.NextCursor,
            PreviousCursor = result.Value.PreviousCursor
        };
        return Result<CursorPaginationResponse<UserDtoResponse>>.Success(SuccessType.Ok,paginationResult);
    }
}