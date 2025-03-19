// Extensions/ExceptionMiddlewareExtensions.cs
using Calendar.API.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Calendar.API.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        /// <summary>
        /// 設定統一的例外處理中間件
        /// </summary>
        public static void UseGlobalExceptionHandler(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        var exception = contextFeature.Error;

                        // 根據異常類型設定不同的狀態碼和錯誤訊息
                        (HttpStatusCode statusCode, string message) = exception switch
                        {
                            EntityNotFoundException ex => (HttpStatusCode.NotFound, ex.Message),
                            DuplicateEntityException ex => (HttpStatusCode.Conflict, ex.Message),
                            BusinessValidationException ex => (HttpStatusCode.BadRequest, ex.Message),
                            AuthenticationException ex => (HttpStatusCode.Unauthorized, ex.Message),
                            ServiceException ex => (HttpStatusCode.InternalServerError, ex.Message),
                            RepositoryException ex => (HttpStatusCode.InternalServerError, env.IsDevelopment() ? ex.Message : "資料庫操作錯誤"),
                            _ => (HttpStatusCode.InternalServerError, env.IsDevelopment() ? exception.Message : "內部伺服器錯誤")
                        };

                        context.Response.StatusCode = (int)statusCode;

                        var response = new
                        {
                            statusCode = context.Response.StatusCode,
                            message,
                            stackTrace = env.IsDevelopment() ? exception.StackTrace : null
                        };

                        var jsonResponse = JsonSerializer.Serialize(response);
                        await context.Response.WriteAsync(jsonResponse);
                    }
                });
            });
        }
    }
}