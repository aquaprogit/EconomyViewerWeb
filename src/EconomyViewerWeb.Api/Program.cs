using EconomyViewerWeb.Application;
using EconomyViewerWeb.Infrastructure;
using EconomyViewerWeb.Infrastructure.ForumSync;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var forumSyncService = scope.ServiceProvider.GetRequiredService<IForumSyncService>();
    await forumSyncService.SeedIfEmptyAsync();
}

app.Run();
