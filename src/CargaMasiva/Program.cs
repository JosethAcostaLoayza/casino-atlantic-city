using CargaMasiva.Messaging;
using CargaMasiva.Storage;
using CargaMasiva.Processing;
using CargaMasiva.Persistence;
using CargaMasiva.Integration;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddHttpClient<SeaweedFileStorage>(client =>
{
    client.BaseAddress = new Uri("http://seaweedfs:8888");
});

builder.Services.AddHttpClient<ControlStatusClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Control:BaseUrl"]!
    );
});

builder.Services.AddDbContext<CargaMasivaDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("CargaMasivaDb")
    )
);

builder.Services.AddHostedService<RabbitMqConsumer>();

builder.Services.AddSingleton<ExcelProcessor>();

builder.Services.AddSingleton<
    INotificacionesPublisher,
    RabbitMqNotificacionesPublisher
>();

var app = builder.Build();

using (var scope = app.Services.CreateScope()){
    var dbContext = scope.ServiceProvider.GetRequiredService<CargaMasivaDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.MapControllers();

app.Run();