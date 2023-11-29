using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace QA.Search.Admin.Errors
{
    public static class ErrorHandler
    {
        public static void Handle(IApplicationBuilder app)
        {
            app.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

                if (exceptionHandlerPathFeature?.Error is BusinessError || exceptionHandlerPathFeature?.Error is AuthError)
                {
                    int statusCode = exceptionHandlerPathFeature?.Error switch
                    {
                        BusinessError => 400,
                        AuthError => 401,
                        _ => 500
                    };

                    context.Response.StatusCode = statusCode;
                    context.Response.ContentType = "application/json";

                    var response = new ValidationProblemDetails
                    {
                        Title = exceptionHandlerPathFeature.Error.Message,
                        Status = statusCode
                    };

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                }
            });
        }
    }
}
