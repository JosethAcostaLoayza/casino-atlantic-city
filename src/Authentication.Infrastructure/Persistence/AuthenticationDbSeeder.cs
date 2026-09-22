using Authentication.Application.Interfaces;
using Authentication.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Infrastructure.Persistence;

public static class AuthenticationDbSeeder{
    public static async Task SeedAsync(AuthenticationDbContext context,IPasswordHasherService passwordHasher){
        if (await context.Usuarios.AnyAsync())
            return;

        var usuario = new Usuario("admin@atlanticcity.com",passwordHasher.Hash("Admin123*"),"Admin");

        await context.Usuarios.AddAsync(usuario);
        await context.SaveChangesAsync();
    }
}