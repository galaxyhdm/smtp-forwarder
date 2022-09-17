using NLog.Web;
using SmtpForwarder.Application;
using SmtpForwarder.Application.Utils;
using SmtpForwarder.DataLayer.Extensions;
using SmtpForwarder.SmtpReceiverServer;

var builder = WebApplication.CreateBuilder(args);

// NLog: Setup NLog for Dependency injection
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddAppContext(Env.GetStringRequired("SQL_CONNECTION"));
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

app.Run();