
using System.Security.Claims;
using Amazon.DynamoDBv2;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using isz.lockbox.service.Models;
using isz.lockbox.service.Providers;
using Microsoft.IdentityModel.Logging;

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

  protected IConfigurationSection keycloakConfiguration
  {
    get
    {
      return _configuration?.GetSection("Keycloak") ?? throw new Exception("Seven Hells!");
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
    app.UseAuthorization();
    app.UseEndpoints(ep => { ep.MapControllers(); });
  }

  public void ConfigureServices(IServiceCollection services)
  {
    IdentityModelEventSource.ShowPII = true;

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
    services.AddAuthentication(options =>
            {
              options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
              options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
              options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
             .AddCookie(cookie =>
            {
              //Sets the cookie name and maxage, so the cookie is invalidated.
              cookie.Cookie.Name = "keycloak.cookie";
              cookie.Cookie.MaxAge = TimeSpan.FromMinutes(60);
              cookie.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
              cookie.SlidingExpiration = true;
            })
            .AddOpenIdConnect(options =>
            {
              /*
               * ASP.NET core uses the http://*:5000 and https://*:5001 ports for default communication with the OIDC middleware
               * The app requires load balancing services to work with :80 or :443
               * These needs to be added to the keycloak client, in order for the redirect to work.
               * If you however intend to use the app by itself then,
               * Change the ports in launchsettings.json, but beware to also change the options.CallbackPath and options.SignedOutCallbackPath!
               * Use LB services whenever possible, to reduce the config hazzle :)
              */

              //Use default signin scheme
              options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
              //Keycloak server
              options.Authority = keycloakConfiguration["ServerRealm"];
              //Keycloak client ID
              options.ClientId = keycloakConfiguration["ClientId"];
              //Keycloak client secret
              options.ClientSecret = keycloakConfiguration["ClientSecret"];
              //Keycloak .wellknown config origin to fetch config
              options.MetadataAddress = keycloakConfiguration["Metadata"];
              //Require keycloak to use SSL
              options.RequireHttpsMetadata = false; // CHANGE FOR PROD
              options.GetClaimsFromUserInfoEndpoint = true;
              options.Scope.Add("openid");
              options.Scope.Add("profile");
              //Save the token
              options.SaveTokens = true;
              //Token response type, will sometimes need to be changed to IdToken, depending on config.
              options.ResponseType = OpenIdConnectResponseType.Code;
              //SameSite is needed for Chrome/Firefox, as they will give http error 500 back, if not set to unspecified.
              options.NonceCookie.SameSite = SameSiteMode.Unspecified;
              options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
              options.TokenValidationParameters = new TokenValidationParameters
              {
                NameClaimType = "name",
                RoleClaimType = ClaimTypes.Role,
                ValidateIssuer = true
              };
            });
    services.AddAuthorization(options =>
    {
      //Create policy with more than one claim
      options.AddPolicy("readonlyusers", policy =>
      {
        policy.RequireAssertion(context =>
        context.User.HasClaim(c => (c.Value == "user") || (c.Value == "admin")));
      });
      //Create policy with only one claim
      options.AddPolicy("administrators", policy =>
      {
        policy.RequireClaim(ClaimTypes.Role, "admin");
      });
      //Create a policy with a claim that doesn't exist or you are unauthorized to
      options.AddPolicy("noaccess", policy =>
      {
        policy.RequireClaim(ClaimTypes.Role, "noaccess");
      });
    });

  }
}
