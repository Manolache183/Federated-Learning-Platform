using RestApi.HttpClients;
using System.Configuration;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var mnistClientBaseAddress = configuration.GetConnectionString("MnistAggregatorEndpoint");
var loggerBaseAddress = configuration.GetConnectionString("LoggerEndpoint");

if (mnistClientBaseAddress == null || loggerBaseAddress == null)
{
    throw new ConfigurationErrorsException("At least one connection string is null");
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient<IAggregatorService, MnistService>(httpClient =>
    httpClient.BaseAddress = new Uri(mnistClientBaseAddress)
);
builder.Services.AddHttpClient<ILoggerService, LoggerService>(httpClient =>
    httpClient.BaseAddress = new Uri(loggerBaseAddress)
);

builder.Services.AddSignalR();
builder.Services.AddCors();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.UseCors(policy => policy
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
);

// https redirection

app.Run();
