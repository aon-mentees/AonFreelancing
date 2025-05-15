using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.Documents;
using AonFreelancing.Services;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Jobs;

public class ElsSetupJob : IHostedService
{
    private readonly ILogger<ElsSetupJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ElsSetupJob(ILogger<ElsSetupJob> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        await CreateIndexesAsync(scope);
        long migrationStartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await MigrateUsersToEsAsync(scope);
        long migrationEndTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _logger.LogInformation($"Migration took: {migrationEndTime - migrationStartTime} milliseconds");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    

    private async Task MigrateUsersToEsAsync(IServiceScope scope)
    {
        MainAppContext context = scope.ServiceProvider.GetRequiredService<MainAppContext>();
        ElasticService<UserDocument> elsService = scope.ServiceProvider.GetRequiredService<ElasticService<UserDocument>>();

        long usersCount = await context.Users.CountAsync();
        int batchSize = 100;
        int pagesCount = (int)Math.Ceiling(usersCount / (double)batchSize);
        for (int i = 0; i < pagesCount; i++)
        {
            List<UserDocument> users = await context.Users.AsNoTracking()
                                                          .Skip(i * batchSize)
                                                          .Take(batchSize)
                                                          .Select(u=>new UserDocument(u))
                                                          .ToListAsync();
            var response = await elsService.AddOrUpdateBulkAsync(users);
            if (!response.IsValidResponse)
                _logger.LogError(
                    $"Failed to add or update users for page number {i} users. {response.ItemsWithErrors}");
            else
            {
                _logger.LogInformation($"Updated users in elastic search.");
            }
        }
    }

    private async Task CreateIndexesAsync(IServiceScope scope)
    {
        _logger.LogInformation("Running Elastic index creation job...");
        long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        await CreateUserIndexAsync(scope);
        await CreateProjectIndexAsync(scope);

        long endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long totalTime = endTime - startTime;

        _logger.LogInformation("Elastic indexes ensured.");
        _logger.LogInformation($"Total time to create indexes (in Millis): {totalTime}");
    }

    private async Task CreateUserIndexAsync(IServiceScope scope)
    {
        var elsUserService = scope.ServiceProvider.GetRequiredService<ElasticService<UserDocument>>();
        Action<CreateIndexRequestDescriptor> descriptor = c => c
            .Mappings(m => m
                .Properties<UserDocument>(ps =>
                    ps.SearchAsYouType("name")
                )
            );
        await elsUserService.CreateIndexIfNotExistsAsync(descriptor);
    }
    private async Task CreateProjectIndexAsync(IServiceScope scope)
    {
        var elsProjectService = scope.ServiceProvider.GetRequiredService<ElasticService<Project>>();
        await elsProjectService.CreateIndexIfNotExistsAsync();
    }
}