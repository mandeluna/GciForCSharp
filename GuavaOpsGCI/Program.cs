using GuavaOpsGCI;
using Microsoft.Extensions.Options;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Microsoft.AspNetCore.Mvc;
using Util;

var builder = WebApplication.CreateBuilder(args);

CCKLogger.Initialize();

// 1. Load config.yml
var yamlContent = File.ReadAllText("config.yml");
var deserializer = new DeserializerBuilder()
    .WithNamingConvention(CamelCaseNamingConvention.Instance)
    .Build();
var gciConfig = deserializer.Deserialize<GciConfig>(yamlContent);

// 2. DI Registration
builder.Services.AddSingleton(Options.Create(gciConfig));
builder.Services.AddSingleton<GemStoneService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Pipeline Configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Access at /swagger
}

// Redirect root (/) to Swagger or a default status page to avoid the middleware error
app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();

var port = 5000;
var url = String.Format($"http://0.0.0.0:{port}", port);
Console.WriteLine($"[Server] is listening on port {port}.");

app.Run(url);

// --- Main GemStone API Controller ---

[ApiController]
[Route("api/[controller]")]
public class GemStoneController : ControllerBase
{
    private readonly GemStoneService _gs;

    public GemStoneController(GemStoneService gs)
    {
        _gs = gs;
    }

    /// <summary>
    /// HTTP GET: Maps to a Read-Only transaction (Begin -> Execute -> Abort)
    /// </summary>
    [HttpGet("execute")]
    public async Task<IActionResult> ExecuteRead([FromQuery] string code)
    {
        CCKLogger.LogInformation(("GET execute/{code}"));
        if (string.IsNullOrEmpty(code)) return BadRequest("Smalltalk code is required.");

        try
        {
            // We use the read-only variant to avoid commit overhead
            var jsonResult = await _gs.CallReadOnlyAsync(code);
            return Content(jsonResult, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// HTTP POST: Maps to a Read-Write transaction (Begin -> Execute -> Commit)
    /// </summary>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteWrite([FromBody] string code)
    {
        CCKLogger.LogInformation(("POST execute/{code}"));
        if (string.IsNullOrEmpty(code)) return BadRequest("Smalltalk code is required.");

        try
        {
            // We use the read-write variant to ensure persistence
            var jsonResult = await _gs.CallReadWriteAsync(code);
            return Content(jsonResult, "application/json");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Status check to verify connectivity
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        try 
        {
            var time = await _gs.CallReadOnlyAsync("DateTime now printString");
            return Ok(new { status = "Connected", gemStoneTime = time });
        }
        catch (Exception ex)
        {
            return StatusCode(503, new { status = "Disconnected", error = ex.Message });
        }
    }
}