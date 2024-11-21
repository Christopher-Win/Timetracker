// Written by: Chris N.
using System.Security.Claims;
using TimeTracker.Services;

namespace TimeTracker.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserNetId(this ClaimsPrincipal user) // This method is an extension method for ClaimsPrincipal
        {
            var userNetIdClaim = user.FindFirst(ClaimTypes.NameIdentifier); // Find the claim with the type "NameIdentifier"
            if (userNetIdClaim == null) // If the claim is not found, throw an UnauthorizedAccessException
            {
                throw new UnauthorizedAccessException("User ID claim not found."); // If the claim is not found, throw an UnauthorizedAccessException
            }
            return (userNetIdClaim.Value); // Return the value of the claim for the user's NetID
        }
    }
}