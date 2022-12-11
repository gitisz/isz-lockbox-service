using Amazon.DynamoDBv2.DataModel;

namespace isz.lockbox.service.Models;


[DynamoDBTable("LockboxSecretPath")]
public class LockboxSecretPath
{
  [DynamoDBRangeKey("pathId")]
  public string? PathId { get; set; }

  [DynamoDBHashKey("lockboxId")]
  public string? LockboxId { get; set; }

  [DynamoDBProperty]
  public string? Path { get; set; }
}
