using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using isz.lockbox.service.Models;
using isz.lockbox.service.Providers;

namespace isz.lockbox.service.Controllers;

[ApiController]
[Route("[controller]")]
public class LockboxSecretController : ControllerBase
{
  private readonly ILogger<LockboxSecretController> _logger;

  private LockboxSecretServiceProvider _lockboxSecretServiceProvider { get; }

  public LockboxSecretController(ILogger<LockboxSecretController> logger,
    LockboxSecretServiceProvider lockboxSecretServiceProvider
    )
  {
    _logger = logger;
    _lockboxSecretServiceProvider = lockboxSecretServiceProvider;
  }

  [HttpPost]
  [Route("update")]
  public async Task<ActionResult> Update([FromBody] LockboxSecret  lockboxSecret)
  {
    try
    {
      if (string.IsNullOrEmpty(lockboxSecret.PathId))
      {
        throw new ArgumentException("A valid secret was not provided.");
      }

      if (lockboxSecret.Secrets == null)
      {
        throw new ArgumentException("A valid secret was not provided.");
      }

      if (lockboxSecret.Secrets.Count() == 0)
      {
        throw new ArgumentException("A valid secret was not provided.");
      }

      if (!(lockboxSecret.Secrets.First() is Secret))
      {
        throw new ArgumentException("A valid secret was not provided.");
      }

      await _lockboxSecretServiceProvider.UpsertAsync(lockboxSecret);

    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content($"Unable to update lockbox secret. {ex.Message}");
    }

    return Ok("OK");
  }


  [HttpGet(Name = "GetLockboxSecret")]
  [Route("{pathId}")]
  public async Task<ActionResult> GetLocboxSecret(string pathId)
  {
    LockboxSecret? lockboxSecret = null;

    try
    {
      lockboxSecret = await _lockboxSecretServiceProvider.GetLocboxSecretAsync(pathId);
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to get lockbox secret.");
    }

    return Ok(lockboxSecret);
  }
}
