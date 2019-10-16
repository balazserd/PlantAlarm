using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Helpers;
using PlantAlarm.Services;
using PlantAlarm.Views;
using SQLite;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public class TasksViewModel : INotifyPropertyChanged
    {
        private readonly INavigation Navigation = Application.Current.MainPage.Navigation;

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

    #region mini-viewmodels

    //ONLY INTERNAL USE
    public class TaskItem
    {
        public List<PlantItemOfTask> Plants { get; set; }
        public PlantTask Task { get; set; }
        public DateTime? NextIncompleteOccurrence { get; set; }

        public string NextIncompleteOccurrenceString => NextIncompleteOccurrence.ExpressAsNextTaskOccurrenceText();

        public TaskItem(PlantTask pt)
        {
            this.Task = pt;

            var plants = PlantActivityService.GetPlantsOfTask(pt);
            var photos = PlantService.GetPrimaryPhotosOfPlants(plants);

            var plantItems = new List<PlantItemOfTask>();
            foreach (var plant in plants)
            {
                var plantItem = new PlantItemOfTask();
                plantItem.Plant = plant;
                plantItem.PrimaryPhoto = photos.FirstOrDefault(ph => ph.PlantFk == plant.Id);

                plantItems.Add(plantItem);
            }
            this.Plants = plantItems;

            var nextActivity = PlantActivityService.GetNextIncompleteActivityOfTask(pt);
            this.NextIncompleteOccurrence = nextActivity?.Time;
        }
    }

    public class PlantItemOfTask
    {
        public PlantPhoto PrimaryPhoto { get; set; }
        public Plant Plant { get; set; }
    }

    #endregion mini-viewmodels
}
