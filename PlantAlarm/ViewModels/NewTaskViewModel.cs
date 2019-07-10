using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public class NewTaskViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Plant> PlantList { get; private set; }
        public ICommand AddPlantsCommand { get; private set; }

        public NewTaskViewModel()
        {
            PlantList = new ObservableCollection<Plant>();
            AddPlantsCommand = new Command(() =>
            {

            });
        }



        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
