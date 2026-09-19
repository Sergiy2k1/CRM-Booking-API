using BookingHub.Api.Authorization;
using BookingHub.Api.Contracts.Employees;
using BookingHub.Application.Employees.Common;
using BookingHub.Application.Employees.Schedules.CancelTimeOff;
using BookingHub.Application.Employees.Schedules.CreateTimeOff;
using BookingHub.Application.Employees.Schedules.CreateWorkingHours;
using BookingHub.Application.Employees.Schedules.ListTimeOff;
using BookingHub.Application.Employees.Schedules.ListWorkingHours;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.OrganizationAccess)]
[Route("api/organizations/{organizationId:guid}/employees/{employeeId:guid}")]
public sealed class EmployeeSchedulesController : ControllerBase
{
    private readonly CreateWorkingHoursHandler _createWorkingHoursHandler;
    private readonly ListWorkingHoursHandler _listWorkingHoursHandler;
    private readonly CreateTimeOffHandler _createTimeOffHandler;
    private readonly ListTimeOffHandler _listTimeOffHandler;
    private readonly CancelTimeOffHandler _cancelTimeOffHandler;

    public EmployeeSchedulesController(
        CreateWorkingHoursHandler createWorkingHoursHandler,
        ListWorkingHoursHandler listWorkingHoursHandler,
        CreateTimeOffHandler createTimeOffHandler,
        ListTimeOffHandler listTimeOffHandler,
        CancelTimeOffHandler cancelTimeOffHandler)
    {
        _createWorkingHoursHandler = createWorkingHoursHandler;
        _listWorkingHoursHandler = listWorkingHoursHandler;
        _createTimeOffHandler = createTimeOffHandler;
        _listTimeOffHandler = listTimeOffHandler;
        _cancelTimeOffHandler = cancelTimeOffHandler;
    }

    [HttpGet("working-hours")]
    public async Task<ActionResult<IReadOnlyCollection<WorkingHoursResponse>>> ListWorkingHours(
        Guid organizationId,
        Guid employeeId,
        CancellationToken cancellationToken)
    {
        var result =
            await _listWorkingHoursHandler.HandleAsync(
                new ListWorkingHoursQuery(
                    organizationId,
                    employeeId),
                cancellationToken);

        return Ok(
            result.Select(Map).ToArray());
    }

    [HttpPost("working-hours")]
    [Authorize(Policy = AuthorizationPolicies.EmployeeManagement)]
    public async Task<ActionResult<WorkingHoursResponse>> CreateWorkingHours(
        Guid organizationId,
        Guid employeeId,
        CreateWorkingHoursRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _createWorkingHoursHandler.HandleAsync(
                new CreateWorkingHoursCommand(
                    organizationId,
                    employeeId,
                    request.DayOfWeek,
                    request.StartTime,
                    request.EndTime),
                cancellationToken);

        return Created(
            $"/api/organizations/{organizationId}/employees/{employeeId}/working-hours/{result.Id}",
            Map(result));
    }

    [HttpGet("time-off")]
    public async Task<ActionResult<IReadOnlyCollection<TimeOffResponse>>> ListTimeOff(
        Guid organizationId,
        Guid employeeId,
        [FromQuery] DateTimeOffset startsAtUtc,
        [FromQuery] DateTimeOffset endsAtUtc,
        CancellationToken cancellationToken)
    {
        var result =
            await _listTimeOffHandler.HandleAsync(
                new ListTimeOffQuery(
                    organizationId,
                    employeeId,
                    startsAtUtc,
                    endsAtUtc),
                cancellationToken);

        return Ok(
            result.Select(Map).ToArray());
    }

    [HttpPost("time-off")]
    [Authorize(Policy = AuthorizationPolicies.EmployeeManagement)]
    public async Task<ActionResult<TimeOffResponse>> CreateTimeOff(
        Guid organizationId,
        Guid employeeId,
        CreateTimeOffRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _createTimeOffHandler.HandleAsync(
                new CreateTimeOffCommand(
                    organizationId,
                    employeeId,
                    request.StartsAtUtc,
                    request.EndsAtUtc,
                    request.Reason),
                cancellationToken);

        return Created(
            $"/api/organizations/{organizationId}/employees/{employeeId}/time-off/{result.Id}",
            Map(result));
    }

    [HttpPost("time-off/{timeOffId:guid}/cancel")]
    [Authorize(Policy = AuthorizationPolicies.EmployeeManagement)]
    public async Task<ActionResult<TimeOffResponse>> CancelTimeOff(
        Guid organizationId,
        Guid employeeId,
        Guid timeOffId,
        CancellationToken cancellationToken)
    {
        var result =
            await _cancelTimeOffHandler.HandleAsync(
                new CancelTimeOffCommand(
                    organizationId,
                    employeeId,
                    timeOffId),
                cancellationToken);

        return Ok(Map(result));
    }

    private static WorkingHoursResponse Map(
        WorkingHoursDetails details)
    {
        return new WorkingHoursResponse(
            details.Id,
            details.OrganizationId,
            details.EmployeeId,
            details.DayOfWeek,
            details.StartTime,
            details.EndTime);
    }

    private static TimeOffResponse Map(
        TimeOffDetails details)
    {
        return new TimeOffResponse(
            details.Id,
            details.OrganizationId,
            details.EmployeeId,
            details.StartsAtUtc,
            details.EndsAtUtc,
            details.Reason,
            details.Status,
            details.CreatedAtUtc,
            details.UpdatedAtUtc,
            details.CancelledAtUtc);
    }
}
