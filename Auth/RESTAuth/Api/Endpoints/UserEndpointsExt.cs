using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using RESTAuth.Api.Enums;
using RESTAuth.Api.Filters;
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
            async (UserDtoRequest userRegister, HttpResponseCreator creator, IUserService userService) =>
                creator.CreateResponse(await userService.RegisterUser(userRegister)))
            .AddEndpointFilter<ValidationFilter<UserDtoRequest>>();
        
        group.MapPut("/", 
                async (UserDtoRequest userEdit, HttpContext context,HttpResponseCreator creator,
                    IUserService userService) =>
                {
                    var userIdClaim = context.User.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                    Guid.TryParse(userIdClaim?.Value, out Guid userId);
                    return creator.CreateResponse(await userService.EditUser(userId, userEdit)); 
                })
            .AddEndpointFilter<ValidationFilter<UserDtoRequest>>()
            .RequireAuthorization();
        
        group.MapDelete("/",
            async (HttpContext context, HttpResponseCreator creator, IUserService userService) =>
            {
                var userIdClaim = context.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                Guid.TryParse(userIdClaim?.Value, out Guid userId);
                return creator.CreateResponse(await userService.DeleteUser(userId));
            })
            .RequireAuthorization();
        
        group.MapGet("/",
            async (DateTime from, DateTime to, UserDateOption option, HttpResponseCreator creator,
                IUserService userService) =>
            {
                switch (option)
                {
                    case UserDateOption.Registration:
                        return creator.CreateResponse(await userService.GetUsersForPeriodByRegistrationDate(from, to));
                    case UserDateOption.Updating:
                        return creator.CreateResponse(await userService.GetUsersForPeriodByUpdatingDate(from, to));
                    default:
                        throw new ArgumentOutOfRangeException(nameof(option), option, null);
                }
            })
            .RequireAuthorization(new AuthorizeAttribute{Roles = "Admin"});
        
        
        return group;
    }
}