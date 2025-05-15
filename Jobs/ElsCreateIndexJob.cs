using AonFreelancing.Models;
using AonFreelancing.Services;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AonFreelancing.Jobs;

public class ElsCreateIndexJob : IHostedService
{
    private readonly ILogger<ElsCreateIndexJob> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public ElsCreateIndexJob(
        ILogger<ElsCreateIndexJob> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        long startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        _logger.LogInformation("Running Elastic index creation job...");

        using var scope = _scopeFactory.CreateScope();

        await CreateUserIndexAsync(scope);

        var elsProjectService = scope.ServiceProvider.GetRequiredService<ElasticService<Project>>();
        await elsProjectService.CreateIndexIfNotExistsAsync();

        long endTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long totalTime = endTime - startTime;

        _logger.LogInformation("Elastic indexes ensured.");
        _logger.LogInformation($"Total time to create indexes (in Millis): {totalTime}");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task CreateUserIndexAsync(IServiceScope scope)
    {
        var elsUserService = scope.ServiceProvider.GetRequiredService<ElasticService<User>>();
        Action<CreateIndexRequestDescriptor> descriptor = c => c
            .Mappings(m => m
                .Properties<User>(ps =>
                    ps.SearchAsYouType("name")
                )
            );
        await elsUserService.CreateIndexIfNotExistsAsync(descriptor);
    }
}