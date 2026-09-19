using BookingHub.Api.Authorization;
using BookingHub.Api.Contracts.Employees;
using BookingHub.Application.Employees.ChangeEmployeeStatus;
using BookingHub.Application.Employees.Common;
using BookingHub.Application.Employees.CreateEmployee;
using BookingHub.Application.Employees.GetEmployee;
using BookingHub.Application.Employees.ListEmployees;
using BookingHub.Application.Employees.UpdateEmployee;
using BookingHub.Domain.Employees;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.OrganizationAccess)]
[Route("api/organizations/{organizationId:guid}/employees")]
public sealed class EmployeesController : ControllerBase
{
    private readonly CreateEmployeeHandler _createEmployeeHandler;
    private readonly GetEmployeeHandler _getEmployeeHandler;
    private readonly ListEmployeesHandler _listEmployeesHandler;
    private readonly UpdateEmployeeHandler _updateEmployeeHandler;
    private readonly ChangeEmployeeStatusHandler _changeEmployeeStatusHandler;

    public EmployeesController(
        CreateEmployeeHandler createEmployeeHandler,
        GetEmployeeHandler getEmployeeHandler,
        ListEmployeesHandler listEmployeesHandler,
        UpdateEmployeeHandler updateEmployeeHandler,
        ChangeEmployeeStatusHandler changeEmployeeStatusHandler)
    {
        _createEmployeeHandler = createEmployeeHandler;
        _getEmployeeHandler = getEmployeeHandler;
        _listEmployeesHandler = listEmployeesHandler;
        _updateEmployeeHandler = updateEmployeeHandler;
        _changeEmployeeStatusHandler = changeEmployeeStatusHandler;
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.EmployeeManagement)]
    public async Task<ActionResult<EmployeeResponse>> Create(
        Guid organizationId,
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var employee =
            await _createEmployeeHandler.HandleAsync(
                new CreateEmployeeCommand(
                    organizationId,
                    request.FirstName,
                    request.LastName,
                    request.Position),
                cancellationToken);

        return Created(
            $"/api/organizations/{organizationId}/employees/{employee.Id}",
            Map(employee));
    }

    [HttpGet("{employeeId:guid}")]
    public async Task<ActionResult<EmployeeResponse>> GetById(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var employee =
            await _getEmployeeHandler.HandleAsync(
                new GetEmployeeQuery(
                    organizationId,
                    employeeId),
                cancellationToken);

        return Ok(Map(employee));
    }

    [HttpGet]
    public async Task<ActionResult<EmployeeListResponse>> List(
        Guid organizationId,
        [FromQuery] EmployeeStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result =
            await _listEmployeesHandler.HandleAsync(
                new ListEmployeesQuery(
                    organizationId,
                    status,
                    page,
                    pageSize),
                cancellationToken);

        return Ok(
            new EmployeeListResponse(
                result.Items.Select(Map).ToArray(),
                result.Page,
                result.PageSize,
                result.TotalCount));
    }

    [HttpPut("{employeeId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.EmployeeManagement)]
    public async Task<ActionResult<EmployeeResponse>> Update(
        Guid organizationId,
        Guid employeeId,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var employee =
            await _updateEmployeeHandler.HandleAsync(
                new UpdateEmployeeCommand(
                    organizationId,
                    employeeId,
                    request.FirstName,
                    request.LastName,
                    request.Position),
                cancellationToken);

        return Ok(Map(employee));
    }

    [HttpPost("{employeeId:guid}/deactivate")]
    [Authorize(Policy = AuthorizationPolicies.EmployeeManagement)]
    public Task<ActionResult<EmployeeResponse>> Deactivate(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        return ChangeStatus(
            organizationId,
            employeeId,
            false,
            cancellationToken);
    }

    [HttpPost("{employeeId:guid}/activate")]
    [Authorize(Policy = AuthorizationPolicies.EmployeeManagement)]
    public Task<ActionResult<EmployeeResponse>> Activate(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        return ChangeStatus(
            organizationId,
            employeeId,
            true,
            cancellationToken);
    }

    private async Task<ActionResult<EmployeeResponse>> ChangeStatus(
        Guid organizationId,
        Guid employeeId,
        bool activate,
        CancellationToken cancellationToken)
    {
        var employee =
            await _changeEmployeeStatusHandler.HandleAsync(
                new ChangeEmployeeStatusCommand(
                    organizationId,
                    employeeId,
                    activate),
                cancellationToken);

        return Ok(Map(employee));
    }

    private static EmployeeResponse Map(
        EmployeeDetails employee)
    {
        return new EmployeeResponse(
            employee.Id,
            employee.OrganizationId,
            employee.UserId,
            employee.FirstName,
            employee.LastName,
            employee.Position,
            employee.Status,
            employee.CreatedAtUtc,
            employee.UpdatedAtUtc);
    }
}
