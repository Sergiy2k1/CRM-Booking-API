using BookingHub.Api.Authorization;
using BookingHub.Api.Contracts.Services;
using BookingHub.Application.Services.ChangeServiceStatus;
using BookingHub.Application.Services.Common;
using BookingHub.Application.Services.CreateService;
using BookingHub.Application.Services.GetService;
using BookingHub.Application.Services.ListServices;
using BookingHub.Application.Services.UpdateService;
using BookingHub.Domain.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.OrganizationAccess)]
[Route("api/organizations/{organizationId:guid}/services")]
public sealed class ServicesController : ControllerBase
{
    private readonly CreateServiceHandler _createServiceHandler;
    private readonly GetServiceHandler _getServiceHandler;
    private readonly ListServicesHandler _listServicesHandler;
    private readonly UpdateServiceHandler _updateServiceHandler;
    private readonly ChangeServiceStatusHandler _changeServiceStatusHandler;

    public ServicesController(
        CreateServiceHandler createServiceHandler,
        GetServiceHandler getServiceHandler,
        ListServicesHandler listServicesHandler,
        UpdateServiceHandler updateServiceHandler,
        ChangeServiceStatusHandler changeServiceStatusHandler)
    {
        _createServiceHandler = createServiceHandler;
        _getServiceHandler = getServiceHandler;
        _listServicesHandler = listServicesHandler;
        _updateServiceHandler = updateServiceHandler;
        _changeServiceStatusHandler = changeServiceStatusHandler;
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ServiceManagement)]
    public async Task<ActionResult<ServiceResponse>> Create(
        Guid organizationId,
        CreateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _createServiceHandler.HandleAsync(
                new CreateServiceCommand(
                    organizationId,
                    request.Name,
                    request.Description,
                    request.Duration,
                    request.PriceAmount,
                    request.Currency),
                cancellationToken);

        return Created(
            $"/api/organizations/{organizationId}/services/{result.Id}",
            Map(result));
    }

    [HttpGet("{serviceId:guid}")]
    public async Task<ActionResult<ServiceResponse>> GetById(
        Guid organizationId,
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        var result =
            await _getServiceHandler.HandleAsync(
                new GetServiceQuery(
                    organizationId,
                    serviceId),
                cancellationToken);

        return Ok(Map(result));
    }

    [HttpGet]
    public async Task<ActionResult<ServiceListResponse>> List(
        Guid organizationId,
        [FromQuery] ServiceStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result =
            await _listServicesHandler.HandleAsync(
                new ListServicesQuery(
                    organizationId,
                    status,
                    page,
                    pageSize),
                cancellationToken);

        return Ok(
            new ServiceListResponse(
                result.Items.Select(Map).ToArray(),
                result.Page,
                result.PageSize,
                result.TotalCount));
    }

    [HttpPut("{serviceId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.ServiceManagement)]
    public async Task<ActionResult<ServiceResponse>> Update(
        Guid organizationId,
        Guid serviceId,
        UpdateServiceRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _updateServiceHandler.HandleAsync(
                new UpdateServiceCommand(
                    organizationId,
                    serviceId,
                    request.Name,
                    request.Description,
                    request.Duration,
                    request.PriceAmount,
                    request.Currency),
                cancellationToken);

        return Ok(Map(result));
    }

    [HttpPost("{serviceId:guid}/deactivate")]
    [Authorize(Policy = AuthorizationPolicies.ServiceManagement)]
    public Task<ActionResult<ServiceResponse>> Deactivate(
        Guid organizationId,
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        return ChangeStatus(
            organizationId,
            serviceId,
            false,
            cancellationToken);
    }

    [HttpPost("{serviceId:guid}/activate")]
    [Authorize(Policy = AuthorizationPolicies.ServiceManagement)]
    public Task<ActionResult<ServiceResponse>> Activate(
        Guid organizationId,
        Guid serviceId,
        CancellationToken cancellationToken)
    {
        return ChangeStatus(
            organizationId,
            serviceId,
            true,
            cancellationToken);
    }

    private async Task<ActionResult<ServiceResponse>> ChangeStatus(
        Guid organizationId,
        Guid serviceId,
        bool activate,
        CancellationToken cancellationToken)
    {
        var result =
            await _changeServiceStatusHandler.HandleAsync(
                new ChangeServiceStatusCommand(
                    organizationId,
                    serviceId,
                    activate),
                cancellationToken);

        return Ok(Map(result));
    }

    private static ServiceResponse Map(
        ServiceDetails service)
    {
        return new ServiceResponse(
            service.Id,
            service.OrganizationId,
            service.Name,
            service.Description,
            service.Duration,
            service.PriceAmount,
            service.Currency,
            service.Status,
            service.CreatedAtUtc,
            service.UpdatedAtUtc);
    }
}
