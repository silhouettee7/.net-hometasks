using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Quartz;
using RESTAuth.Api.CustomSessionAuth;
using RESTAuth.Api.Utils;
using RESTAuth.Api.Workers;
using RESTAuth.Application.Generators;
using RESTAuth.Application.Graph;
using RESTAuth.Application.Graph.Types;
using RESTAuth.Application.Jobs;
using RESTAuth.Application.Rabbit.Consumers;
using RESTAuth.Application.Rabbit.Producers;
using RESTAuth.Application.Services;
using RESTAuth.Domain.Abstractions.Generators;
using RESTAuth.Domain.Abstractions.Repositories;
using RESTAuth.Domain.Abstractions.Services;
using RESTAuth.Domain.Entities;
using RESTAuth.Persistence.DataBase;
using RESTAuth.Persistence.DataBase.Repositories;
using RESTAuth.Persistence.FileStorage;
using Shared.Configuration.Abstractions;
using Shared.Rabbit.Abstractions;
using Shared.Rabbit.Utils;

namespace RESTAuth.Api.Extensions;

public static class ServiceCollectionExt
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IFileReportService, FileReportService>();
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

    public static IServiceCollection AddRabbit(this IServiceCollection services)
    {
        services.AddSingleton<RabbitMqChannelAccessor>();
        services.AddSingleton<IRabbitMqChannelAccessor, RabbitMqChannelAccessor>(sp => 
            sp.GetRequiredService<RabbitMqChannelAccessor>());
        services.AddSingleton<IInitializable, RabbitMqChannelAccessor>(sp => 
            sp.GetRequiredService<RabbitMqChannelAccessor>());
        services.AddScoped<ReportQueueProducer>();
        services.AddScoped<ReportNotificationQueueProducer>();
        services.AddScoped<ReportQueueConsumer>();
        return services;
    }

    public static IServiceCollection AddHostedServices(this IServiceCollection services)
    {
        services.AddHostedService<InitializerHostedService>();
        services.AddHostedService<ReportQueueConsumerBackgroundService>();
        return services;
    }

    public static IServiceCollection AddMinio(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MinioOptions>(configuration.GetSection("Minio"));
        services.AddScoped<IFileStorageService, MinioService>();
        return services;
    }

    public static IServiceCollection AddGraphQL(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddType<UserType>()
            .AddType<UserAddressType>()
            .AddType<UserProfileType>()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>()
            .AddProjections()
            .AddFiltering()
            .AddSorting()
            .RegisterDbContextFactory<AppDbContext>();

        return services;
    }
    
}