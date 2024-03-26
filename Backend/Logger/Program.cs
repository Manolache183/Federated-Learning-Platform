using Microsoft.Azure.Cosmos;

IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton(new CosmosClient(
    configuration.GetConnectionString("CosmosEndpoint"),
    configuration.GetConnectionString("CosmosKey")));

var app = builder.Build();

app.UseAuthorization();

app.UseAuthorization();
app.MapControllers();

app.UseCors(policy => policy
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
);

app.Run();
