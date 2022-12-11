using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Options;
using isz.lockbox.service.Models;

namespace isz.lockbox.service.Providers;

public class LockboxSecretPathServiceProvider
{
  private readonly ILogger<LockboxSecretPathServiceProvider>? _logger;

  private IOptions<AwsConfiguration>? _awsConfiguration { get; }

  private AmazonDynamoDBConfig _clientConfig { get; }

  public LockboxSecretPathServiceProvider(ILogger<LockboxSecretPathServiceProvider> logger,
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

  public async Task UpsertAsync(LockboxSecretPath lockboxSecretPath)
  {
    var client = new AmazonDynamoDBClient(_clientConfig);

    DynamoDBContext context = new DynamoDBContext(client);

    await context.SaveAsync(lockboxSecretPath);

  }

  public async Task<IEnumerable<LockboxSecretPath>> GetLockboxSecretPathsByLockboxIdAsync(string lockboxId)
  {
    IEnumerable<LockboxSecretPath> lockboxSecretPaths = new List<LockboxSecretPath> { };

    var client = new AmazonDynamoDBClient(_clientConfig);

    DynamoDBContext context = new DynamoDBContext(client);

    lockboxSecretPaths = await context.QueryAsync<LockboxSecretPath>(lockboxId)
      .GetRemainingAsync();

    return lockboxSecretPaths;
  }


  public async Task<LockboxSecretPath> GetLocboxSecretPathAsync(string pathId)
  {
    LockboxSecretPath? lockboxSecretPath = null;

    var client = new AmazonDynamoDBClient(_clientConfig);

    DynamoDBContext context = new DynamoDBContext(client);

    lockboxSecretPath = await context.LoadAsync<LockboxSecretPath>(pathId);

    return lockboxSecretPath;
  }
}