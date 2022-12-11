using System.Net;
using Serilog;
using Microsoft.AspNetCore.Mvc;
using isz.lockbox.service.Models;
using isz.lockbox.service.Providers;
using System.Text.Json;

namespace isz.lockbox.service.Controllers;

[ApiController]
[Route("[controller]")]
public class LockboxController : ControllerBase
{
  private readonly Serilog.ILogger _logger;

  private LockboxServiceProvider _lockboxServiceProvider { get; }

  public LockboxController(Serilog.ILogger logger,
    LockboxServiceProvider lockboxServiceProvider
    )
  {
    _logger = logger;
    _lockboxServiceProvider = lockboxServiceProvider;
  }

  [HttpGet(Name = "GetLockboxes")]
  [Route("lockboxes")]
  public async Task<ActionResult> GetLocboxes()
  {
    _logger.Information("GetLocboxes");

    IEnumerable<Lockbox>? lockboxes = null;

    try
    {
      lockboxes = await _lockboxServiceProvider.GetLocboxesAsync();
    }
    catch (System.Exception ex)
    {
      _logger.Error(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to get lockboxes.");
    }

    return Ok(lockboxes);
  }

  [HttpGet(Name = "GetLockbox")]
  [Route("{lockboxId}")]
  public async Task<ActionResult> GetLocbox(string lockboxId)
  {
    _logger.Information($"GetLocbox - {JsonSerializer.Serialize(lockboxId)}");

    Lockbox? lockbox = null;

    try
    {
      lockbox = await _lockboxServiceProvider.GetLocboxAsync(lockboxId);
    }
    catch (System.Exception ex)
    {
      _logger.Error(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to get lockbox.");
    }

    return Ok(lockbox);
  }

  [HttpPost]
  [Route("update")]
  public async Task<ActionResult> Update([FromBody] Lockbox lockbox)
  {
    _logger.Information($"Update - {JsonSerializer.Serialize(lockbox)}");

    try
    {
      await _lockboxServiceProvider.UpsertAsync(lockbox);
    }
    catch (System.Exception ex)
    {
      _logger.Error(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to update lockbox.");
    }

    return Ok("OK");
  }


  [HttpPost]
  [Route("updatelockboxes")]
  public async Task<ActionResult> UpdateLockboxes([FromBody] List<Lockbox> lockboxes)
  {
    _logger.Information($"UpdateLockboxes - {JsonSerializer.Serialize(lockboxes)}");
    try
    {
      ParallelOptions parallelOptions = new()
      {
        MaxDegreeOfParallelism = 3
      };

      await Parallel.ForEachAsync(lockboxes, parallelOptions, async (lockbox, token) =>
      {
        await _lockboxServiceProvider.UpsertAsync(lockbox);
      });
    }
    catch (System.Exception ex)
    {
      _logger.Error(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to update lockbox.");
    }

    return Ok("OK");
  }


  [HttpGet(Name = "GenerateLockboxSecrets")]
  [Route("generate")]
  public async Task<ActionResult> GenerateLockboxSecrets()
  {
    _logger.Information($"GenerateLockboxSecrets");

    List<LockboxSecretPath> lockboxSecrets = new List<LockboxSecretPath> { };

    try
    {
      var lockboxes = await _lockboxServiceProvider.GetLocboxesAsync();

      var keys = new List<string>() { "db-username", "db-password", "cicd-user", "cicd-password", "exchange-clint-id", "exchange-clint-secret" };

      var count = 12345;

      lockboxes.ForEach<Lockbox>(l =>
      {
        l.Components?.ForEach<string>(c =>
        {
          keys.ForEach<string>(k =>
          {
            string path = $"{l.LockboxId}/{l.Application?.ToLower()}/{c.ToLower()}/{k.ToLower()}";

            var lockboxSecret = new LockboxSecretPath
            {
              PathId = count.ToString(),
              LockboxId = l.LockboxId?.ToLower(),
              Path = path
            };

            lockboxSecrets.Add(lockboxSecret);
            count++;
          });
        });
      });
    }
    catch (System.Exception ex)
    {
      _logger.Error(ex, "Seven Hells!");
      Response.StatusCode = (int)HttpStatusCode.BadRequest;
      return Content("Unable to update lockbox.");
    }
    return Ok(lockboxSecrets);
  }
}
