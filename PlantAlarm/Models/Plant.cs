using System;
using System.Collections.Generic;
using Plugin.Media.Abstractions;

namespace PlantAlarm.Models
{
    public class Plant
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<MediaFile> Pictures { get; set; }
    }
}
