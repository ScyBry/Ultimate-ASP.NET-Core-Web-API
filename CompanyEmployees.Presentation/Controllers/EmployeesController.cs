using System.Text.Json;
using CompanyEmployees.Presentation.ActionFilters;
using Entities.LinkModels;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;

namespace CompanyEmployees.Presentation.Controllers;

[Route("api/companies/{companyId}/employees")]
[ApiController]
public class EmployeesController : ControllerBase
{
    private readonly IServiceManager _service;

    public EmployeesController(IServiceManager service)
    {
        _service = service;
    }

    [HttpGet]
    [HttpHead]
    [ServiceFilter(typeof(ValidateMediaTypeAttribute))]
    public async Task<IActionResult> GetEmployeesForCompany(Guid companyId,
        [FromQuery] EmployeeParameters employeeParameters)
    {
        var linkParams = new LinkParameters(employeeParameters, HttpContext);

        var result = await _service.EmployeeService.GetEmployeesAsync(companyId, linkParams, trackChanges: false);

        Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(result.metaData));

        return result.linkResponse.HasLinks
            ? Ok(result.linkResponse.LinkedEntities)
            : Ok(result.linkResponse.ShapedEntities);
    }

    [HttpGet("{id:guid}", Name = "GetEmployeeForCompany")]
    public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
    {
        var employee = await _service.EmployeeService.GetEmployeeAsync(
            companyId,
            id,
            false
        );
        return Ok(employee);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId,
        [FromBody] EmployeeForCreationDto employee)
    {
        if (employee is null)
            return BadRequest("EmployeeForCreationDto object is null");

        if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

        var employeeToReturn =
            await _service.EmployeeService.CreateEmployeeForCompanyAsync(companyId, employee, false);

        return CreatedAtRoute("GetEmployeeForCompany", new
            {
                companyId, id = employeeToReturn.Id
            },
            employeeToReturn);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
    {
        await _service.EmployeeService.DeleteEmployeeForCompanyAsync(companyId, id, false);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid id,
        [FromBody] EmployeeForUpdateDto employee)
    {
        if (employee is null)
            return BadRequest("EmployeeForUpdateDto object is null");

        if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

        await _service.EmployeeService.UpdateEmployeeForCompanyAsync(companyId, id, employee, false, true);

        return NoContent();
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> ParticallyUpdateEmployeeForCompany(Guid companyId, Guid id,
        [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDocument)
    {
        if (patchDocument is null)
            return BadRequest("patchDoc object sent from client is null.");

        var result = await _service.EmployeeService.GetEmployeeForPatchAsync(companyId, id, false, true);
        patchDocument.ApplyTo(result.employeeToPatch, ModelState);

        TryValidateModel(result.employeeToPatch);

        if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

        await _service.EmployeeService.SaveChangesForPatchAsync(result.employeeToPatch, result.employeeEntity);

        return NoContent();
    }
}