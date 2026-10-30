using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
using VaultPass.Application;
using VaultPass.Infrastructure;
using VaultPass.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
if (OperatingSystem.IsWindows())
    builder.Host.UseWindowsService();

//Serilog
var loggerConfig = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console();

var seqUrl = builder.Configuration["Serilog:SeqUrl"];
if (!string.IsNullOrWhiteSpace(seqUrl))
    loggerConfig = loggerConfig.WriteTo.Seq(seqUrl);

Log.Logger = loggerConfig.CreateLogger();

builder.Host.UseSerilog();

//Services
builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi(); //for scalar ui


// Clean Architecture layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

//JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

//CORS
var corsOrigins = (builder.Configuration["CorsOrigins"] ?? "http://localhost:5243,https://localhost:7120")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());

    options.AddPolicy("ChromeExtension", policy =>
        policy.SetIsOriginAllowed(origin => origin.StartsWith("chrome-extension://"))
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddHealthChecks();

var app = builder.Build();

//Middleware Pipeline\
app.UseSerilogRequestLogging();
if (!app.Environment.IsProduction())
    app.UseHttpsRedirection();
app.UseCors("BlazorClient");
app.UseCors("ChromeExtension");
app.UseAuthentication();
app.UseAuthorization();


app.MapOpenApi();
app.MapScalarApiReference();



app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    if (!db.ItemTemplates.Any())
    {
        var templates = VaultPass.Infrastructure.Seed.BuiltInTemplates.GetAll();
        db.ItemTemplates.AddRange(templates);
        await db.SaveChangesAsync();
    }
}

app.Run();