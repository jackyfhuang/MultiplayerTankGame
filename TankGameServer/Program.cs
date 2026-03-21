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

// launchSettings.json sets ASPNETCORE_URLS to http://localhost:5190 by default, which
// only accepts connections from this machine. Clear and bind 0.0.0.0 so LAN clients work.
app.Urls.Clear();
app.Urls.Add("http://0.0.0.0:5190");

app.Run();