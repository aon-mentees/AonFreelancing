using System.Xml.Serialization;
using AonFreelancing.Configs;
using AonFreelancing.Models;
using AonFreelancing.Utilities;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.Mapping;
using Microsoft.Extensions.Options;

namespace AonFreelancing.Services;

public class ElasticService<T> where T : class
{
    private readonly ElasticsearchClient _client;
    private readonly string _indexName;

    public ElasticService(IOptions<ElasticSettings> elasticOptions)
    {
        var settings = elasticOptions.Value;
        _indexName = typeof(T).Name.ToLower() + 's';
        var clientSettings = new ElasticsearchClientSettings(new Uri(settings.Url))
            .DefaultIndex(_indexName);

        _client = new ElasticsearchClient(clientSettings);
    }

    public async Task CreateIndexIfNotExistsAsync()
    {
        if (!(await _client.Indices.ExistsAsync(_indexName)).Exists)
            await _client.Indices.CreateAsync(_indexName);
    }

    public async Task CreateIndexIfNotExistsAsync(Action<CreateIndexRequestDescriptor> descriptor)
    {
        if (!(await _client.Indices.ExistsAsync(_indexName)).Exists)
            await _client.Indices.CreateAsync(_indexName, descriptor);
    }

    public async Task<bool> AddOrUpdate(T document, string id)
    {
        var response = await _client.IndexAsync(document, i => i
            .Index(_indexName)
            .Id(id)
        );

        return response.IsValidResponse;
    }

    public async Task<T?> GetAsync(string id)
    {
        var result = await _client.GetAsync<T>(id, i => i.Index(_indexName));
        return result.Source;
    }

    public async Task<List<T>> SearchAsync(string key,int pageNumber, int pageSize, params string[] fields)
    {
        var response = await _client.SearchAsync<T>(s => s
            .Index(_indexName)
            .From(pageNumber*pageSize)
            .Size(pageSize)
            .Query(q => q
                .MultiMatch(mm => mm
                    .Fields(fields)
                    .Query(key)
                    .Fuzziness(new Fuzziness("AUTO"))
                )
            )
        );

        return response.Documents.ToList();
    }


    public async Task<List<T>> AutocompleteAsync(string prefix, string autocompleteField)
    {
        var response = await _client.SearchAsync<T>(s => s
            .Index(_indexName)
            .Size(Constants.AUTOCOMPLETE_DEFAULT_PAGE_SIZE)
            // Uses MatchBoolPrefix: treats all terms as required and allows the last term to match as a prefix.
            // This is ideal for autocomplete scenarios where users might type partial terms.
            // It has more overhead than MultiMatch but provides smarter matching for incomplete inputs.
            .Query(q => q.MatchBoolPrefix(bp => bp.Query(prefix).Field(autocompleteField)))
        );
        if (!response.IsValidResponse)
            return new List<T>();
        return response.Documents.ToList();
    }
}