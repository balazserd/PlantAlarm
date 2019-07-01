using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PlantAlarm.ViewModels
{
    public class NewPlantViewModel : INotifyPropertyChanged
    {
        public NewPlantViewModel()
        {
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
