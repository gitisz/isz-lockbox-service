namespace isz.lockbox.service.Models;

public class VaultConfiguration
{
  public string? Address { get; set; }
  public string? Role { get; set; }
  public string? Secret { get; set; }
  public string? MountPath { get; set; }
  public string? SecretType { get; set; }
}