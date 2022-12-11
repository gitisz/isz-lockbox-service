namespace isz.lockbox.service.Models;

public class AwsConfiguration
{
  public ProxyCredentials? ProxyCredentials { get; set; }

  public string? ProxyHost { get; set; }
  public int? ProxyPort { get; set; }
  public string? Profile { get; set; }
  public string? EndpointDefinition { get; set; }
  public List<string>? BypassList { get; set; }
  public List<string>? Region { get; set; }
  public List<string>? AccountId { get; set; }
}

public class ProxyCredentials
{
  public string? ProxyUser { get; set; }
  public string? ProxyPassword { get; set; }

}