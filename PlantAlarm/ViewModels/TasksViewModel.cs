using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PlantAlarm.Views;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public class TasksViewModel : INotifyPropertyChanged
    {
        public INavigation Navigation { get; private set; }

        public ICommand ShowNewTaskPageCommand { get; private set; }

        public TasksViewModel(INavigation navigation)
        {
            Navigation = navigation;
            ShowNewTaskPageCommand = new Command(async () =>
            {
                await Navigation.PushAsync(new NewTaskPage());
            });
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
