using Microsoft.EntityFrameworkCore;
using FIAP.Agro.AlertWorker.Infrastructure.Data;
using FIAP.Agro.AlertWorker.Infrastructure.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AlertDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddHostedService<SensorReadingConsumer>();

var host = builder.Build();

// Nao chamar .Migrate() aqui - Alert API cuida disso.
// So aguarda tabela 'Alerts' existir (criada pelo Alert API).
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AlertDbContext>();
    for (int i = 0; i < 60; i++)
    {
        try { _ = db.Alerts.Count(); break; }
        catch { Thread.Sleep(5000); }
    }
}

host.Run();
