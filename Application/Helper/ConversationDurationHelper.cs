namespace Application.Helper;

public static class ConversationDurationHelper
{
    public static string Format(DateTime startedAt, DateTime? completedAt)
    {
        var duration = (completedAt ?? DateTime.UtcNow) - startedAt;
        if (duration.TotalMinutes < 1)
            return "<1m";

        var hours = (int)duration.TotalHours;
        var minutes = duration.Minutes;

        if (hours == 0)
            return $"{minutes}m";

        if (minutes == 0)
            return $"{hours}h";

        return $"{hours}h {minutes}m";
    }
}
