using FluentValidation;
using RESTAuth.Application.Graph.Models;
using RESTAuth.Domain.Abstractions.Services;
using RESTAuth.Domain.Dtos;
using RESTAuth.Domain.Models;

namespace RESTAuth.Application.Graph;

public class Mutation
{
    public async Task<CreateUserPayload> CreateUser(
        string name,
        string email,
        string password,
        string department,
        [Service] IValidator<UserDtoRequest> validator,
        [Service] IUserService userService)
    {
        var dto = new UserDtoRequest
        {
            Name = name,
            Email = email,
            Password = password,
            Department = department,
        };
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .Select(x => x.ErrorMessage);
            return new CreateUserPayload{Errors = errors};
        }
        var result = await userService.RegisterUser(dto);
        
        return !result!.IsSuccess 
            ? new CreateUserPayload{Errors = [result.AppError.Message] } 
            : new CreateUserPayload{Id = result.Value};
    }
}