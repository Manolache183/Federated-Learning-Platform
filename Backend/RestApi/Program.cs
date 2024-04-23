using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RestApi.Firebase;
using RestApi.HttpClients;
using RestApi.MessageBroker;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();

var mnistClientBaseAddress = builder.Configuration.GetConnectionString("MnistAggregatorEndpoint");
var loggerBaseAddress = builder.Configuration.GetConnectionString("LoggerEndpoint");
var authenticatorBaseAddress = builder.Configuration.GetConnectionString("AuthenticatorEndpoint");

if (string.IsNullOrEmpty(mnistClientBaseAddress) || string.IsNullOrEmpty(loggerBaseAddress) || string.IsNullOrEmpty(authenticatorBaseAddress))
{
    Console.WriteLine("One or more endpoints are missing from the configuration file!");
    return;
}

builder.Services.AddAuthentication(authOptions =>
{
    authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(bearerOptions =>
{
    bearerOptions.TokenValidationParameters = new TokenValidationParameters
    {
        //ValidIssuer = configuration.GetSection("JwtSettins")["Issuer"],
        //ValidAudience = configuration.GetSection("JwtSettings")["Audience"],
        //IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("JwtSettings")["SecretKey"]!)),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidateIssuer = true,
        ValidateAudience = true
    };
});

builder.Services.AddSingleton<EventBus>();
builder.Services.AddSingleton<StorageService>();

builder.Services.AddControllers();
builder.Services.AddHttpClient<IAggregatorService, MnistService>(httpClient =>
    httpClient.BaseAddress = new Uri(mnistClientBaseAddress)
);
builder.Services.AddHttpClient<ILoggerService, LoggerService>(httpClient =>
    httpClient.BaseAddress = new Uri(loggerBaseAddress)
);
builder.Services.AddHttpClient<IAuthenticatorService, AuthenticatorService>(httpClient =>
    httpClient.BaseAddress = new Uri(authenticatorBaseAddress)
);

builder.Services.AddCors();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseCors(policy => policy
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
);

app.Run();
