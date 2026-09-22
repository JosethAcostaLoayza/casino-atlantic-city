using Authentication.Application.Interfaces;
using Authentication.Models;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(IAuthenticationService authenticationService){
        _authenticationService = authenticationService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request){
        var token = await _authenticationService.LoginAsync(request.Email,request.Password);

        if (token is null){
            return Unauthorized(new
            {
                message = "Credenciales inválidas."
            });
        }

        return Ok(new
        {
            accessToken = token
        });
    }
}