using System;
using System.Collections.Generic;

namespace PlantAlarm.Models
{
    public class TaskBase
    {
        public uint Id { get; set; }
        public Plant Plant { get; set; }
        public string Description { get; set; }
        public List<AccessoryUsage> AccessoryUsages { get; set; }
    }
}
