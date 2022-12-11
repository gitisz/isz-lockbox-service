using System.Net;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Mvc;
using isz.lockbox.service.Models;
using isz.lockbox.service.Providers;

namespace isz.lockbox.service.Controllers;

[ApiController]
[Route("[controller]")]
public class LockboxAccessController : ControllerBase
{
  private readonly ILogger<LockboxController> _logger;

  private LockboxAccessServiceProvider _lockboxAccessServiceProvider { get; }

  public LockboxAccessController(ILogger<LockboxController> logger,
    LockboxAccessServiceProvider lockboxAccessServiceProvider
    )
  {
    _logger = logger;
    _lockboxAccessServiceProvider = lockboxAccessServiceProvider;
  }


  [HttpGet(Name = "GetLockboxAccesses")]
  [Route("accesses")]
  public async Task<ActionResult> GetLocboxAccesses()
  {
    IEnumerable<LockboxAccess>? lockboxAccesses = null;

    try
    {
      lockboxAccesses = await _lockboxAccessServiceProvider.GetLocboxAccessesAsync();
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to get lockbox accesses.");
    }

    return Ok(lockboxAccesses);
  }


  [HttpGet(Name = "GetLockboxAccessesByLockboxId")]
  [Route("accesses/{lockboxId}")]
  public async Task<ActionResult> GetLocboxAccessesByLockboxId(string lockboxId)
  {
    IEnumerable<LockboxAccess>? lockboxAccesses = null;

    try
    {
      lockboxAccesses = await _lockboxAccessServiceProvider.GetLocboxAccessesByLockboxIdAsync(lockboxId);
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to get lockbox accesses.");
    }

    return Ok(lockboxAccesses);
  }

  [HttpGet(Name = "GetLockboxAccess")]
  [Route("{accessId}")]
  public async Task<ActionResult> GetLocboxAccess(string accessId)
  {
    LockboxAccess? lockboxAccess = null;

    try
    {
      lockboxAccess = await _lockboxAccessServiceProvider.GetLocboxAccessAsync(accessId);
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to get lockbox access.");
    }

    return Ok(lockboxAccess);
  }

  [HttpPost]
  [Route("update")]
  public async Task<ActionResult> Update([FromBody] LockboxAccess lockboxAccess)
  {
    try
    {
      var allAccesses = await _lockboxAccessServiceProvider.GetLocboxAccessesAsync();

      var accesses = await _lockboxAccessServiceProvider.GetLocboxAccessesByLockboxIdAsync(lockboxAccess.LockboxId ?? "");

      var access = accesses
        .Where(a => a.AccessId == lockboxAccess.AccessId)
        .FirstOrDefault();

      if (access == null)
      {
        var accessId = allAccesses
            .OrderByDescending(s => s.AccessId)
            .Select(s =>
            {
              int id = Convert.ToInt32(s.AccessId);
              id += 1;
              return id.ToString();
            })
            .First();

        access = new LockboxAccess
        {
          AccessId = accessId,
          VaultRole = Guid.NewGuid().ToString()
        };
      }

      // We should not be updating an access, but the pattern here will support it.
      access.AccessType = lockboxAccess.AccessType;
      access.LockboxId = lockboxAccess.LockboxId;
      access.Permissions = lockboxAccess.Permissions;
      access.ResourceName = lockboxAccess.ResourceName;

      await _lockboxAccessServiceProvider.UpsertAsync(access);
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content($"Unable to update lockbox access. {ex.Message}");
    }

    return Ok("OK");
  }


  [HttpPost]
  [Route("updateaccesses")]
  public async Task<ActionResult> UpdateLockboxAccesses([FromBody] List<LockboxAccess> lockboxAccesses)
  {
    try
    {
      ParallelOptions parallelOptions = new()
      {
        MaxDegreeOfParallelism = 3
      };

      await Parallel.ForEachAsync(lockboxAccesses, parallelOptions, async (lockboxAccess, token) =>
      {
        await _lockboxAccessServiceProvider.UpsertAsync(lockboxAccess);
      });
    }
    catch (System.Exception ex)
    {
      _logger.LogError(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to update lockbox accesses.");
    }

    return Ok("OK");
  }
}
