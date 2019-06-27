using System;
using SQLite;

namespace PlantAlarm.DatabaseModels
{
    public class Plant
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PlantPhoto
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        [Indexed]
        public int PlantId { get; set; }
        [MaxLength(255)]
        public string Url { get; set; }
        public DateTime TakenAt { get; set; }
    }

    public class PlantTask
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        [Indexed]
        public int PlantId { get; set; }
        [MaxLength(511)]
        public string Description { get; set; }
        public bool IsRepeating { get; set; }
        public DateTime FirstOccurrenceDate { get; set; }
        public bool? OnMonday { get; set; }
        public bool? OnTuesday { get; set; }
        public bool? OnWednesday { get; set; }
        public bool? OnThursday { get; set; }
        public bool? OnFriday { get; set; }
        public bool? OnSaturday { get; set; }
        public bool? OnSunday { get; set; }
        public byte? EveryXDays { get; set; }
        public byte? EveryXMonths { get; set; }
        public byte? OnDayOfMonth { get; set; }
    }

    public class PlantActivityItem
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        [Indexed]
        public int PlantTaskId { get; set; }
        public DateTime Time { get; set; }
        public bool IsCompleted { get; set; }
    }
}
