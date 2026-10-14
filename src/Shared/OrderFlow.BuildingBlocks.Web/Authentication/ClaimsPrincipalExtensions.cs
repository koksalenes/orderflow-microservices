using System.Security.Claims;

namespace OrderFlow.BuildingBlocks.Web.Authentication;

public static class ClaimsPrincipalExtensions
{
    public static string UserId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        return principal.FindFirstValue(JwtAuthenticationExtensions.SubjectClaim)
            ?? throw new InvalidOperationException("The access token has no 'sub' claim.");
    }
}
