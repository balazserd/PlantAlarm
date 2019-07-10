using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Views;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public class PlantsViewModel : INotifyPropertyChanged
    {
        private INavigation Navigation { get; set; }

        public ObservableCollection<Plant> PlantList { get; private set; }

        public ICommand ShowNewPlantPageCommand { get; private set; }

        public PlantsViewModel(INavigation navigation)
        {
            Navigation = navigation;

            ShowNewPlantPageCommand = new Command(async () =>
            {
                await Navigation.PushAsync(new NewPlantPage());
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
