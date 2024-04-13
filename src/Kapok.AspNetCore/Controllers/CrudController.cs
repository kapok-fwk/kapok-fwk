using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Kapok.BusinessLayer;
using Kapok.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Kapok.AspNetCore.Controllers;

/// <summary>
/// Generic controller for data entities with CRUD.
/// </summary>
/// <typeparam name="T"></typeparam>
//TODO[Authorize]
[Route("api/enty/[controller]")]
public class CrudController<T> : ReadonlyController<T>
    where T : class, new()
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="serviceProvider"></param>
    public CrudController(ILogger<CrudController<T>> logger, IServiceProvider serviceProvider)
        : base(logger, serviceProvider)
    {
    }

    // POST api/<controller>
    /// <summary>
    /// 
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    [HttpPost]
    //TODO[RequiresPermission(ClaimType.DataWrite)]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> PostAsync([FromBody]T entry)
    {
        try
        {
            if (entry == null)
            {
                Logger.LogError($"{typeof(T).Name} object sent from client is null.");
                return BadRequest($"{typeof(T).Name} object is null");
            }

            if (!ModelState.IsValid)
            {
                Logger.LogError($"Invalid {typeof(T).Name} object sent from client");
                return BadRequest("Invalid model object");
            }

            var entityModel = EntityBase.GetEntityModel<T>();
            var properties = entityModel.PrimaryKeyProperties;

            var primaryKeyValues = new object[properties.Length];
            int i = 0;
            foreach (var property in properties)
            {
                var propertyInfo =
                    typeof(T).GetProperty(property.Name, BindingFlags.Instance | BindingFlags.Public);

                if (propertyInfo == null || propertyInfo.GetMethod == null)
                    throw new NotSupportedException();

                primaryKeyValues[i++] = propertyInfo.GetMethod.Invoke(entry, null);
            }

            if (await DataDomainScope.GetDao<T>().FindByKeyAsync(primaryKeyValues) != null)
            {
                Logger.LogError($"{typeof(T).Name} with primary key {{{entry.GetPrimaryKeyAsString()}}} is already used.");
                return BadRequest("Primary key is already used.");
            }

            await DataDomainScope.GetDao<T>().CreateAsync(entry);
            await DataDomainScope.SaveAsync();

            return Created($"/{GetRouteTemplate()}/GetByKey?values={string.Join("&values=", primaryKeyValues)}", entry);
            //return CreatedAtRoute("GetByKey", primaryKeyValues, entry);
        }
        catch (Exception ex)
        {
            Logger.LogError($"Something went wrong inside {nameof(PostAsync)} action: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT api/<controller>/&k=primaryKeyValue1&k=primaryKeyValue2
    /// <summary>
    /// 
    /// </summary>
    /// <param name="k"></param>
    /// <param name="entry"></param>
    /// <returns></returns>
    [HttpPut]
    //TODO[RequiresPermission(ClaimType.DataWrite)]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> PutAsync([FromQuery] string[] k, [FromBody]T entry)
    {
        var values = k;

        if (values == null)
        {
            Logger.LogError($"No field given for {nameof(PutAsync)} for type {typeof(T).Name}.");
            return StatusCode(500, "No primary key field was given");
        }

        try
        {
            if (entry == null)
            {
                Logger.LogError($"{typeof(T).Name} object sent from client is null.");
                return BadRequest($"{typeof(T).Name} object is null");
            }
 
            if (!ModelState.IsValid)
            {
                Logger.LogError($"Invalid {typeof(T).Name} object sent from client.");
                return BadRequest("Invalid model object");
            }

            // ReSharper disable once RedundantEnumerableCastCall
            var dbEntry = await DataDomainScope.GetDao<T>().FindByKeyAsync(values.Cast<object>().ToArray());
            if (dbEntry == null)
            {
                Logger.LogError($"{nameof(CLSCompliantAttribute)} with primary key {{{entry.GetPrimaryKeyAsString()}}} hasn't been found in db.");
                return NotFound();
            }
 
            dbEntry.Map(entry);
            await DataDomainScope.GetDao<T>().UpdateAsync(dbEntry);
            await DataDomainScope.SaveAsync();
 
            return NoContent();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Something went wrong inside {nameof(PutAsync)} action: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }

    }

    // DELETE api/<controller>/&k=primaryKeyValue1&k=primaryKeyValue2
    /// <summary>
    /// 
    /// </summary>
    /// <param name="k"></param>
    /// <returns></returns>
    [HttpDelete]
    //TODO[RequiresPermission(ClaimType.DataWrite)]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Delete([FromQuery] string[] k)
    {
        var values = k;

        if (values == null)
        {
            Logger.LogError($"No field given for {nameof(Delete)} for type {typeof(T).Name}.");
            return StatusCode(500, "No primary key field was given");
        }

        try
        {
            // ReSharper disable once RedundantEnumerableCastCall
            var dbEntry = await DataDomainScope.GetDao<T>().FindByKeyAsync(values.Cast<object>().ToArray());
            if (dbEntry == null)
            {
                Logger.LogError($"{typeof(T).Name} with primary key {{{string.Join(", ", values)}}} hasn't been found in db.");
                return NotFound();
            }

            await DataDomainScope.GetDao<T>().DeleteAsync(dbEntry);
            await DataDomainScope.SaveAsync();
 
            return NoContent();
        }
        catch (Exception ex)
        {
            Logger.LogError($"Something went wrong inside {nameof(Delete)} action: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }
}