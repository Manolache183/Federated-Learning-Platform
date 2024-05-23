using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RestApi.Firebase;
using RestApi.HttpClients;
using RestApi.Learning;
using RestApi.MessageBroker;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();

var loggerBaseAddress = configuration.GetConnectionString("LoggerEndpoint");
var authenticatorBaseAddress = configuration.GetConnectionString("AuthenticatorEndpoint");

if (string.IsNullOrEmpty(loggerBaseAddress) || string.IsNullOrEmpty(authenticatorBaseAddress))
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
        ValidIssuer = configuration.GetSection("JwtSettings")["Issuer"],
        ValidAudience = configuration.GetSection("JwtSettings")["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("JwtSettings")["SecretKey"]!)),
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,
        ValidateIssuer = true,
        ValidateAudience = true
    };
});

builder.Services.AddSingleton<CacheService>();
builder.Services.AddSingleton<EventBus>();

builder.Services.AddHttpClient<StorageService>();
builder.Services.AddHttpClient<ILoggerService, LoggerService>(httpClient =>
    httpClient.BaseAddress = new Uri(loggerBaseAddress)
);
builder.Services.AddHttpClient<IAuthenticatorService, AuthenticatorService>(httpClient =>
    httpClient.BaseAddress = new Uri(authenticatorBaseAddress)
);

builder.Services.AddScoped<LearningManager>();

builder.Services.AddControllers();

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
