using ConsoleApp;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Schedly.Core;

Console.WriteLine("App starting");
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddHostedService<HostedScheduler>();
builder.Services.AddSingleton<Job, SampleJob>();

var host = builder.Build();
host.Run();
