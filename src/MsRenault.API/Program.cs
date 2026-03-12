using MsRenault.API.Extensions;
using MsRenault.API.Workers;
using MsRenault.API.Consumers;
using MsRenault.Dominio.Interfaces;
using MsRenault.Infra.Dados.Services;
using MsRenault.Infra.Dados.Repositories;
using MsRenault.Infra.Mensageria.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // .NET 9 standard

// Observability
builder.Services.AddMsRenaultObservability(builder.Configuration);

// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp => 
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDB") ?? "mongodb://localhost:27017";
    return new MongoClient(connectionString);
});

// Services
builder.Services.AddSingleton<IRenaultAuthService, RenaultAuthService>();
builder.Services.AddHttpClient<IRenaultApiService, RenaultApiService>();
builder.Services.AddHttpClient<ISalesforceApiService, SalesforceApiService>();

builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<IRabbitMqService, RabbitMqService>();

// Workers
builder.Services.AddHostedService<RenaultIngestionWorker>();
builder.Services.AddHostedService<RabbitMqLeadConsumer>();
builder.Services.AddHostedService<RabbitMqFeedbackConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // .NET 9 standard
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
