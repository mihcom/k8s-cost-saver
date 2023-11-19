using System.Reflection;
using CostSaver.Infrastructure.Services;
using KubeOps.Operator;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services);
// ConfigureLogging(builder);

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

// static void ConfigureLogging(WebApplicationBuilder builder)
// {
//     builder.Host.UseSerilog();
//     
//     Log.Logger = new LoggerConfiguration()
//         .ReadFrom.Configuration(builder.Configuration)
//         .CreateLogger();
// }
