using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using RestApi.HttpClients;
using RestApi.Learning;
using RestApi.MessageBroker;
using System.Text;
using RestApi.Firebase;
using Polly.Extensions.Http;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true).AddEnvironmentVariables();

var loggerBaseAddress = configuration.GetConnectionString("LoggerEndpoint");
var authenticatorBaseAddress = configuration.GetConnectionString("AuthenticatorEndpoint");
var clientPlatformBaseAddress = configuration.GetConnectionString("ClientPlatformEndpoint");

if (string.IsNullOrEmpty(loggerBaseAddress) || string.IsNullOrEmpty(authenticatorBaseAddress) || string.IsNullOrEmpty(clientPlatformBaseAddress))
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

HttpStatusCode[] retryCodes = [HttpStatusCode.ServiceUnavailable, HttpStatusCode.GatewayTimeout, HttpStatusCode.BadGateway,
                               HttpStatusCode.NotFound, HttpStatusCode.RequestTimeout, HttpStatusCode.TooManyRequests];
AsyncRetryPolicy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(r => Array.Exists(retryCodes, statusCode => statusCode == r.StatusCode))
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

builder.Services.AddHttpClient<StorageService>()
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);
builder.Services.AddHttpClient<ILoggerService, LoggerService>(httpClient =>
    httpClient.BaseAddress = new Uri(loggerBaseAddress))
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);
builder.Services.AddHttpClient<IAuthenticatorService, AuthenticatorService>(httpClient =>
    httpClient.BaseAddress = new Uri(authenticatorBaseAddress))
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);
builder.Services.AddHttpClient<IClientPlatformService, ClientPlatformService>(httpClient =>
    httpClient.BaseAddress = new Uri(clientPlatformBaseAddress))
    .AddPolicyHandler(retryPolicy)
    .AddPolicyHandler(circuitBreakerPolicy);

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
