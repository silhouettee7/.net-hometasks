using FluentValidation;
using RESTAuth.Api.Endpoints;
using RESTAuth.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddStorage();
builder.Services.AddServices();
builder.Services.AddRepositories();
builder.Services.AddUtils();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddAuth();
//builder.Services.AddJobs();
builder.Services.AddDataGenerators();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

var group = app.MapGroup("/api/v1/");
group.MapUsersEndpoints();
group.MapAuthEndpoints();

app.Run();

