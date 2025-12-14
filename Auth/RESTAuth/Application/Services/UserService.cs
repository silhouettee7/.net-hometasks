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
    IFileReportService fileReportService,
    IFileStorageService fileStorageService,
    IConfiguration configuration): IUserService
{
    public async Task<AppResult<Guid>> RegisterUser(UserDtoRequest dto)
    {
        try
        {
            var possibleUserResult = await userRepository.GetUserByEmail(dto.Email);
            if (!possibleUserResult.IsSuccess)
            {
                return AppResult<Guid>.Failure(new AppError(ErrorType.BadRequest, "User already exists"));
            }
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Email = dto.Email,
                Password = dto.Password,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                Role = dto.Email == configuration["AdminEmail"] ? "Admin" : "User",
                Department = dto.Department
            };
            var result = await userRepository.Add(user);
            return result;
        }
        catch (Exception ex)
        {
            return AppResult<Guid>.Failure(new AppError(ErrorType.ServerError, ex.Message));
        }
    }

    public async Task<AppResult> EditUser(Guid id, UserDtoRequest dto)
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
            UpdatedDate = DateTime.UtcNow,
            Password = dto.Password,
            Name = dto.Name,
            Role = oldUser.Role
        };
        var updateResult = await userRepository.Update(user);
        return updateResult;
    }

    public async Task<AppResult> DeleteUser(Guid id)
    {
        var result = await userRepository.Delete(id);
        return result;
    }

    public async Task<AppResult<CursorPaginationResponse<UserDtoResponse>>> GetUsersPageForPeriodByRegistrationDate(
        CursorPaginationRequest request, DateTime startDate, DateTime endDate)
    {
        startDate = startDate.ToUniversalTime();
        endDate = endDate.ToUniversalTime();
        queryBuilder.Where(u => u.CreatedDate >= startDate && u.CreatedDate <= endDate);
        var result = await paginationService.GetPageAsync(queryBuilder, request);
        return HandleUsersPaginationResult(result);
    }

    public async Task<AppResult<CursorPaginationResponse<UserDtoResponse>>> GetUsersPageForPeriodByUpdatingDate(
        CursorPaginationRequest request, DateTime startDate, DateTime endDate)
    {
        startDate = startDate.ToUniversalTime();
        endDate = endDate.ToUniversalTime();
        queryBuilder.Where(u => u.UpdatedDate >= startDate && u.UpdatedDate <= endDate);
        var result = await paginationService.GetPageAsync(queryBuilder, request);
        return HandleUsersPaginationResult(result);
    }
    
    public async Task<AppResult<List<DepartmentAverageSalaryDto>>> GetUserDepartmentAverageSalaries()
    {
        var result = await userRepository.GetUserAverageSalariesByDepartment();
        if (!result.IsSuccess)
        {
            return AppResult<List<DepartmentAverageSalaryDto>>.Failure(result.AppError!);
        }
        return AppResult<List<DepartmentAverageSalaryDto>>.Success(SuccessType.Ok, 
            result.Value!
                .Select(pair => new DepartmentAverageSalaryDto 
                {
                    Department = pair.Key, 
                    Salary = pair.Value 
                })
                .ToList());
    }

    public async Task<AppResult<string>> GenerateReportOnUsersAndReturnLink(Guid reportId)
    {
        try
        {
            var users = await userRepository.GetAllUsers();
            var fileReport = fileReportService.CreateFileReport(reportId, users);
            await fileStorageService.UploadFileAsync(fileReport.FileName, fileReport.Content,
                fileReport.ContentType);
            var link = await fileStorageService.GetFileLinkAsync(fileReport.FileName);
            await Task.Delay(3000); //искусственная задержка
            return AppResult<string>.Success(SuccessType.Ok, link);
        }
        catch (Exception ex)
        {
            return AppResult<string>.Failure(
                new AppError(ErrorType.ServerError, "Не удалось сгенерировать отчет"));
        }
    }

    private AppResult<CursorPaginationResponse<UserDtoResponse>> HandleUsersPaginationResult(
        AppResult<CursorPaginationResponse<User>> appResult)
    {
        if (!appResult.IsSuccess)
        {
            return AppResult<CursorPaginationResponse<UserDtoResponse>>.Failure(appResult.AppError!);
        }
        var users = appResult.Value!.Items
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
            HasNext = appResult.Value.HasNext,
            HasPrevious = appResult.Value.HasPrevious,
            NextCursor = appResult.Value.NextCursor,
            PreviousCursor = appResult.Value.PreviousCursor
        };
        return AppResult<CursorPaginationResponse<UserDtoResponse>>.Success(SuccessType.Ok,paginationResult);
    }
}