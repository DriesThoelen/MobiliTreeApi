using System;

namespace MobiliTreeApi.Helper
{
    public static class DateHelper
    {
        public static bool IsWeekend(DateOnly date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsBetweenTimeSlot(DateTime startDate, DateTime endDate, DateTime startTimeSlotDateTime, DateTime endTimeSlotDateTime)
        {
            return (startDate < endTimeSlotDateTime && startTimeSlotDateTime < endDate);
        }
    }
}
