using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using XownerWebOne.Data;
using XownerWebOne.Hubs;
using XownerWebOne.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();

// ================= CORS =================
var AllowFrontend = "AllowFrontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowFrontend,
        policy =>
        {
            policy.WithOrigins(
                "http://localhost:5173",
                "http://localhost:5174",
                "https://xowner.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .AllowAnyOrigin();
        });
});

// ================= DATABASE =================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new Exception("❌ DefaultConnection missing in appsettings.json");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// ================= JWT CONFIG =================
var jwtSection = builder.Configuration.GetSection("Jwt");

var jwtKeyString = jwtSection["Key"]
    ?? throw new Exception("❌ Jwt:Key missing in appsettings.json");

var jwtIssuer = jwtSection["Issuer"]
    ?? throw new Exception("❌ Jwt:Issuer missing in appsettings.json");

var jwtAudience = jwtSection["Audience"]
    ?? throw new Exception("❌ Jwt:Audience missing in appsettings.json");

var jwtKey = Encoding.UTF8.GetBytes(jwtKeyString);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
    };

    // 🔹 SignalR JWT support
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken =
                context.Request.Query["access_token"].FirstOrDefault()
                ?? context.Request.Query["token"].FirstOrDefault();

            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken)
                && path.StartsWithSegments("/chat"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

// ================= SERVICES =================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSignalR();

// ================= SWAGGER =================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Xowner API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
//
// ================= PORT CONFIG =================
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

var app = builder.Build();

// ================= AUTO MIGRATION =================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ================= STATIC FILES =================
var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

if (!Directory.Exists(uploadPath))
{ 
    Directory.CreateDirectory(uploadPath);
}

// wwwroot files
app.UseStaticFiles();

// uploads folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath),
    RequestPath = "/uploads"
});

// ================= MIDDLEWARE =================
app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseCors(AllowFrontend);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ================= SIGNALR =================
app.MapHub<ChatHub>("/chat");

app.Run();