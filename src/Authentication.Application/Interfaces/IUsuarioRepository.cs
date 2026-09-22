using Authentication.Domain.Entities;

namespace Authentication.Application.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email);

    Task AddAsync(Usuario usuario);
}