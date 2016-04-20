using System;

namespace WebApiSeed.AxHelpers
{
    public class DateHelpers
    {
        /// <summary>
        /// Gets the date range.
        /// </summary>
        /// <param name="period">The period.</param>
        /// <returns></returns>
        public static DateRange GetDateRange(DatePeriod period)
        {
            switch (period)
            {
                case DatePeriod.Last7Days:
                    return new DateRange(DateTime.Today, -7);
                case DatePeriod.Today:
                    return new DateRange(DateTime.Today);
                case DatePeriod.Week:
                    var s = DateTime.Today.AddDays((DaySub(DateTime.Today.DayOfWeek)));
                    return new DateRange(s, s.AddDays(6));
                case DatePeriod.Month:
                    var x = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                    return new DateRange(x, new DateTime(x.Year, x.Month, DateTime.DaysInMonth(x.Year, x.Month)));
                case DatePeriod.Year:
                    return new DateRange(new DateTime(DateTime.Today.Year, 1, 1), new DateTime(DateTime.Today.Year, 12, 31));
                default:
                    return new DateRange(new DateTime(2010, 1, 1), DateTime.UtcNow);
            }
        }

        /// <summary>
        /// Find number of days to subtract from current day.
        /// </summary>
        /// <param name="dayName">Name of the day.</param>
        /// <returns></returns>
        private static int DaySub(DayOfWeek dayName)
        {
            switch (dayName)
            {
                case DayOfWeek.Sunday:
                    return 0;
                case DayOfWeek.Monday:
                    return -1;
                case DayOfWeek.Tuesday:
                    return -2;
                case DayOfWeek.Wednesday:
                    return -3;
                case DayOfWeek.Thursday:
                    return -4;
                case DayOfWeek.Friday:
                    return -5;
                case DayOfWeek.Saturday:
                    return -6;
                default:
                    return 0;
            }
        }
    }

    public class DateRange
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public DateRange(DateTime startDate, DateTime endDate)
        {
            Start = startDate;
            End = endDate.AddHours(23);
        }

        public DateRange(DateTime date)
        {
            Start = date;
            End = date.AddHours(23);
        }

        public DateRange(DateTime date, int days)
        {
            Start = date.AddDays(days);
            End = date;
        }
    }

    public enum DatePeriod
    {
        Today,
        Week,
        Month,
        Year,
        Last7Days
    }
}