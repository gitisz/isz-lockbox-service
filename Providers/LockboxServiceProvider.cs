using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.Extensions.Options;
using Keycloak.AuthServices.Sdk.AuthZ;


using isz.lockbox.service.Models;

namespace isz.lockbox.service.Providers;

public class LockboxServiceProvider
{
  private readonly ILogger<LockboxServiceProvider>? _logger;

  private IOptions<AwsConfiguration>? _awsConfiguration { get; }

  private AmazonDynamoDBConfig _clientConfig { get; }

  public LockboxServiceProvider(ILogger<LockboxServiceProvider> logger,
    IOptions<AwsConfiguration> awsConfiguration
    )
  {
    _logger = logger;
    _awsConfiguration = awsConfiguration;

    AWSConfigs.ProxyConfig.Host = _awsConfiguration.Value.ProxyHost;
    AWSConfigs.ProxyConfig.Port = _awsConfiguration.Value.ProxyPort;
    AWSConfigs.ProxyConfig.BypassList = _awsConfiguration.Value.BypassList;
    AWSConfigs.EndpointDefinition = _awsConfiguration.Value.EndpointDefinition;

    _clientConfig = new AmazonDynamoDBConfig();
    _clientConfig.RegionEndpoint = RegionEndpoint.USEast1;
    _clientConfig.ServiceURL = _awsConfiguration.Value.EndpointDefinition;

  }


  public async Task UpsertAsync(Lockbox lockbox)
  {
    var client = new AmazonDynamoDBClient(_clientConfig);

    DynamoDBContext context = new DynamoDBContext(client);

    await context.SaveAsync(lockbox);

  }

  public async Task<IEnumerable<Lockbox>> GetLocboxesAsync(string paginationToken = "")
  {
    IEnumerable<Lockbox> lockboxes = new List<Lockbox> { };

    var client = new AmazonDynamoDBClient(_clientConfig);

    DynamoDBContext context = new DynamoDBContext(client);

    var table = context.GetTargetTable<Lockbox>();

    var scanOps = new ScanOperationConfig();

    if (!string.IsNullOrEmpty(paginationToken))
    {
      scanOps.PaginationToken = paginationToken;
    }

    var results = table.Scan(scanOps);

    List<Document> data = await results.GetNextSetAsync();

    lockboxes = context.FromDocuments<Lockbox>(data);

    return lockboxes;
  }

  public async Task<Lockbox> GetLocboxAsync(string lockboxId)
  {
    Lockbox ? lockbox = null;

    var client = new AmazonDynamoDBClient(_clientConfig);

    DynamoDBContext context = new DynamoDBContext(client);

    lockbox = await context.LoadAsync<Lockbox>(lockboxId);

    return lockbox;
  }
}