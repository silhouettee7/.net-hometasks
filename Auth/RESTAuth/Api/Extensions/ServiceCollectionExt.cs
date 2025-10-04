using Microsoft.AspNetCore.Authentication.Cookies;
using RESTAuth.Api.Utils;
using RESTAuth.Application.Services;
using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Abstractions.Services;
using RESTAuth.Domain.Entities;
using RESTAuth.Persistence.InMemoryStorage;

namespace RESTAuth.Api.Extensions;

public static class ServiceCollectionExt
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICursorPaginationService<User, Guid>, CursorPaginationService<User, Guid>>();
        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, InMemoryUserRepository>();
        services.AddScoped<IQueryBuilder<User, Guid>, InMemoryQueryBuilder<User, Guid>>();
        return services;
    }

    public static IServiceCollection AddStorage(this IServiceCollection services)
    {
        return services.AddSingleton<LocalStorage<User, Guid>>();
    }
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = "/api/v1/auth/login";
                options.AccessDeniedPath = "/api/v1/auth/login";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToAccessDenied = context =>
                    {
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    },
                    OnRedirectToLogin = context =>
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    },
                };
            }); 
        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddUtils(this IServiceCollection services)
    {
        services.AddSingleton<HttpResponseConvertingUtil>();
        return services;
    }
}