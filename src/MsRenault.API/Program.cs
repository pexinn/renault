using MsRenault.API.Extensions;
using MsRenault.Dominio.Interfaces;
using MsRenault.Aplicacao.Servicos;
using MsRenault.Infra.Dados.Services;
using MsRenault.Infra.Dados.Repositories;
using MsRenault.Infra.Mensageria.Services;
using MongoDB.Driver;
using Hangfire;
using Hangfire.Console;
using Hangfire.InMemory;
using MsRenault.API.Configuracoes;
using Microsoft.Extensions.Options;

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

// Configurações
builder.Services.Configure<MsRenault.Aplicacao.Configuracao.FilasConfiguracao>(builder.Configuration.GetSection("Filas"));

// Services
builder.Services.AddSingleton<IRenaultAuthService, RenaultAuthService>();
builder.Services.AddHttpClient<IRenaultApiService, RenaultApiService>();
builder.Services.AddHttpClient<ISalesforceApiService, SalesforceApiService>();

builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddSingleton<IRabbitMqService, RabbitMqService>();
builder.Services.AddSingleton<IRenaultServico, RenaultServico>();
builder.Services.AddSingleton<IMessageProcessingService, MessageProcessingService>();

// Hangfire
builder.Services.AddHangfire(config => config
    .UseConsole()
    .UseInMemoryStorage()
);

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1;
});

// Workers
builder.Services.AddHostedService<MsRenault.Aplicacao.Hospedagem.MensageriaHosted>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // .NET 9 standard
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions 
{ 
    Authorization = new[] { new AutorizacaoHangfire() },
    DashboardTitle = "MsRenault"
});

// Recurring Jobs
RecurringJob.AddOrUpdate<IRenaultServico>(
    "Obter leads renault",
    x => x.ObterLeads(),
    builder.Configuration["Hangfire:Cron"] ?? "*/10 * * * *"
);

app.MapControllers();

app.Run();
