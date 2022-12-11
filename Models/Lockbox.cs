using Amazon.DynamoDBv2.DataModel;

namespace isz.lockbox.service.Models;

public class OwnerContact
{
  [DynamoDBProperty]
  public string? Eid { get; set; }

  [DynamoDBProperty]
  public string? EmailAddress { get; set; }
}

[DynamoDBTable("Lockbox")]
public class Lockbox
{

  [DynamoDBHashKey("lockboxId")]
  public string? LockboxId { get; set; }

  [DynamoDBProperty]
  public string? Application { get; set; }

  [DynamoDBProperty]
  public string? Name { get; set; }

  [DynamoDBProperty]
  public string? Environment { get; set; }

  [DynamoDBProperty]
  public OwnerContact? OwnerContact { get; set; }

  [DynamoDBProperty]
  public string[]? Components { get; set; }

}
