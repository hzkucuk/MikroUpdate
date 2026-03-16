using MikroUpdate.Service;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "MikroUpdateService";
});
builder.Services.AddHostedService<UpdateWorker>();

var host = builder.Build();
host.Run();
