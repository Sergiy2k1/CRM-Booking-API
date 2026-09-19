using BookingHub.Api.Authorization;
using BookingHub.Api.Contracts.Services;
using BookingHub.Application.Services.Assignments.AssignService;
using BookingHub.Application.Services.Assignments.ListEmployeeServices;
using BookingHub.Application.Services.Assignments.UnassignService;
using BookingHub.Application.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.OrganizationAccess)]
[Route("api/organizations/{organizationId:guid}/employees/{employeeId:guid}/services")]
public sealed class EmployeeServicesController : ControllerBase
{
    private readonly AssignServiceHandler _assignServiceHandler;
    private readonly ListEmployeeServicesHandler _listEmployeeServicesHandler;
    private readonly UnassignServiceHandler _unassignServiceHandler;

    public EmployeeServicesController(
        AssignServiceHandler assignServiceHandler,
        ListEmployeeServicesHandler listEmployeeServicesHandler,
        UnassignServiceHandler unassignServiceHandler)
    {
        _assignServiceHandler = assignServiceHandler;
        _listEmployeeServicesHandler = listEmployeeServicesHandler;
        _unassignServiceHandler = unassignServiceHandler;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<EmployeeServiceResponse>>> List(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var result =
            await _listEmployeeServicesHandler.HandleAsync(
                new ListEmployeeServicesQuery(
                    organizationId,
                    employeeId),
                cancellationToken);

        return Ok(result.Select(Map).ToArray());
    }

    [HttpPost("{serviceId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ServiceManagement)]
    public async Task<ActionResult<EmployeeServiceResponse>> Assign(
        Guid organizationId,
        Guid employeeId,
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        var result =
            await _assignServiceHandler.HandleAsync(
                new AssignServiceCommand(
                    organizationId,
                    employeeId,
                    serviceId),
                cancellationToken);

        return Created(
            $"/api/organizations/{organizationId}/employees/{employeeId}/services/{serviceId}",
            Map(result));
    }

    [HttpDelete("{serviceId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ServiceManagement)]
    public async Task<IActionResult> Unassign(
        Guid organizationId,
        Guid employeeId,
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        await _unassignServiceHandler.HandleAsync(
            new UnassignServiceCommand(
                organizationId,
                employeeId,
                serviceId),
            cancellationToken);

        return NoContent();
    }

    private static EmployeeServiceResponse Map(
        EmployeeServiceDetails assignment)
    {
        return new EmployeeServiceResponse(
            assignment.Id,
            assignment.OrganizationId,
            assignment.EmployeeId,
            assignment.ServiceId,
            assignment.AssignedAtUtc);
    }
}
