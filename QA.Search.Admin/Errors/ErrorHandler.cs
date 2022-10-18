using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

                if (exceptionHandlerPathFeature?.Error is BusinessError)
                {
                    context.Response.StatusCode = 400;
                    context.Response.ContentType = "application/json";

                    var response = new ValidationProblemDetails
                    {
                        Title = exceptionHandlerPathFeature.Error.Message,
                        Status = 400
                    };

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                }
            });
        }
    }
}
