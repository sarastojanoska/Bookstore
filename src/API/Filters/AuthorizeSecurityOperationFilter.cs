using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace API.Filters;

public class AuthorizeSecurityOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var policy = context.MethodInfo.GetCustomAttributes(true)
            .OfType<AuthorizeAttribute>().FirstOrDefault()?.Policy
            ?? context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
                .OfType<AuthorizeAttribute>().FirstOrDefault()?.Policy;

        var schemeId = policy switch
        {
            "BooksCrud" => "client_credentials",
            "BooksSearch" => "implicit",
            _ => null
        };

        if (schemeId is null) return;

        var scheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = schemeId }
        };

        operation.Security = [new() { [scheme] = Array.Empty<string>() }];
    }
}
