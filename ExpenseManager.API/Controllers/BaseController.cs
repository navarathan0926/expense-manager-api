using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ExpenseManager.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected Guid CurrentUserId
    {
        get
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

            if (string.IsNullOrWhiteSpace(userId))
                throw new UnauthorizedException("User identifier claim is missing.");

            return Guid.Parse(userId);
        }
    }

    protected bool IsAdmin =>
        User.IsInRole("Admin");
}
