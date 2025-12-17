using YearService.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();

var app = builder.Build();
app.MapGrpcService<YearCalculatorService>();
app.MapGet("/", () => "Year Calculator gRPC Service - Port 5020");

app.Run();