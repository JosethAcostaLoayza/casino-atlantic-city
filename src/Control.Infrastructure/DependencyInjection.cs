using Control.Application.Interfaces;
using Control.Infrastructure.Persistence;
using Control.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Control.Infrastructure.Storage;
using Control.Infrastructure.Messaging;

namespace Control.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services,IConfiguration configuration){
        services.AddDbContext<ControlDbContext>(options =>options.UseNpgsql(configuration.GetConnectionString("ControlDb")));
        services.AddScoped<ICargaArchivoRepository,CargaArchivoRepository>();

        services.AddHttpClient<IFileStorage, SeaweedFileStorage>(client =>{
            client.BaseAddress = new Uri("http://casino-seaweedfs:8888");
        });

        services.AddScoped<ICargaMasivaPublisher, RabbitMqCargaMasivaPublisher>();

        return services;
    }
}