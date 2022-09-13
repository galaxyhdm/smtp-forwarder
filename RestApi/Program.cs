using Application;
using Application.Utils;
using DataLayer.Extensions;
using SmtpReceiverServer;

var builder = WebApplication.CreateBuilder(args);

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