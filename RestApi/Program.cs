using Microsoft.Extensions.Options;
using NLog.Web;
using SmtpForwarder.Application;
using SmtpForwarder.Application.Utils;
using SmtpForwarder.DataLayer.Extensions;
using SmtpForwarder.Domain.Settings;
using SmtpForwarder.RestApi.Utils;
using SmtpForwarder.SmtpReceiverServer;

var builder = WebApplication.CreateBuilder(args);

// Add custom settings
builder.Host.ConfigureAppSettings();


// NLog: Setup NLog for Dependency injection
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/hOpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database

var settings = builder.Configuration.GetRequiredSection("App").Get<Settings>();

builder.Services.Configure<Settings>(
    builder.Configuration.GetRequiredSection("App"));

builder.Services.AddAppContext(settings.ConnectionString);
builder.Services.AddRepositories();

// Add application event system
builder.Services.AddEvents(typeof(ServiceInjector).Assembly);

// Services
builder.Services.AddAuthorizationHandlers();
builder.Services.AddSmtpService();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Services.WarmUp();
app.Run();