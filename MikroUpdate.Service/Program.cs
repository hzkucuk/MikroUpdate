using MikroUpdate.Service;
using MikroUpdate.Shared.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "MikroUpdateService";
});

builder.Logging.AddDiagnosticFileLogger("Service");

builder.Services.AddHostedService<UpdateWorker>();

var host = builder.Build();
host.Run();
