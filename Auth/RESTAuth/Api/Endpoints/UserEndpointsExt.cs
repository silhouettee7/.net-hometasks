using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using RESTAuth.Api.Enums;
using RESTAuth.Api.Filters;
using RESTAuth.Api.Models;
using RESTAuth.Api.Utils;
using RESTAuth.Domain.Abstractions.Services;
using RESTAuth.Domain.Dtos;

namespace RESTAuth.Api.Endpoints;

public static class UserEndpointsExt
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users");

        group.MapPost("/",
            async (UserDtoRequest userRegister, HttpResponseConvertingUtil converter, IUserService userService) =>
                converter.CreateResponse(await userService.RegisterUser(userRegister)))
            .AddEndpointFilter<ValidationFilter<UserDtoRequest>>();
        
        group.MapPut("/", 
                async (UserDtoRequest userEdit, HttpContext context,HttpResponseConvertingUtil converter,
                    IUserService userService) =>
                {
                    var userIdClaim = context.User.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                    Guid.TryParse(userIdClaim?.Value, out Guid userId);
                    return converter.CreateResponse(await userService.EditUser(userId, userEdit)); 
                })
            .AddEndpointFilter<ValidationFilter<UserDtoRequest>>()
            .RequireAuthorization();
        
        group.MapDelete("/",
            async (HttpContext context, HttpResponseConvertingUtil converter, IUserService userService) =>
            {
                var userIdClaim = context.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                Guid.TryParse(userIdClaim?.Value, out Guid userId);
                return converter.CreateResponse(await userService.DeleteUser(userId));
            })
            .RequireAuthorization();
        
        group.MapPost("/",
            async (UsersPageWithPeriodDateDto dto, HttpResponseConvertingUtil converter, IUserService userService) =>
            {
                switch (dto.Option)
                {
                    case UserDateOption.Registration:
                        return converter.CreateResponse(
                            await userService.GetUsersPageForPeriodByRegistrationDate(
                                dto.CursorPaginationRequest, dto.From,dto.To));
                    case UserDateOption.Updating:
                        return converter.CreateResponse(
                            await userService.GetUsersPageForPeriodByUpdatingDate(
                                dto.CursorPaginationRequest, dto.From, dto.To));
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dto.Option), dto.Option, null);
                }
            })
            .RequireAuthorization(new AuthorizeAttribute{Roles = "Admin"});

        group.MapGet("/salaries",
            async (IUserService userService, HttpResponseConvertingUtil converter) => 
                converter.CreateResponse(await userService.GetUserDepartmentAverageSalaries()));
        
        return group;
    }
}