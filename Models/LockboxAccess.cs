using Amazon.DynamoDBv2.DataModel;

namespace isz.lockbox.service.Models;


[DynamoDBTable("LockboxAccess")]
public class LockboxAccess
{

  [DynamoDBHashKey("accessId")]
  public string? AccessId { get; set; }

  [DynamoDBProperty]
  public string? LockboxId { get; set; }

  [DynamoDBProperty]
  public string? AccessType { get; set; }

  [DynamoDBProperty]
  public string? VaultRole { get; set; }

  [DynamoDBProperty]
  public string? ResourceName { get; set; }

  [DynamoDBProperty]
  public string[]? Permissions { get; set; }

}
