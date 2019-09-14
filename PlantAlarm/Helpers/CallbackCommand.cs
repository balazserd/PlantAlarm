using System;
using System.Windows.Input;

namespace PlantAlarm.Helpers
{
    public static class CommandExtension
    {
        public static void ExecuteWithCallback<T>(this T command, Action callback, object objectToPass) where T : ICommand
        {
            command.Execute(objectToPass);
            callback();
        }
    }
}
