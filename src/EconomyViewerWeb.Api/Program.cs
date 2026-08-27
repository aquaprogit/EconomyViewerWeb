using EconomyViewerWeb.Api.Filters;
using EconomyViewerWeb.Api.Jobs;
using EconomyViewerWeb.Api.Middleware;
using EconomyViewerWeb.Application;
using EconomyViewerWeb.Infrastructure;
using Hangfire;
using Hangfire.SqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<ForumSyncJob>();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' was not found.");

builder.Services.AddHangfire(configuration =>
{
    configuration
        .SetDataCompatibilityLevel(
            CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(
            connectionString,
            new SqlServerStorageOptions
            {
                PrepareSchemaIfNecessary = true
            });
});

builder.Services.AddHangfireServer();

var app = builder.Build();

var recurringJobManager =
    app.Services.GetRequiredService<IRecurringJobManager>();

recurringJobManager.AddOrUpdate<ForumSyncJob>(
    recurringJobId: "forum-sync-daily",
    methodCall: job => job.ExecuteAsync(
        CancellationToken.None),
    cronExpression: Cron.Daily(),
    options: new RecurringJobOptions
    {
        TimeZone = TimeZoneInfo.Local
    });

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapHealthChecks("/health");



app.Run();
