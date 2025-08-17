using System;

public static class DateTimeExtensions
{
    //How to use: SavedDate.ToRelativeTime();
    public static string ToRelativeTime(this DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;

        if (timeSpan <= TimeSpan.FromSeconds(60))
            return $"{timeSpan.Seconds} seconds ago";

        if (timeSpan <= TimeSpan.FromMinutes(60))
            return timeSpan.Minutes > 1 ? $"{timeSpan.Minutes} minutes ago" : "a minute ago";

        if (timeSpan <= TimeSpan.FromHours(24))
            return timeSpan.Hours > 1 ? $"{timeSpan.Hours} hours ago" : "an hour ago";

        if (timeSpan <= TimeSpan.FromDays(30))
            return timeSpan.Days > 1 ? $"{timeSpan.Days} days ago" : "yesterday";

        if (timeSpan <= TimeSpan.FromDays(365))
        {
            var months = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 30));
            return months > 1 ? $"{months} months ago" : "a month ago";
        }

        var years = Convert.ToInt32(Math.Floor((double)timeSpan.Days / 365));
        return years > 1 ? $"{years} years ago" : "a year ago";
    }

    /// <summary>
    /// Returns the first day of the week (default: Monday).
    /// Usage: 
    /// <code>DateTime weekStart = DateTime.Now.StartOfWeek();</code>
    /// <code>DateTime usWeekStart = DateTime.Now.StartOfWeek(DayOfWeek.Sunday);</code>
    /// </summary>
    public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Monday)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }

    /// <summary>
    /// Calculates age in years from a birth date (handles leap years).
    /// Usage: 
    /// <code>var age = birthDate.CalculateAge();</code>
    /// </summary>
    public static int CalculateAge(this DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age)) age--;
        return age;
    }

    /// <summary>
    /// Checks if the date falls on a weekend (Saturday/Sunday).
    /// Usage: 
    /// <code>bool isWeekend = DateTime.Now.IsWeekend();</code>
    /// </summary>
    public static bool IsWeekend(this DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    /// <summary>
    /// Checks if the date is in the future relative to current time.
    /// Usage: 
    /// <code>bool isUpcoming = eventDate.IsFuture();</code>
    /// </summary>
    public static bool IsFuture(this DateTime date)
    {
        return date > DateTime.Now;
    }

    public static bool IsPast(this DateTime date)
    {
        return date < DateTime.Now;
    }

    public static string ToDateAndDay(this DateTime date, bool includeTime = false)
    {
        return includeTime
            ? date.ToString("dddd, dd MMM yyyy HH:mm")
            : date.ToString("dddd, dd MMM yyyy");
    }

    public static string ToFullDate(this DateTime date, bool includeTime = false)
    {
        return includeTime
            ? date.ToString("dd MMMM yyyy HH:mm")
            : date.ToString("dd MMMM yyyy");
    }

    public static string ToDate(this DateTime date, bool includeTime = false)
    {
        return includeTime
            ? date.ToString("dd MMM yyyy HH:mm")
            : date.ToString("dd MMM yyyy");
    }

}