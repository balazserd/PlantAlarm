using System;

namespace PlantAlarm.Helpers
{
    public static class DateHelpers
    {
        public static string ExpressAsNextTaskOccurrenceText(this DateTime? dateTime)
        {
            if (dateTime == null) return string.Empty;

            string timePart;

            if (dateTime?.Date == DateTime.Today)
            {
                timePart = "Today";
            }
            else if (dateTime?.Date == DateTime.Today.AddDays(1))
            {
                timePart = "Tomorrow";
            }
            else {
                timePart = dateTime?.ToString("MMM dd");
            }

            return $"Next up: {timePart}";
        }
    }
}
