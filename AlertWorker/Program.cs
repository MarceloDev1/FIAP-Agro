using Microsoft.EntityFrameworkCore;
using FIAP.Agro.AlertWorker.Infrastructure.Data;
using FIAP.Agro.AlertWorker.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AlertDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHostedService<SensorReadingConsumer>();

var host = builder.Build();
host.Run();
