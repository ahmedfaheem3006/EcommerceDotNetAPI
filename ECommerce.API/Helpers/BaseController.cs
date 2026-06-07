using Microsoft.AspNetCore.Mvc;
using ECommerce.Common.Result;

namespace ECommerce.API.Helpers
{
    /// <summary>
    /// BaseController: provides consistent API response formatting
    /// 
    /// لماذا: عشان كل الـ API Responses تكون بنفس الشكل
    /// بنستخدم Result<T> Pattern وبنرجع HTTP Status Code مناسب
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// Returns 200 OK with the result data
        /// </summary>
        protected IActionResult HandleResult<T>(Result<T> result)
        {
            if (result.IsFailure)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns 200 OK with non-generic result
        /// </summary>
        protected IActionResult HandleResult(Result result)
        {
            if (result.IsFailure)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        /// <summary>
        /// Returns 201 Created
        /// </summary>
        protected IActionResult HandleCreated<T>(Result<T> result, string routeName, object routeValues)
        {
            if (result.IsFailure)
            {
                return BadRequest(result);
            }

            return CreatedAtRoute(routeName, routeValues, result);
        }

        /// <summary>
        /// Returns 404 Not Found
        /// </summary>
        protected IActionResult HandleNotFound(string message = "Resource not found")
        {
            return NotFound(new Result { IsSuccess = false, Message = message });
        }

        /// <summary>
        /// Returns 401 Unauthorized
        /// </summary>
        protected IActionResult HandleUnauthorized(string message = "Unauthorized")
        {
            return Unauthorized(new Result { IsSuccess = false, Message = message });
        }
    }
}
