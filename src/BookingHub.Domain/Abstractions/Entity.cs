namespace BookingHub.Domain.Abstractions;

public abstract class Entity
{
    protected Entity(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Entity id cannot be empty.",
                nameof(id));
        }

        Id = id;
    }

    public Guid Id { get; private set; }
}