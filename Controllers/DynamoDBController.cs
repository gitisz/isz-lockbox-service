using System.Net;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using isz.lockbox.service.Providers;

namespace piral.dynamodb.service.Controllers;

[ApiController]
[Route("[controller]")]
public class DynamoDBController : ControllerBase
{
  private readonly ILogger<DynamoDBController> _logger;

  private DynamoDBServiceProvider _dynamodbServiceProvider { get; }

  public DynamoDBController(ILogger<DynamoDBController> logger,
    DynamoDBServiceProvider dynamodbServiceProvider
    )
  {
    _logger = logger;
    _dynamodbServiceProvider = dynamodbServiceProvider;
  }

  [HttpGet(Name = "GetTables")]
  [Route("tables")]
  public async Task<ActionResult> GetTables()
  {
    List<string> ? tables = null;

    try
    {
      tables = await _dynamodbServiceProvider.GetTablesAsync();
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to get tables.");
    }

    return Ok(tables);
  }

  [HttpPut(Name = "CreateDynamoDBTable")]
  [Route("createtable/{tableName}/{primaryKey}")]
  public async Task<ActionResult> CreateDynamoDBTable(string tableName, string primaryKey)
  {
    try
    {
      await _dynamodbServiceProvider.CreateTableAsync(tableName, primaryKey);
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to create dynamodb table.");
    }

    return Ok();
  }

  [HttpPut(Name = "CreateDynamoDBTableWithSort")]
  [Route("createtable/{tableName}/{primaryKey}/{sortKey}")]
  public async Task<ActionResult> CreateDynamoDBTableWithSort(string tableName, string primaryKey, string sortKey)
  {
    try
    {
      await _dynamodbServiceProvider.CreateTableWithSortAsync(tableName, primaryKey, sortKey);
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to create dynamodb table.");
    }

    return Ok();
  }

  [HttpGet(Name = "GetTableDescription")]
  [Route("tabledescription/{tableName}")]
  public async Task<ActionResult> GetTableDescription(string tableName)
  {
    TableDescription ? tableDesc = null;

    try
    {
      tableDesc = await _dynamodbServiceProvider.DescribeTableAsync(tableName);
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to get table description.");
    }

    return Ok(tableDesc);
  }

  [HttpDelete(Name = "DeleteTable")]
  [Route("deletetable/{tableName}")]
  public async Task<ActionResult> DeleteTable(string tableName)
  {
    try
    {
      await _dynamodbServiceProvider.DeleteTableAsync(tableName);
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to get table description.");
    }

    return Ok();
  }
}
