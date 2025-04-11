using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using SidoAgung.ApiSaga.Infrastruktur.Models;

[Route("wongnormal/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (request.Username == "admin" && request.Password == "password123") 
        {
            var token = _authService.GenerateToken(request.Username);
            return Ok(new { Token = token });
        }
        return Unauthorized("Username atau password salah!");
    }

    [Authorize]
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token))
            return BadRequest("Token tidak ditemukan!");

        _authService.RevokeToken(token);
        return Ok("Logout berhasil, token dibatalkan.");
    }

    [Authorize]
    [HttpGet("check-token")]
    public IActionResult CheckToken()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (_authService.IsTokenRevoked(token))
            return Unauthorized("Token telah dibatalkan!");

        return Ok("Token masih valid.");
    }
}


