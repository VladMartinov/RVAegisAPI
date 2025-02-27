using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddLogging(configure => configure.AddConsole());
builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddTransient<MigrationManager>();

builder.Services.AddControllers();
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ƒобавл€ем gRPC клиент дл€ взаимодействи€ с Python сервисом
builder.Services.AddGrpcClient<FaceRecognition.FaceRecognitionClient>(options =>
{
    options.Address = new Uri(builder.Configuration.GetSection("gRPC:ServiceAddress").Value);
});

// ƒобавл€ем фоновую задачу дл€ загрузки изображений
builder.Services.AddHostedService<ImageLoaderService>();

// ƒобавл€ем фоновую задачу дл€ запроса фото с Python сервиса
builder.Services.AddHostedService<ImageBroadcastService>();

// ƒобавл€ем CORS политику дл€ нашего API
builder.Services.AddCors(options => {
            options.AddPolicy("WebClient", policyBuilder =>
            {
                policyBuilder.WithOrigins(builder.Configuration.GetSection("WebClientAddress").Value);
                policyBuilder.AllowAnyHeader();
                policyBuilder.AllowAnyMethod();
                policyBuilder.AllowCredentials();
            });
});

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
app.UseCors("WebClient");

app.UseAuthentication();
app.UseAuthorization();

// ƒобавл€ем WebSocket middleware
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

webSocketOptions.AllowedOrigins.Add(builder.Configuration.GetSection("WebClientAddress").Value);

app.UseWebSockets(webSocketOptions);
app.UseMiddleware<WebSocketMiddleware>();

app.MapControllers();

app.Run();
