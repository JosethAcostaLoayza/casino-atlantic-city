using Authentication.Application.Interfaces;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Authentication.Infrastructure.Services;

public class PasswordHasherService : IPasswordHasherService
{
    private readonly PasswordHasher<Usuario> _passwordHasher = new();

    public string Hash(string password)
    {
        var usuario = new Usuario(
            "temporary",
            string.Empty,
            "User");

        return _passwordHasher.HashPassword(usuario, password);
    }

    public bool Verify(string password, string passwordHash)
    {
        var usuario = new Usuario(
            "temporary",
            passwordHash,
            "User");

        var result = _passwordHasher.VerifyHashedPassword(
            usuario,
            passwordHash,
            password);

        return result == PasswordVerificationResult.Success ||
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}