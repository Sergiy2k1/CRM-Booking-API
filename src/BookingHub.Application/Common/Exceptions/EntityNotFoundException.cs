namespace BookingHub.Application.Common.Exceptions;

public sealed class EntityNotFoundException : Exception
{
    public EntityNotFoundException(
        string entityName,
        Guid entityId)
        : base($"{entityName} with id '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }

    public string EntityName { get; }

    public Guid EntityId { get; }
}
