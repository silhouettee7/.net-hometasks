using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Quartz;
using RESTAuth.Api.CustomSessionAuth;
using RESTAuth.Api.Utils;
using RESTAuth.Application.Generators;
using RESTAuth.Application.Jobs;
using RESTAuth.Application.Services;
using RESTAuth.Domain.Abstractions.Generators;
using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Abstractions.Services;
using RESTAuth.Domain.Entities;
using RESTAuth.Persistence.DataBase;
using RESTAuth.Persistence.DataBase.Repositories;

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
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IQueryBuilder<User, Guid>, QueryBuilder<User, Guid>>();
        return services;
    }

    public static IServiceCollection AddStorage(this IServiceCollection services)
    {
        return services.AddDbContext<AppDbContext>();
    }
    public static IServiceCollection AddAuth(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme,options =>
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
            })
            .AddScheme<AuthenticationSchemeOptions, SessionTokenAuthenticationHandler>(
                SessionTokenDefaults.AuthenticationScheme, _ => { });
        services.AddAuthorization();
        return services;
    }

    public static IServiceCollection AddUtils(this IServiceCollection services)
    {
        services.AddSingleton<HttpResponseConvertingUtil>();
        return services;
    }

    public static IServiceCollection AddJobs(this IServiceCollection services)
    {
        services.AddQuartz(q =>
        {
            q.AddJob<CreateUsersJob>(opts => opts
                .WithIdentity("initCreateUsersJob", "createUsersJobs")
                .UsingJobData("usersCount",50));
            q.AddTrigger(opts => opts
                .WithIdentity("initCreateUsersTrigger", "createUsersTriggers")
                .ForJob("initCreateUsersJob", "createUsersJobs")
                .StartAt(DateBuilder.FutureDate(20, IntervalUnit.Second)));
        });
        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        return services;
    }

    public static IServiceCollection AddDataGenerators(this IServiceCollection services)
    {
        services.AddSingleton<ITestDataGenerator, TestDataGenerator>();
        return services;
    }
}