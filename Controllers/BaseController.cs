using AonFreelancing.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Twilio.TwiML.Messaging;

namespace AonFreelancing.Controllers
{
    public class BaseController : ControllerBase
    {

        protected ApiResponse<T> CreateSuccessResponse<T>(T data)
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Results = data
            };
        }

        protected ApiResponse<object> CreateErrorResponse(string code, string message)
        {
            return new ApiResponse<object>
            {
                Errors = [new Error { Code = code, Message = message }]
            };
        }
        protected IActionResult CustomBadRequest()
        {
            var errors = ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(e => new Error
                {
                    Code = StatusCodes.Status400BadRequest.ToString(),
                    Message = e.ErrorMessage
                })
                .ToList();

            return BadRequest(new ApiResponse<object>
            {
                Errors = errors
            });
        }
    }
}
