using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using isz.lockbox.service.Models;
using System.Text.Json.Serialization;

namespace isz.lockbox.service.Providers;


public class TokenExchangeProvider
{
  static readonly HttpClient client = new HttpClient();

  private KeycloakConfiguration _keyloakConfiguration { get; }

  public TokenExchangeProvider(ILogger<DynamoDBServiceProvider> logger,
    IOptions<KeycloakConfiguration> keyloakConfiguration)
  {
    _keyloakConfiguration = keyloakConfiguration.Value;
  }

  public async Task<string> GetRefreshTokenAsync(string refreshToken)
  {
    try
    {
      var form = new Dictionary<string, string>
          {
              {"grant_type", "refresh_token"},
              {"client_id", _keyloakConfiguration.ClientId ?? ""},
              {"client_secret", _keyloakConfiguration.ClientSecret ?? ""},
              {"refresh_token", refreshToken }
          };

      HttpResponseMessage tokenResponse = await client.PostAsync(_keyloakConfiguration.TokenExchange, new FormUrlEncodedContent(form));
      var jsonContent = await tokenResponse.Content.ReadAsStringAsync();
      Token tok = JsonSerializer.Deserialize<Token>(jsonContent) ?? new Token { };
      return tok.AccessToken ?? "";
    }
    catch (Exception ex)
    {
      return ex.Message;
    }
  }

  public async Task<string> GetTokenExchangeAsync(string accessToken)
  {
    /*
     * Get exchange token
     * ses the settings injected from startup to read the configuration
     */
    try
    {
      var form = new Dictionary<string, string>
                {
                    {"grant_type", "urn:ietf:params:oauth:grant-type:uma-ticket"},
                    {"client_id", _keyloakConfiguration.ClientId ?? "" },
                    {"client_secret", _keyloakConfiguration.ClientSecret ?? "" },
                    {"audience", _keyloakConfiguration.Audience ?? "" },
                    {"subject_token", accessToken }
                };

      HttpResponseMessage tokenResponse = await client.PostAsync(_keyloakConfiguration.TokenExchange, new FormUrlEncodedContent(form));
      var jsonContent = await tokenResponse.Content.ReadAsStringAsync();
      Token tok = JsonSerializer.Deserialize<Token>(jsonContent) ?? new Token { };
      return tok.AccessToken ?? "";
    }
    catch (Exception ex)
    {
      return ex.Message;
    }
  }

  internal class Token
  {
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }
  }
}