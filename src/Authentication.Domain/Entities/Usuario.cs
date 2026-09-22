namespace Authentication.Domain.Entities;

public class Usuario
{
    public Guid Id { get; private set; }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public string Rol { get; private set; }

    public Usuario(string email, string passwordHash, string rol)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        Rol = rol;
    }
}