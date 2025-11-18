using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MiniHR.WebAPI.Models;

namespace MiniHR.WebAPI.Filters
{
    public class SuccessResponseFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is OkObjectResult okResult)
            {
                var originalValue = okResult.Value;

                if (originalValue is ApiResponse<object> apiResponse && !apiResponse.Success)
                {
                    return;
                }

                if (originalValue != null)
                {
                    var successMethod = typeof(ApiResponse<>)
                        .MakeGenericType(originalValue.GetType())
                        .GetMethod(nameof(ApiResponse<object>.SuccessResponse));

                    if (successMethod != null)
                    {
                        var wrappedResponse = successMethod.Invoke(null, new[] { originalValue });
                        context.Result = new OkObjectResult(wrappedResponse);
                    }
                }
                else
                {
                    context.Result = new OkObjectResult(ApiResponse<object>.SuccessResponse(null));
                }
            }
            else if (context.Result is CreatedAtActionResult createdResult)
            {
                var originalValue = createdResult.Value;

                if (originalValue != null)
                {
                    var successMethod = typeof(ApiResponse<>)
                        .MakeGenericType(originalValue.GetType())
                        .GetMethod(nameof(ApiResponse<object>.SuccessResponse));

                    if (successMethod != null)
                    {
                        var wrappedResponse = successMethod.Invoke(null, new[] { originalValue });
                        createdResult.Value = wrappedResponse;
                    }
                }
                else
                {
                    createdResult.Value = ApiResponse<object>.SuccessResponse(null);
                }
            }
            else if (context.Result is OkResult)
            {
                context.Result = new OkObjectResult(ApiResponse<object>.SuccessResponse(null));
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            
        }
    }
}