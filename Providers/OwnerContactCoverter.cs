using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
using isz.lockbox.service.Models;

namespace isz.lockbox.service.Providers;

public class DimensionTypeConverter : IPropertyConverter
{
  public DynamoDBEntry ToEntry(object value)
  {
    OwnerContact? ownerContact = value as OwnerContact;

    if (ownerContact == null) throw new ArgumentOutOfRangeException();

    string data = string.Format("{1}{0}{2}", " x ",
                    ownerContact.Eid, ownerContact.EmailAddress);

    DynamoDBEntry entry = new Primitive
    {
      Value = data
    };

    return entry;
  }

  public object FromEntry(DynamoDBEntry entry)
  {
    Primitive? primitive = entry as Primitive;
    if (primitive == null || !(primitive.Value is String) || string.IsNullOrEmpty((string)primitive.Value))
      throw new ArgumentOutOfRangeException();

    string[] data = ((string)(primitive.Value)).Split(new string[] { " x " }, StringSplitOptions.None);
    if (data.Length != 3) throw new ArgumentOutOfRangeException();

    OwnerContact complexData = new OwnerContact
    {
      Eid = Convert.ToString(data[0]),
      EmailAddress = Convert.ToString(data[1]),
    };
    return complexData;
  }
}