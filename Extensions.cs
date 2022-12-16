using isz.lockbox.service.Models;
using isz.lockbox.service.Providers;
using Keycloak.AuthServices.Authorization;
using Keycloak.AuthServices.Sdk.AuthZ;
using Keycloak.AuthServices.Sdk.HttpMiddleware;

namespace isz.lockbox.service;

public static class ExtensionMethods
{
  public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action)
  {
    if (action == null)
    {
      throw new ArgumentNullException(nameof(action));
    }
    foreach (T item in sequence)
    {
      action(item);
    }
  }

  public static IHttpClientBuilder AddKeycloakProtectionHttpClient(
        this IServiceCollection services,
        KeycloakProtectionClientOptions keycloakOptions,
        Action<HttpClient>? configureClient = default)
    {
        services.AddSingleton(keycloakOptions);
        services.AddHttpContextAccessor();

        return services.AddHttpClient<IKeycloakProtectionClient, KeycloakProtectionClient>()
            .ConfigureHttpClient(client =>
            {
                var baseUrl = new Uri(keycloakOptions.KeycloakUrlRealm.TrimEnd('/') + "/");
                client.BaseAddress = baseUrl;
                configureClient?.Invoke(client);
            }).AddHeaderPropagation();
    }
}