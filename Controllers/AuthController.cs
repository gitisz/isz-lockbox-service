using Microsoft.AspNetCore.Mvc;
using isz.lockbox.service.Providers;
using Keycloak.AuthServices.Sdk.AuthZ;
using Serilog;

namespace isz.lockbox.service.Controllers;

[Route("[controller]")]
public class AuthController : Controller
{
  private readonly Serilog.ILogger _logger;

  private readonly TokenExchangeProvider _tokenExchangeProvider;

  private readonly IKeycloakProtectionClient _protectionClient;

  public AuthController(Serilog.ILogger logger,
    IKeycloakProtectionClient protectionClient
    )
  {
    _logger = logger;
    _protectionClient = protectionClient;
  }


  [HttpGet("try-resource")]
  public async Task<IActionResult> VerifyAccess(
      [FromQuery] string? resource,
      [FromQuery] string? scope,
      CancellationToken cancellationToken)
  {
    var verified = await _protectionClient
        .VerifyAccessToResource(resource ?? "isz-lockbox-service", scope ?? "roles:read", cancellationToken);

    return this.Ok(verified);
  }

}
