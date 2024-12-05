using API.Errors;
using System.Net;
using System.Text.Json;

namespace API.Middleware
{
    public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        public async Task InvokeAsync(HttpContext httpContext)
        {
			try
			{
				await next(httpContext);
			}
			catch (Exception ex)
			{
				logger.LogError(ex.Message);
				httpContext.Response.ContentType = "application/json";
				httpContext.Response.StatusCode=(int)HttpStatusCode.InternalServerError;

				var response = env.IsDevelopment() ? new ApiExceptions(httpContext.Response.StatusCode, ex.Message, ex.StackTrace)
					:new ApiExceptions(httpContext.Response.StatusCode,ex.Message,"Internal server error");
				var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
				
				var json= JsonSerializer.Serialize(response, options);

				await httpContext.Response.WriteAsync(json);
			
			}
        }
    }
}
