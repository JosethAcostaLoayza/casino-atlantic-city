using Authentication.Application.Interfaces;

namespace Authentication.Application.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtService _jwtService;

    public AuthenticationService(
        IUsuarioRepository usuarioRepository,IPasswordHasherService passwordHasher,IJwtService jwtService){
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<string?> LoginAsync(string email, string password)
    {
        var usuario = await _usuarioRepository.GetByEmailAsync(email);

        if (usuario is null)
            return null;

        var passwordValido = _passwordHasher.Verify(password,usuario.PasswordHash);

        if (!passwordValido)
            return null;

        return _jwtService.GenerateToken(
            usuario.Id,
            usuario.Email,
            usuario.Rol);
    }
}