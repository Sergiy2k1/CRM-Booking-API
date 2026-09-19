using BookingHub.Domain.Customers;
using Xunit;

namespace BookingHub.Domain.UnitTests.Customers;

public sealed class CustomerNoteTests
{
    [Fact]
    public void CreateWithValidDataShouldCreateCustomerNote()
    {
        var organizationId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var authorUserId = Guid.NewGuid();

        var createdAtUtc =
            new DateTimeOffset(
                2026,
                9,
                19,
                10,
                0,
                0,
                TimeSpan.Zero);

        var note = CustomerNote.Create(
            Guid.NewGuid(),
            organizationId,
            customerId,
            authorUserId,
            "Important customer note",
            createdAtUtc);

        Assert.Equal(organizationId, note.OrganizationId);
        Assert.Equal(customerId, note.CustomerId);
        Assert.Equal(authorUserId, note.AuthorUserId);
        Assert.Equal("Important customer note", note.Content);
        Assert.Equal(createdAtUtc, note.CreatedAtUtc);
        Assert.Equal(createdAtUtc, note.UpdatedAtUtc);
    }

    [Fact]
    public void CreateWithWhitespaceContentShouldThrowArgumentException()
    {
        var action = () => CustomerNote.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "   ",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void CreateWithEmptyCustomerIdShouldThrowArgumentException()
    {
        var action = () => CustomerNote.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.Empty,
            Guid.NewGuid(),
            "Important customer note",
            DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void UpdateContentShouldChangeContent()
    {
        var note = CreateNote();
        var updatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(10);

        note.UpdateContent(
            "Updated note",
            updatedAtUtc);

        Assert.Equal("Updated note", note.Content);
        Assert.Equal(
            updatedAtUtc.ToUniversalTime(),
            note.UpdatedAtUtc);
    }

    private static CustomerNote CreateNote()
    {
        return CustomerNote.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Important customer note",
            DateTimeOffset.UtcNow);
    }
}
