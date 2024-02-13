using RestApi.HttpClients;
using RestApi.Hubs;

const string mnistAggregatorUri = "http://aggregator:5000/upload_model";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddHttpClient<IAggregatorService, MnistService>(httpClient =>
    httpClient.BaseAddress = new Uri(mnistAggregatorUri)
    // configs to add
);

builder.Services.AddSignalR();
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();
app.MapControllers();

app.UseCors(policy => policy
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
);

app.MapHub<ClientHub>("clientHub");

app.Run();
