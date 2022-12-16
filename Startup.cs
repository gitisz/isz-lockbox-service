
using Amazon.DynamoDBv2;
using isz.lockbox.service.Models;
using isz.lockbox.service.Providers;
using Microsoft.IdentityModel.Logging;
using Keycloak.AuthServices.Authentication;
using Microsoft.AspNetCore.Authorization;
using Keycloak.AuthServices.Authorization;

namespace isz.lockbox.service;

public class StartUp
{
  protected IConfiguration? _configuration { get; }

  protected IConfigurationSection awsConfiguration
  {
    get
    {
      return _configuration?.GetSection("Aws") ?? throw new Exception("Seven Hells!");
    }
  }

  protected IConfigurationSection vaultConfiguration
  {
    get
    {
      return _configuration?.GetSection("Vault") ?? throw new Exception("Seven Hells!");
    }
  }

  protected KeycloakAuthenticationOptions keycloakConfiguration
  {
    get
    {
      return _configuration?.GetSection(KeycloakAuthenticationOptions.Section).Get<KeycloakAuthenticationOptions>() ?? throw new Exception("Seven Hells!");;
    }
  }


  protected KeycloakProtectionClientOptions keycloakProtectionConfiguration
  {
    get
    {
      return _configuration?.GetSection(KeycloakProtectionClientOptions.Section).Get<KeycloakProtectionClientOptions>() ?? throw new Exception("Seven Hells!");;
    }
  }

  public StartUp(IConfiguration configuration, IWebHostEnvironment env)
  {
    _configuration = configuration;

    _configuration.AsEnumerable().ForEach(x =>
    {
      // TODO: log out envs.
    });
  }

  public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
  {
    app.UseCors("CorsPolicy");
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseEndpoints(ep => { ep.MapControllers(); });
  }

  public void ConfigureServices(IServiceCollection services)
  {
    IdentityModelEventSource.ShowPII = true;

    services.AddKeycloakAuthentication(keycloakConfiguration);
    services.AddKeycloakProtectionHttpClient(keycloakProtectionConfiguration);

    services.AddCors(options =>
    {
      options.AddPolicy(name: "CorsPolicy",
            policy =>
            {
              policy.WithOrigins(new string[] { "http://localhost:1234", "*" });
              policy.AllowAnyMethod();
              policy.AllowAnyHeader();
            });
    });
    services.Configure<AwsConfiguration>(awsConfiguration);
    services.Configure<VaultConfiguration>(vaultConfiguration);
    services.AddControllers();
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen();
    services.AddScoped<DynamoDBServiceProvider>();
    services.AddScoped<LockboxServiceProvider>();
    services.AddScoped<LockboxAccessServiceProvider>();
    services.AddScoped<LockboxSecretPathServiceProvider>();
    services.AddScoped<LockboxSecretServiceProvider>();
    services.AddScoped<TokenExchangeProvider>();
    services.AddAWSService<IAmazonDynamoDB>();
    services.AddSingleton<IAuthorizationPolicyProvider, ProtectedResourcePolicyProvider>();
  }
}
