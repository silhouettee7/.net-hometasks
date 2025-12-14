using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using RESTAuth.Api.CustomSessionAuth;
using RESTAuth.Api.Enums;
using RESTAuth.Api.Filters;
using RESTAuth.Api.Models;
using RESTAuth.Api.Utils;
using RESTAuth.Application.Rabbit.Producers;
using RESTAuth.Domain.Abstractions.Services;
using RESTAuth.Domain.Dtos;
using Shared.Rabbit.Models;

namespace RESTAuth.Api.Endpoints;

public static class UserEndpointsExt
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users");

        group.MapPost("/registration",
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
                return dto.Option switch
                {
                    UserDateOption.Registration => converter.CreateResponse(
                        await userService.GetUsersPageForPeriodByRegistrationDate(dto.CursorPaginationRequest, dto.From,
                            dto.To)),
                    UserDateOption.Updating => converter.CreateResponse(
                        await userService.GetUsersPageForPeriodByUpdatingDate(dto.CursorPaginationRequest, dto.From,
                            dto.To)),
                    _ => throw new ArgumentOutOfRangeException(nameof(dto.Option), dto.Option, null)
                };
            })
            .RequireAuthorization(new AuthorizeAttribute
            {
                Roles = "Admin"
            });

        group.MapGet("/salaries",
            async (IUserService userService, HttpResponseConvertingUtil converter) => 
                converter.CreateResponse(await userService.GetUserDepartmentAverageSalaries()))
            .RequireAuthorization(new AuthorizeAttribute
            {
                Roles = "Admin"
            });

        group.MapPost("/telegram/report/create", async (HttpContext context, ReportQueueProducer producer) =>
        {
            long.TryParse(context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                out long telegramChatId);
            var reportRequest = new ReportRequest
            {
                ReportId = Guid.NewGuid(),
                TelegramChatId = telegramChatId,
            };
            await producer.EnqueueReportAsync(reportRequest);
            return Results.Created();
        })
        .RequireAuthorization(new AuthorizeAttribute
        {
            Roles = "Admin",
            AuthenticationSchemes = SessionTokenDefaults.AuthenticationScheme,
        });
        
        return group;
    }
}