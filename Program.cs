using SQLAuditWatcherJsonService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "SQLAuditWatcherJson";
});
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
