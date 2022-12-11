namespace isz.lockbox.service.Models;

public class KeycloakConfiguration
{
  public string ? ServerRealm { get; set; }
  public string ? Metadata { get; set; }
  public string ? ClientId { get; set; }
  public string ? ClientSecret { get; set; }
  public string ? TokenExchange { get; set; }
  public string ? Audience { get; set; }
}