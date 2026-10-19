using API.Extensions;
using API.Filters;
using API.Settings;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var identitySettings = builder.Configuration.GetSection("IdentityServer").Get<IdentitySettings>()
    ?? throw new InvalidOperationException("IdentityServer settings are not configured.");

builder.AddServiceDefaults();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = identitySettings.Authority;
        options.TokenValidationParameters = new() { ValidateAudience = true, ValidAudience = identitySettings.ApiName };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("BooksCrud", policy =>
        policy.RequireClaim("scope", "books.crud")
                .RequireAssertion(context => !context.User.HasClaim(c => c.Type == "sub")))

    .AddPolicy("BooksSearch", policy =>
        policy.RequireClaim("scope", "books.search")
                .RequireAuthenticatedUser());

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("client_credentials", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            ClientCredentials = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(identitySettings.AuthorizationUrl!),
                TokenUrl = new Uri(identitySettings.TokenUrl!),
                Scopes = new Dictionary<string, string> { ["books.crud"] = "CRUD access to books" }
            }
        }
    });

    options.AddSecurityDefinition("implicit", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(identitySettings.AuthorizationUrl!),
                Scopes = new Dictionary<string, string> { ["books.search"] = "Search access to books" }
            }
        }
    });

    options.OperationFilter<AuthorizeSecurityOperationFilter>();
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("bookstore")));

builder.Services.AddApplicationServices();

var app = builder.Build();

bool seed = true;

if (seed)
{
    using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    context.Database.Migrate();

    if (!context.Authors.Any())
    {
        context.Authors.AddRange(
            new Author { Name = "Author 1" },
            new Author { Name = "Author 2" },
            new Author { Name = "Author 3" }
        );
        context.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Books API v1");
        options.OAuthClientId("books.search.client");
        options.OAuthAppName("bookstore");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.MapDefaultEndpoints();

app.Run();

public partial class Program { }
