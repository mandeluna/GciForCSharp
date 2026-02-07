using GCI;

var builder = WebApplication.CreateBuilder(args);

// 1. Bind the GciConfig section from appsettings.json (or other config sources)
// Example appsettings.json structure:
// "GciConfig": {
//   "StoneName": "gs64stone",
//   "GsUserName": "DataCurator",
//   "Pool": { "MaxSessions": 5 }
// }
builder.Services.Configure<GciConfig>(
    builder.Configuration.GetSection("GciConfig"));

// 2. Register the GemStoneService as a Singleton.
// The DI container will automatically inject IOptions<GciConfig> into the constructor.
builder.Services.AddSingleton<GemStoneService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();