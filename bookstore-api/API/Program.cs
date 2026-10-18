using API.Extensions;
using API.Filters;
using API.Settings;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

// TODO: Settings bindings for DI , maybe remove later
builder.Services.AddOptions<IdentitySettings>()
    .Bind(builder.Configuration.GetSection("IdentityServer"))
    .ValidateOnStart();

var identitySettings = builder.Configuration.GetSection("IdentityServer").Get<IdentitySettings>()
    ?? throw new InvalidOperationException("IdentityServer settings are not configured.");

builder.AddServiceDefaults();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims mapping

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
                .RequireAssertion(context => context.User.HasClaim(c => c.Type == "sub")));

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
