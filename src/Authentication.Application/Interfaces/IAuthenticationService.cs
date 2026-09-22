namespace Authentication.Application.Interfaces;

public interface IAuthenticationService
{
    Task<string?> LoginAsync(string email, string password);
}