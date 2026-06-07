using System.Security.Claims;

namespace ECommerce.Common.Helpers
{
    /// <summary>
    /// Extension methods to extract UserId from HttpContext (JWT Claims)
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Extract UserId from JWT Claims (NameIdentifier)
        /// IMPORTANT: We NEVER pass UserId in request body — we always extract it from the token
        /// </summary>
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("UserId not found in token.");
        }

        public static string GetUserEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value
                ?? throw new UnauthorizedAccessException("Email not found in token.");
        }

        public static string GetUserRole(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Role)?.Value
                ?? throw new UnauthorizedAccessException("Role not found in token.");
        }
    }
}
