using System;
using System.Linq;
using PlantAlarm.DatabaseModels;

namespace PlantAlarm.Helpers
{
    public static class Extensions
    {
        public static string GetMonogram(this Plant plant)
        {
            string result = "";

            string[] words = plant.Name.Split(' ');
            for (int i = 0; i < Math.Min(2, words.Count()); i++)
            {
                result += words[i].ToUpper()[0];
            }

            return result;
        }
    }
}
