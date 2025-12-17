using MovieApp.StatsService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add gRPC
builder.Services.AddGrpc();

var app = builder.Build();

// Map gRPC service
app.MapGrpcService<CalculatorGrpcService>();

app.MapGet("/", () => "MovieApp Calculator Service - gRPC running on port 5010");

app.Run();