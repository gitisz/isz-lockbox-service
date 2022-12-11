using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Options;
using isz.lockbox.service.Models;

namespace isz.lockbox.service.Providers;

public class LockboxSecretServiceProvider
{
  private readonly ILogger<LockboxSecretServiceProvider>? _logger;

  private IOptions<AwsConfiguration>? _awsConfiguration { get; }

  private AmazonDynamoDBConfig _clientConfig { get; }

  public LockboxSecretServiceProvider(ILogger<LockboxSecretServiceProvider> logger,
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

  public async Task UpsertAsync(LockboxSecret lockboxSecret)
  {
    var client = new AmazonDynamoDBClient(_clientConfig);

    DynamoDBContext context = new DynamoDBContext(client);

    await context.SaveAsync(lockboxSecret);
  }

  public async Task<LockboxSecret> GetLocboxSecretAsync(string pathId)
  {
    LockboxSecret? lockboxSecret = null;

    var client = new AmazonDynamoDBClient(_clientConfig);

    DynamoDBContext context = new DynamoDBContext(client);

    lockboxSecret = await context.LoadAsync<LockboxSecret>(pathId);

    return lockboxSecret;
  }
}