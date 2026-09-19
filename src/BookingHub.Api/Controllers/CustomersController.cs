using BookingHub.Api.Authorization;
using BookingHub.Api.Contracts.Customers;
using BookingHub.Application.Customers.ArchiveCustomer;
using BookingHub.Application.Customers.Common;
using BookingHub.Application.Customers.CreateCustomer;
using BookingHub.Application.Customers.GetCustomer;
using BookingHub.Application.Customers.ListCustomers;
using BookingHub.Application.Customers.RestoreCustomer;
using BookingHub.Application.Customers.UpdateCustomer;
using BookingHub.Domain.Customers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookingHub.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.OrganizationAccess)]
[Route("api/organizations/{organizationId:guid}/customers")]
public sealed class CustomersController : ControllerBase
{
    private readonly CreateCustomerHandler _createCustomerHandler;
    private readonly GetCustomerHandler _getCustomerHandler;
    private readonly ListCustomersHandler _listCustomersHandler;
    private readonly UpdateCustomerHandler _updateCustomerHandler;
    private readonly ArchiveCustomerHandler _archiveCustomerHandler;
    private readonly RestoreCustomerHandler _restoreCustomerHandler;

    public CustomersController(
        CreateCustomerHandler createCustomerHandler,
        GetCustomerHandler getCustomerHandler,
        ListCustomersHandler listCustomersHandler,
        UpdateCustomerHandler updateCustomerHandler,
        ArchiveCustomerHandler archiveCustomerHandler,
        RestoreCustomerHandler restoreCustomerHandler)
    {
        _createCustomerHandler = createCustomerHandler;
        _getCustomerHandler = getCustomerHandler;
        _listCustomersHandler = listCustomersHandler;
        _updateCustomerHandler = updateCustomerHandler;
        _archiveCustomerHandler = archiveCustomerHandler;
        _restoreCustomerHandler = restoreCustomerHandler;
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.CustomerManagement)]
    public async Task<ActionResult<CustomerResponse>> Create(
        Guid organizationId,
        CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer =
            await _createCustomerHandler.HandleAsync(
                new CreateCustomerCommand(
                    organizationId,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Phone),
                cancellationToken);

        return Created(
            $"/api/organizations/{organizationId}/customers/{customer.Id}",
            Map(customer));
    }

    [HttpGet("{customerId:guid}")]
    public async Task<ActionResult<CustomerResponse>> GetById(
        Guid organizationId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var customer =
            await _getCustomerHandler.HandleAsync(
                new GetCustomerQuery(
                    organizationId,
                    customerId),
                cancellationToken);

        return Ok(Map(customer));
    }

    [HttpGet]
    public async Task<ActionResult<CustomerListResponse>> List(
        Guid organizationId,
        [FromQuery] CustomerStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result =
            await _listCustomersHandler.HandleAsync(
                new ListCustomersQuery(
                    organizationId,
                    status,
                    page,
                    pageSize),
                cancellationToken);

        return Ok(
            new CustomerListResponse(
                result.Items
                    .Select(Map)
                    .ToArray(),
                result.Page,
                result.PageSize,
                result.TotalCount));
    }

    [HttpPut("{customerId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.CustomerManagement)]
    public async Task<ActionResult<CustomerResponse>> Update(
        Guid organizationId,
        Guid customerId,
        UpdateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var customer =
            await _updateCustomerHandler.HandleAsync(
                new UpdateCustomerCommand(
                    organizationId,
                    customerId,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.Phone),
                cancellationToken);

        return Ok(Map(customer));
    }

    [HttpPost("{customerId:guid}/archive")]
    [Authorize(Policy = AuthorizationPolicies.CustomerManagement)]
    public async Task<ActionResult<CustomerResponse>> Archive(
        Guid organizationId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var customer =
            await _archiveCustomerHandler.HandleAsync(
                new ArchiveCustomerCommand(
                    organizationId,
                    customerId),
                cancellationToken);

        return Ok(Map(customer));
    }

    [HttpPost("{customerId:guid}/restore")]
    [Authorize(Policy = AuthorizationPolicies.CustomerManagement)]
    public async Task<ActionResult<CustomerResponse>> Restore(
        Guid organizationId,
        Guid customerId,
        CancellationToken cancellationToken)
    {
        var customer =
            await _restoreCustomerHandler.HandleAsync(
                new RestoreCustomerCommand(
                    organizationId,
                    customerId),
                cancellationToken);

        return Ok(Map(customer));
    }

    private static CustomerResponse Map(
        CustomerDetails customer)
    {
        return new CustomerResponse(
            customer.Id,
            customer.OrganizationId,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.Phone,
            customer.Status,
            customer.CreatedAtUtc,
            customer.UpdatedAtUtc,
            customer.ArchivedAtUtc);
    }
}
