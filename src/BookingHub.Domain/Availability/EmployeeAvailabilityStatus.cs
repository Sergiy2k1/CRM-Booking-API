namespace BookingHub.Domain.Availability;

public enum EmployeeAvailabilityStatus
{
    Available = 1,
    OutsideWorkingHours = 2,
    TimeOff = 3,
    BookingConflict = 4
}
