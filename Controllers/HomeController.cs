using System.Net;
using Microsoft.AspNetCore.Mvc;
using isz.lockbox.service.Models;
using isz.lockbox.service.Providers;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace isz.lockbox.service.Controllers;

[Route("[controller]")]
public class HomeController : Controller
{
  private readonly ILogger<HomeController> _logger;

  private readonly TokenExchangeProvider _tokenExchangeProvider;

  public HomeController(ILogger<HomeController> logger,
    TokenExchangeProvider tokenExchangeProvider
    )
  {
    _logger = logger;
    _tokenExchangeProvider = tokenExchangeProvider;
  }

  // [Authorize(Policy = "administrators")]
  // public IActionResult AuthenticationAdmin()
  // {
  //   return Ok();
  // }

  [Authorize(Policy = "readonlyusers")]
  public async Task<IActionResult> AuthenticationAsync()
  {
    //Find claims for the current user
    ClaimsPrincipal currentUser = this.User;
    //Get username, for keycloak you need to regex this to get the clean username
    var currentUserName = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    //logs an error so it's easier to find - thanks debug.
    _logger.LogError(currentUserName);

    //Debug this line of code if you want to validate the content jwt.io
    string? accessToken = await HttpContext.GetTokenAsync("access_token");
    string? idToken = await HttpContext.GetTokenAsync("id_token");
    string? refreshToken = await HttpContext.GetTokenAsync("refresh_token");

    var newAccessToken = await _tokenExchangeProvider.GetRefreshTokenAsync(refreshToken ?? "");
    var serviceAccessToken = await _tokenExchangeProvider.GetTokenExchangeAsync(newAccessToken);

    return Ok();
  }
}
