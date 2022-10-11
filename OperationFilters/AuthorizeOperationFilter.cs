using Microsoft.OpenApi.Models;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace art_gallery_api
{
  public class AuthorizeOperationFilter : IOperationFilter
  {
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
      if (context.MethodInfo.GetCustomAttributes().OfType<AuthorizeAttribute>().Any() || context.MethodInfo.DeclaringType != null && context.MethodInfo.DeclaringType.GetCustomAttributes().OfType<AuthorizeAttribute>().Any())
      {
        operation.Responses.Add(StatusCodes.Status401Unauthorized.ToString(), new OpenApiResponse { Description = "Unauthorized" });
        operation.Responses.Add(StatusCodes.Status403Forbidden.ToString(), new OpenApiResponse { Description = "Forbidden" });

        operation.Security = new List<OpenApiSecurityRequirement>
        {
          new()
          {
            {
              new OpenApiSecurityScheme
              {
                Reference = new OpenApiReference
                {
                  Type = ReferenceType.SecurityScheme,
                  Id = "oauth2"
                }
              },
              new List<string>
              {
                "api"
              }
            }
          }
        };
      }
    }
  }
}
