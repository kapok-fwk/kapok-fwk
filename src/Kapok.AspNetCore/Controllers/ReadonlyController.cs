using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Kapok.BusinessLayer;
using Kapok.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kapok.AspNetCore.Controllers;

/// <summary>
/// Generic controller for read-only entities.
/// </summary>
/// <typeparam name="T"></typeparam>
//TODO[Authorize]
[Route("api/enty/[controller]")]
public class ReadonlyController<T> : EntityBaseController<T>
    where T : class, new()
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="serviceProvider"></param>
    public ReadonlyController(ILogger<ReadonlyController<T>> logger, IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    // GET: api/<controller>
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [HttpGet("all")]
    //TODO[RequiresPermission(ClaimType.DataRead)]
    [ProducesResponseType(200, Type = typeof(IEnumerable))]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetAllAsync()
    {
#if !DEBUG
        try
#endif
        {
            var entries = await DataDomainScope.GetDao<T>().AsQueryable().ToListAsync();

            Logger.LogInformation($"Returned all entries from type {typeof(T).Name} from database.");

            return Ok(entries);
        }
#if !DEBUG
        catch (Exception ex)
        {
            Logger.LogError($"Something went wrong inside {nameof(GetAllAsync)} action: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
#endif
    }

    // GET api/<controller>/GetByKey/&k=primaryKeyValue1&k=primaryKeyValue2
    /// <summary>
    /// 
    /// </summary>
    /// <param name="k"></param>
    /// <returns></returns>
    [HttpGet]
    //[ActionName("GetByKey")]
    //TODO[RequiresPermission(ClaimType.DataRead)]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<ActionResult<T>> GetByKeyAsync([FromQuery] string[] k)
    {
        var values = k;

        if (values == null)
        {
            Logger.LogError($"No field given for {nameof(GetByKeyAsync)} for type {typeof(T).Name}.");
            return StatusCode(500, "No primary key field was given");
        }

        try
        {
            // ReSharper disable once RedundantEnumerableCastCall
            var entry = await DataDomainScope.GetDao<T>().FindByKeyAsync(values.Cast<object>().ToArray());
            if (entry == null)
            {
                Logger.LogError($"{typeof(T).Name} with {{{string.Join(", ", values)}}} hasn't been found in db.");
                return NotFound();
            }

            // TODO: this should be changed so no reference is necessary to 'IEntityFrameworkCoreDataDomainScope'
            Logger.LogInformation($"Returned {typeof(T).Name} with {{{entry.GetPrimaryKeyAsString()}}}");
            return entry;
        }
        catch (Exception ex)
        {
            Logger.LogError($"Something went wrong inside {nameof(GetByKeyAsync)} action: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
}