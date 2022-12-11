using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using isz.lockbox.service.Models;
using isz.lockbox.service.Providers;

namespace isz.lockbox.service.Controllers;

[ApiController]
[Route("[controller]")]
public class LockboxSecretPathController : ControllerBase
{
  private readonly ILogger<LockboxSecretPathController> _logger;

  private LockboxSecretPathServiceProvider _lockboxSecretPathServiceProvider { get; }

  public LockboxSecretPathController(ILogger<LockboxSecretPathController> logger,
    LockboxSecretPathServiceProvider lockboxSecretPathServiceProvider
    )
  {
    _logger = logger;
    _lockboxSecretPathServiceProvider = lockboxSecretPathServiceProvider;
  }

  [HttpGet(Name = "GetLockboxSecretPathByLockboxId")]
  [Route("secretpaths/{lockboxId}")]
  public async Task<ActionResult> GetLockboxSecretPathByLockboxId(string lockboxId)
  {
    IEnumerable<LockboxSecretPath>? lockboxSecretPath = null;

    try
    {
      lockboxSecretPath = await _lockboxSecretPathServiceProvider.GetLockboxSecretPathsByLockboxIdAsync(lockboxId);
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content($"Unable to get lockbox secrets. Exception: {ex.ToString()}");
    }

    return Ok(lockboxSecretPath);
  }

  [HttpGet(Name = "GetLockboxSecretPath")]
  [Route("{accessId}")]
  public async Task<ActionResult> GetLocboxSecret(string accessId)
  {
    LockboxSecretPath? lockboxSecret = null;

    try
    {
      lockboxSecret = await _lockboxSecretPathServiceProvider.GetLocboxSecretPathAsync(accessId);
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content($"Unable to get lockbox secret. Exception: {ex.ToString()}");
    }

    return Ok(lockboxSecret);
  }

  [HttpPost]
  [Route("update")]
  public async Task<ActionResult> Update([FromBody] LockboxSecretPath lockboxSecretPath)
  {
    try
    {
      if (string.IsNullOrEmpty(lockboxSecretPath.Path))
      {
        throw new ArgumentException("A secret path was not provided.");
      }

      if (!string.IsNullOrEmpty(lockboxSecretPath.Path))
      {
        if (!lockboxSecretPath.Path.StartsWith(lockboxSecretPath.LockboxId ?? ""))
        {
          throw new ArgumentException("The secret path is not prefixed with the correct Lockbox ID.");
        }
      }

      if(!Regex.IsMatch(lockboxSecretPath.Path, @"^\w+([\s-_\/]\w+)*$"))
      {
          throw new ArgumentException("The secret path provided is not valid.");
      }

      var allSecretPaths = await _lockboxSecretPathServiceProvider.GetLockboxSecretPathsByLockboxIdAsync(lockboxSecretPath.LockboxId ?? "");

      var secrePath = allSecretPaths
        .Where(a => a.PathId == lockboxSecretPath.PathId)
        .FirstOrDefault();

      if (secrePath == null)
      {
        var hasSecretPath = allSecretPaths.Where(s => {
          return String.Equals(s.Path, lockboxSecretPath.Path, StringComparison.CurrentCultureIgnoreCase);
        }).Any();

        if (hasSecretPath)
        {
          throw new ArgumentException($"The Lockbox {lockboxSecretPath.LockboxId} already has a path matching {lockboxSecretPath.Path}.");
        }

        var pathId = allSecretPaths
          .OrderByDescending(s => s.PathId)
          .Select(s =>
          {
            int id = Convert.ToInt32(s.PathId);
            id += 1;
            return id.ToString();
          })
          .First();

        secrePath = new LockboxSecretPath
        {
          PathId = pathId,
        };
      }

      // We should not be updating a secret path, but the pattern here will support it.
      secrePath.LockboxId = lockboxSecretPath.LockboxId;
      secrePath.Path = lockboxSecretPath.Path;

      await _lockboxSecretPathServiceProvider.UpsertAsync(secrePath);
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content($"Unable to update lockbox secret path. {ex.Message}");
    }

    return Ok("OK");
  }

  [HttpPost]
  [Route("updatesecretpaths")]
  public async Task<ActionResult> UpdateLockboxSecretPath([FromBody] List<LockboxSecretPath> lockboxSecretPath)
  {
    try
    {
      ParallelOptions parallelOptions = new()
      {
        MaxDegreeOfParallelism = 3
      };

      await Parallel.ForEachAsync(lockboxSecretPath, parallelOptions, async (lsp, token) =>
      {
        await _lockboxSecretPathServiceProvider.UpsertAsync(lsp);
      });
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content($"Unable to update lockbox secrets.  Exception: {ex.ToString()}");
    }

    return Ok("OK");
  }
}
