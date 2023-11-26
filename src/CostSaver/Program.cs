using System.Runtime.CompilerServices;
using System.Reflection;
using CostSaver.Infrastructure.Services;
using KubeOps.Operator;

[assembly:InternalsVisibleTo("CostSaver.UnitTests")]

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services);

var app = builder.Build();
app.UseKubernetesOperator();

await app.RunOperatorAsync(args);

return;

static void ConfigureServices(IServiceCollection services)
{
    services.AddKubernetesOperator();
    
    services.AddHostedService<DetectExpiredWorkloadsService>();
    services.AddMediatR(Assembly.GetExecutingAssembly());
}