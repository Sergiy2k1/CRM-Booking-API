using BookingHub.Application.Abstractions;
using BookingHub.Application.Abstractions.Persistence;
using BookingHub.Application.Notifications.ListNotifications;
using BookingHub.Application.Notifications.MarkNotificationRead;
using BookingHub.Domain.Notifications;
using NSubstitute;
using Xunit;

namespace BookingHub.Application.UnitTests.Notifications;

public sealed class NotificationHandlersTests
{
    [Fact]
    public async Task ListShouldReturnOnlyCurrentUserNotifications()
    {
        var context = CreateContext();
        var repository =
            Substitute.For<INotificationRepository>();

        repository
            .ListAsync(
                context.OrganizationId,
                context.UserId,
                false,
                0,
                20,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<IReadOnlyCollection<Notification>>(
                    [context.Notification]));

        repository
            .CountAsync(
                context.OrganizationId,
                context.UserId,
                false,
                Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(1));

        var handler =
            new ListNotificationsHandler(
                repository);

        var result =
            await handler.HandleAsync(
                new ListNotificationsQuery(
                    context.OrganizationId,
                    context.UserId,
                    false),
                TestContext.Current.CancellationToken);

        Assert.Single(result.Items);
        Assert.Equal(context.NotificationId, result.Items.Single().Id);
        Assert.False(result.Items.Single().IsRead);
    }

    [Fact]
    public async Task MarkReadShouldSetReadTimestamp()
    {
        var context = CreateContext();
        var repository =
            Substitute.For<INotificationRepository>();

        var clock =
            Substitute.For<IClock>();

        var unitOfWork =
            Substitute.For<IUnitOfWork>();

        repository
            .GetTrackedAsync(
                context.OrganizationId,
                context.UserId,
                context.NotificationId,
                Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult<Notification?>(
                    context.Notification));

        clock.UtcNow.Returns(
            context.ReadAtUtc);

        unitOfWork
            .SaveChangesAsync(
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handler =
            new MarkNotificationReadHandler(
                repository,
                clock,
                unitOfWork);

        var result =
            await handler.HandleAsync(
                new MarkNotificationReadCommand(
                    context.OrganizationId,
                    context.UserId,
                    context.NotificationId),
                TestContext.Current.CancellationToken);

        Assert.True(result.IsRead);
        Assert.Equal(context.ReadAtUtc, result.ReadAtUtc);
    }

    private static NotificationTestContext CreateContext()
    {
        var organizationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var notificationId = Guid.NewGuid();

        var createdAtUtc =
            new DateTimeOffset(
                2026,
                9,
                19,
                21,
                30,
                0,
                TimeSpan.Zero);

        var notification =
            Notification.Create(
                notificationId,
                organizationId,
                userId,
                Guid.NewGuid(),
                "booking.created",
                "Booking created",
                "A booking was created.",
                Guid.NewGuid(),
                createdAtUtc);

        return new NotificationTestContext(
            organizationId,
            userId,
            notificationId,
            createdAtUtc.AddMinutes(5),
            notification);
    }

    private sealed record NotificationTestContext(
        Guid OrganizationId,
        Guid UserId,
        Guid NotificationId,
        DateTimeOffset ReadAtUtc,
        Notification Notification);
}
