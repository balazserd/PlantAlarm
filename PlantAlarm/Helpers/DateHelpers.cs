using System;
using System.Collections.Generic;
using PlantAlarm.DatabaseModels;

namespace PlantAlarm.Helpers
{
    public static class DateHelpers
    {
        public static string ExpressAsNextTaskOccurrenceText(this DateTime? dateTime)
        {
            if (dateTime == null) return "No upcoming activity";

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

        public static List<string> CreateRecurrenceTextArray(this PlantTask plantTask)
        {
            var result = new List<string>();

            if (plantTask.IsRepeating)
            {
                if (plantTask.OnMonday) result.Add("Every Monday");
                if (plantTask.OnTuesday) result.Add("Every Tuesday");
                if (plantTask.OnWednesday) result.Add("Every Wednesday");
                if (plantTask.OnThursday) result.Add("Every Thursday");
                if (plantTask.OnFriday) result.Add("Every Friday");
                if (plantTask.OnSaturday) result.Add("Every Saturday");
                if (plantTask.OnSunday) result.Add("Every Sunday");

                if (plantTask.EveryXDays > 0)
                {
                    if (plantTask.EveryXDays == 1) result.Add("Everyday");
                    else result.Add($"Every {plantTask.EveryXDays} days");
                }

                if (plantTask.EveryXMonths > 0)
                {
                    if (plantTask.EveryXMonths == 1) result.Add("Every month");
                    else result.Add($"Every {plantTask.EveryXMonths} months");
                }
            }
            else
            {
                result.Add("No recurrence");
            }

            return result;
        }
    }
}
