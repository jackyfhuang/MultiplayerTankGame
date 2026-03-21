var builder = WebApplication.CreateBuilder(args);

// Register SignalR
builder.Services.AddSignalR();

// Allow Unity client to connect without CORS issues
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors();

// Map the hub to the /tankgame endpoint
// Unity will connect to: ex. localhost:5000/tankgame
app.MapHub<TankHub>("/tankgame");

// Listen on all network interfaces (0.0.0.0) to allow external connections
// Use localhost for local-only, or 0.0.0.0 for network access
app.Urls.Add("http://0.0.0.0:5190");

app.Run();