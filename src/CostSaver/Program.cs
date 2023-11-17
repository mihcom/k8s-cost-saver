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
}

// static void ConfigureLogging(WebApplicationBuilder builder)
// {
//     builder.Host.UseSerilog();
//     
//     Log.Logger = new LoggerConfiguration()
//         .ReadFrom.Configuration(builder.Configuration)
//         .CreateLogger();
// }