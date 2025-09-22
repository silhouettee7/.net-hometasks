using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using RESTAuth.Api.Filters;
using RESTAuth.Api.Utils;
using RESTAuth.Domain.Abstractions.Services;
using RESTAuth.Domain.Dtos;

namespace RESTAuth.Api.Endpoints;

public static class AuthEndpointsExt
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/login",
                async (UserLoginDto dto, HttpContext context, IAuthService authService, HttpResponseConvertingUtil converter) =>
                {
                    var result = await authService.Login(dto);
                    if (!result.IsSuccess)
                    {
                        return converter.CreateResponse(result);
                    }

                    var claims = new List<Claim>
                    {
                        new (ClaimTypes.NameIdentifier, result.Value.Id.ToString()),
                        new (ClaimTypes.Role, result.Value.Role)
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return Results.Ok();
                })
            .AddEndpointFilter<ValidationFilter<UserLoginDto>>();

        return group;
    }
}