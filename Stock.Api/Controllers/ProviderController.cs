using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Stock.Api.DTOs;
using Stock.Api.Extensions;
using Stock.AppService.Services;
using Stock.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Stock.Api.Controllers
{
  	/// <summary>
  	/// Provider endpoint.
  	/// </summary>
  	[Produces("application/json")]
  	[Route("api/provider")]
  	[ApiController]
  	public class ProviderController : ControllerBase
  	{
    	private readonly ProviderService service;
    	private readonly ILogger<ProviderController> logger;
    	private readonly IMapper mapper;

	    /// <summary>
    	/// Initializes a new instance of the <see cref="ProviderController"/> class.
	    /// </summary>
    	/// <param name="service">Provider service.</param>
    	/// <param name="mapper">Mapper configurator.</param>
    	/// <param name="logger">Logger service.</param>
    	public ProviderController(ProviderService service, IMapper mapper, ILogger<ProviderController> logger)
    	{
      		this.service = service ?? throw new ArgumentException(nameof(service));
      		this.mapper = mapper ?? throw new ArgumentException(nameof(mapper));
      		this.logger = logger ?? throw new ArgumentException(nameof(logger));
    	}

    	/// <summary>
	    /// Adds a provider.
	    /// </summary>
		/// <param name="value">Provider info.</param>
    	/// <returns></returns>
    	[HttpPost]
    	public ActionResult Post([FromBody] ProviderDTO value)
    	{
      		TryValidateModel(value);

     		try
		    {
        		var provider = mapper.Map<Provider>(value);
        		service.Create(provider);
        		value.Id = provider.Id;
        		return Ok(new { Success = true, Message = "", data = value });
      		}
      		catch (Exception ex)
      		{
        		logger.LogCritical(ex.StackTrace);
        		return Ok(new { Success = false, Message = "The name is already in use", });
      		}
    	}

		/// <summary>
        /// Gets all providers.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
		public ActionResult<IEnumerable<ProviderDTO>> Get()
		{
			try
			{
        		var result = service.GetAll();
        		return mapper.Map<IEnumerable<ProviderDTO>>(result).ToList();
      		}
	  		catch (Exception) 
	  		{
		  		return StatusCode(500);
	  		}
		}

		/// <summary>
        /// Gets a provider by id.
        /// </summary>
        /// <param name="id">Provider id.</param>
        /// <returns></returns>
        [HttpGet("{id}")]
		public ActionResult<ProviderDTO> Get(string id)
		{
			try 
			{
        		var result = service.Get(id);
        		return mapper.Map<ProviderDTO>(result);
      		}
			catch (Exception) {
        		return StatusCode(500);
     		}
		}

		/// <summary>
        /// Updates a provider.
        /// </summary>
        /// <param name="id">Provider id.</param>
        /// <param name="value">Provider information.</param>
        [HttpPut("{id}")]
		public void Put(string id, [FromBody] ProviderDTO value)
		{
      		var provider = service.Get(id);
	  		TryValidateModel(value);
      		mapper.Map<ProviderDTO, Provider>(value, provider);
      		service.Update(provider);
    	}

        /// <summary>
        /// Deletes a provider.
        /// </summary>
        /// <param name="id">Provider id to delete</param>
        [HttpDelete("{id}")]
		public ActionResult Delete(string id)
		{
			var provider = service.Get(id);
			if (provider is null)
				return NotFound();

      		service.Delete(provider);
	  		return Ok(new { Success = true, Message = "", data = id});
    	}

		/// <summary>
        /// Search some providers.
        /// </summary>
        /// <param name="model">Provider filters.</param>
        /// <returns></returns>
        [HttpPost("search")]
		public ActionResult Search([FromBody] ProviderSearchDTO model)
		{
      		Expression<Func<Provider, bool>> filter = x => !string.IsNullOrWhiteSpace(x.Id);

	  		if (!string.IsNullOrWhiteSpace(model.Name))
	  		{
        		filter = filter.AndOrCustom(
          		x => x.Name.ToUpper().Contains(model.Name.ToUpper()),
         		model.Condition.Equals(ActionDto.AND));
      		}

			if (!string.IsNullOrWhiteSpace(model.Email))
			{
				filter = filter.AndOrCustom(
				x => x.Email.ToUpper().Contains(model.Email.ToUpper()),
				model.Condition.Equals(ActionDto.AND));
			}

      		var providers = service.Search(filter);
	  		return Ok(providers);
    	}
    }
}
