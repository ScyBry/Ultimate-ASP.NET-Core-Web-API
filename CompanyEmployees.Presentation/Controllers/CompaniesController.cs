﻿using CompanyEmployees.Presentation.ActionFilters;
using CompanyEmployees.Presentation.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;

namespace CompanyEmployees.Presentation.Controllers;

[ApiVersion("1.0")]
[Route("api/[controller]")]
[ApiController]
[ResponseCache(CacheProfileName = "120SecondsDuration")]
public class CompaniesController : ControllerBase
{
    private readonly IServiceManager _service;

    public CompaniesController(IServiceManager service)
    {
        _service = service;
    }

    [HttpOptions]
    public IActionResult GetCompaniesOptions()
    {
        Response.Headers.Add("Allow", "GET, OPTIONS, POST");

        return Ok();
    }

    [HttpGet(Name = "GetCompanies")]
    public async Task<IActionResult> GetCompanies()
    {
        var companies = await _service.CompanyService.GetAllCompaniesAsync(false);
        return Ok(companies);
    }


    [HttpGet("{id:guid}", Name = "CompanyById")]
    public async Task<IActionResult> GetCompany(Guid id)
    {
        var company = await _service.CompanyService.GetCompanyAsync(id, false);
        return Ok(company);
    }

    [HttpPost(Name = "CreateCompany")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto company)
    {
        var createdCompany = await _service.CompanyService.CreateCompanyAsync(company);

        return CreatedAtRoute("CompanyById", new { id = createdCompany.Id },
            createdCompany);
    }

    [HttpGet("collection/({ids})", Name = "CompanyCollection")]
    public async Task<IActionResult> GetCompanyCollection(
        [ModelBinder(BinderType = typeof(ArrayModelBinder))]
        IEnumerable<Guid> ids)
    {
        var companies = await _service.CompanyService.GetByIdsAsync(ids, false);

        return Ok(companies);
    }

    [HttpPost("collection")]
    public async Task<IActionResult> CreateCompanyCollection(
        [FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
    {
        var result = await _service.CompanyService.CreateCompanyCollectionAsync(companyCollection);

        return CreatedAtRoute("CompanyCollection", new { result.ids },
            result.companies);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCompany(Guid id)
    {
        await _service.CompanyService.DeleteCompanyAsync(id, false);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [ServiceFilter(typeof(ValidationFilterAttribute))]
    public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] CompanyForUpdateDto companyForUpdate)
    {
        await _service.CompanyService.UpdateCompanyAsync(id, companyForUpdate, true);

        return NoContent();
    }
}