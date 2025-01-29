using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RVAegis;
using RVAegis.Contexts;
using RVAegis.Helpers;
using RVAegis.Services.Classes;
using RVAegis.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add JWT authentication.
builder.Services.AddAuthorization();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var issuer = config.GetRequiredSection("Jwt")["Issuer"];
        var audience = config.GetRequiredSection("Jwt")["Audience"];
        var key = config.GetRequiredSection("Jwt")["Key"];

        if (string.IsNullOrEmpty(key)) return;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateActor = true,
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuerSigningKey = true,
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["AccessToken"];
                return Task.CompletedTask;
            }
        };
    }
);

// Add services to the container.
builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<MigrationManager>();

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Добавляем gRPC клиент для взаимодействия с Python сервисом
builder.Services.AddGrpcClient<FaceRecognition.FaceRecognitionClient>(options =>
{
    options.Address = new Uri("http://localhost:50052");
});

// Добавляем фоновую задачу для запроса фото с Python сервиса
builder.Services.AddHostedService<ImageBroadcastService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var migrationManager = services.GetRequiredService<MigrationManager>();
    await migrationManager.MigrateDatabaseAsync(app);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Добавляем WebSocket middleware
app.UseWebSockets();
app.UseMiddleware<WebSocketMiddleware>();

app.MapControllers();

app.Run();
