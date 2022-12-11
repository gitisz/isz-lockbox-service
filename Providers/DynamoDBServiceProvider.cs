using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Options;
using isz.lockbox.service.Models;

namespace isz.lockbox.service.Providers;

public class DynamoDBServiceProvider
{
  private readonly ILogger<DynamoDBServiceProvider>? _logger;

  private IOptions<AwsConfiguration>? _awsConfiguration { get; }

  private AmazonDynamoDBConfig _clientConfig { get; }

  public DynamoDBServiceProvider(ILogger<DynamoDBServiceProvider> logger,
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

  public async Task<List<string>> GetTablesAsync()
  {
    List<string> tables = new List<string> { };

    var client = new AmazonDynamoDBClient(_clientConfig);

    string? lastTableNameEvaluated = null;

    do
    {
      var request = new ListTablesRequest
      {
        Limit = 2,
        ExclusiveStartTableName = lastTableNameEvaluated
      };

      var response = await client.ListTablesAsync(request);

      foreach (string name in response.TableNames)
        tables.Add(name);

      lastTableNameEvaluated = response.LastEvaluatedTableName;
    } while (lastTableNameEvaluated != null);

    return tables;

  }

  public async Task CreateTableAsync(string tableName, string primaryKey)
  {
    var client = new AmazonDynamoDBClient(_clientConfig);

    var request = new CreateTableRequest
    {
      AttributeDefinitions = new List<AttributeDefinition>()
      {
          new AttributeDefinition
          {
              AttributeName = primaryKey,
              AttributeType = "S"
          }
      },
      KeySchema = new List<KeySchemaElement>
      {
          new KeySchemaElement
          {
              AttributeName = primaryKey,
              KeyType = "HASH" //Partition key
          },
      },
      ProvisionedThroughput = new ProvisionedThroughput
      {
        ReadCapacityUnits = 5,
        WriteCapacityUnits = 6
      },
      TableName = tableName
    };

    var response = await client.CreateTableAsync(request);
  }

  public async Task CreateTableWithSortAsync(string tableName, string primaryKey, string sortKey)
  {
    var client = new AmazonDynamoDBClient(_clientConfig);

    var request = new CreateTableRequest
    {
      AttributeDefinitions = new List<AttributeDefinition>()
      {
          new AttributeDefinition
          {
              AttributeName = primaryKey,
              AttributeType = "S"
          },
          new AttributeDefinition
          {
              AttributeName = sortKey,
              AttributeType = "S"
          }
      },
      KeySchema = new List<KeySchemaElement>
      {
          new KeySchemaElement
          {
              AttributeName = primaryKey,
              KeyType = "HASH" //Partition key
          },
          new KeySchemaElement
          {
            AttributeName = sortKey,
            KeyType = "RANGE" //Sort key
          }
      },
      ProvisionedThroughput = new ProvisionedThroughput
      {
        ReadCapacityUnits = 5,
        WriteCapacityUnits = 6
      },
      TableName = tableName
    };

    var response = await client.CreateTableAsync(request);
  }

  public async Task<TableDescription> DescribeTableAsync(string tableName)
  {
    var client = new AmazonDynamoDBClient(_clientConfig);

    var request = new DescribeTableRequest
    {
      TableName = tableName
    };

    var response = await client.DescribeTableAsync(request);

    return response.Table;
  }

  public async Task DeleteTableAsync(string tableName)
  {
    var client = new AmazonDynamoDBClient(_clientConfig);

    var request = new DeleteTableRequest
    {
      TableName = tableName
    };

    var response = await client.DeleteTableAsync(request);
  }
}