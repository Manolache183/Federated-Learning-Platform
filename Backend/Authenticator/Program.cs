using Authenticator.Auth;
using Authenticator.Firebase;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<FirestoreDatabaseService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
