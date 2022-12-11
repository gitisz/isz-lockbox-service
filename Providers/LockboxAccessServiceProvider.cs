using System.Text.Json;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Options;
using isz.lockbox.service.Models;

namespace isz.lockbox.service.Providers;

public class LockboxAccessServiceProvider
{
  private readonly ILogger<LockboxAccessServiceProvider> _logger;

  private IOptions<AwsConfiguration>? _awsConfiguration { get; }

  private AmazonDynamoDBConfig _dynamoClientConfig { get; }

  public LockboxAccessServiceProvider(ILogger<LockboxAccessServiceProvider> logger,
    IOptions<AwsConfiguration> awsConfiguration
    )
  {
    _logger = logger;
    _awsConfiguration = awsConfiguration;

    AWSConfigs.ProxyConfig.Host = _awsConfiguration.Value.ProxyHost;
    AWSConfigs.ProxyConfig.Port = _awsConfiguration.Value.ProxyPort;
    AWSConfigs.ProxyConfig.BypassList = _awsConfiguration.Value.BypassList;
    AWSConfigs.EndpointDefinition = _awsConfiguration.Value.EndpointDefinition;

    _dynamoClientConfig = new AmazonDynamoDBConfig();
    _dynamoClientConfig.RegionEndpoint = RegionEndpoint.USEast1;
    _dynamoClientConfig.ServiceURL = _awsConfiguration.Value.EndpointDefinition;

  }


  public async Task UpsertAsync(LockboxAccess lockboxaccess)
  {
    var client = new AmazonDynamoDBClient(_dynamoClientConfig);

    DynamoDBContext context = new DynamoDBContext(client);

    await context.SaveAsync(lockboxaccess);

  }

  public async Task<IEnumerable<LockboxAccess>> GetLocboxAccessesAsync(string paginationToken = "")
  {
    IEnumerable<LockboxAccess> lockboxAccesses = new List<LockboxAccess> { };

    var client = new AmazonDynamoDBClient(_dynamoClientConfig);

    DynamoDBContext context = new DynamoDBContext(client);

    var table = context.GetTargetTable<LockboxAccess>();

    var scanOps = new ScanOperationConfig();

    if (!string.IsNullOrEmpty(paginationToken))
    {
      scanOps.PaginationToken = paginationToken;
    }

    var results = table.Scan(scanOps);

    List<Document> data = await results.GetNextSetAsync();

    lockboxAccesses = context.FromDocuments<LockboxAccess>(data);

    return lockboxAccesses;
  }

  public async Task<IEnumerable<LockboxAccess>> GetLocboxAccessesByLockboxIdAsync(string lockboxId)
  {
    IEnumerable<LockboxAccess> lockboxAccesses = new List<LockboxAccess> { };

    lockboxAccesses = await GetLocboxAccessesAsync();

    var filteredLockboxAccesses = lockboxAccesses
      .Where(l =>
      {
        return String.Equals(l.LockboxId, lockboxId, StringComparison.CurrentCultureIgnoreCase);
      })
      .Select(l => l)
      .ToList();

    return filteredLockboxAccesses;
  }


  public async Task<LockboxAccess> GetLocboxAccessAsync(string accessId)
  {
    LockboxAccess? lockboxaccess = null;

    var client = new AmazonDynamoDBClient(_dynamoClientConfig);

    DynamoDBContext context = new DynamoDBContext(client);

    lockboxaccess = await context.LoadAsync<LockboxAccess>(accessId);

    return lockboxaccess;
  }

}