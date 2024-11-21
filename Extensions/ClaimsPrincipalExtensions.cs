// Written by: Chris N.
using System.Security.Claims;
using TimeTracker.Services;

namespace TimeTracker.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserNetId(this ClaimsPrincipal user)
        {
            var userNetIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (userNetIdClaim == null)
            {
                throw new UnauthorizedAccessException("User ID claim not found.");
            }
            return (userNetIdClaim.Value); 
        }
    }
}