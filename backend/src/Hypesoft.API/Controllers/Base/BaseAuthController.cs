using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hypesoft.API.Controllers.Base;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
public abstract class BaseAuthController : ControllerBase
{
    protected string? UserId => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    protected string? UserEmail => User.FindFirst(ClaimTypes.Email)?.Value;
    protected string? UserName => User.FindFirst(ClaimTypes.Name)?.Value;
    protected IEnumerable<string> UserRoles => User.FindAll(ClaimTypes.Role).Select(c => c.Value);

    protected bool IsInRole(string role)
    {
        return User.IsInRole(role) || User.HasClaim(c => c.Type == "realm_roles" && c.Value.Contains(role));
    }

    protected IActionResult Forbidden(string message = "Acesso negado")
    {
        return StatusCode(StatusCodes.Status403Forbidden, new { message });
    }

    protected IActionResult Unauthorized(string message = "Não autorizado")
    {
        return StatusCode(StatusCodes.Status401Unauthorized, new { message });
    }

    protected IActionResult InternalError(string message = "Ocorreu um erro interno")
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new { message });
    }
}
