using Amazon.DynamoDBv2.DataModel;

namespace isz.lockbox.service.Models;

public class Secret
{
  public string? Key { get; set; }
  public string? Value { get; set; }

}

[DynamoDBTable("LockboxSecret")]
public class LockboxSecret
{
  [DynamoDBHashKey("pathId")]
  public string? PathId { get; set; }

  [DynamoDBProperty("secrets")]
  public Secret [] ? Secrets { get; set; }
}
