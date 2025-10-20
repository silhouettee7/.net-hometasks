using Quartz;

namespace RESTAuth.Application.Configuration.Calendars;

public class SkipSecondMinuteCalendar(DateTimeOffset jobStartTime): ICalendar
{
    public string? Description { get; set; } =  "Calendar that excludes the second minute of job execution";
    public ICalendar? CalendarBase { get; set; }
    
    public bool IsTimeIncluded(DateTimeOffset timeUtc)
    {
        return !IsInSecondMinute(timeUtc);
    }

    public DateTimeOffset GetNextIncludedTimeUtc(DateTimeOffset timeUtc)
    {

        return IsInSecondMinute(timeUtc) ? jobStartTime.AddMinutes(2) : timeUtc;
    }

    public ICalendar Clone()
    {
        return new SkipSecondMinuteCalendar(jobStartTime);
    }

    private bool IsInSecondMinute(DateTimeOffset timeUtc)
    {
        var currentTime = timeUtc.LocalDateTime;
        var startTime = jobStartTime.LocalDateTime;
        var currentMinute = (currentTime - startTime).TotalMinutes;
        return currentMinute is > 1.0 and < 2.0;
    }
}