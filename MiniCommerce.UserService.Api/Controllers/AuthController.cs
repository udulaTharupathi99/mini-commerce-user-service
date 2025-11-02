using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniCommerce.UserService.Application.DTOs;
using MiniCommerce.UserService.Application.Interfaces;
using MiniCommerce.UserService.Domain.Entities;

namespace MiniCommerce.UserService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly ILogger<AuthController> _log;

        public AuthController(IAuthService auth, ILogger<AuthController> log)
        {
            _auth = auth;
            _log = log;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
        {
            try
            {
                var user = await _auth.RegisterAsync(req, ct);
                return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
            }
            catch (InvalidOperationException ex)
            {
                _log.LogWarning(ex, "Register failed for {Email}", req.Email);
                return Conflict(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
        {
            try
            {
                var (user, token) = await _auth.LoginAsync(req, ct);
                return Ok(new { user, token });
            }
            catch (UnauthorizedAccessException ex)
            {
                _log.LogWarning("Login failed for {Email}", req.Email);
                return Unauthorized(new { message = "invalid_credentials" });
            }
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> Me(CancellationToken ct)
        {
            var sub = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (!Guid.TryParse(sub, out var id))
                return Unauthorized();

            var user = await _auth.GetByIdAsync(id, ct);
            if (user == null) return NotFound();
            return Ok(user);
        }
    }
}
