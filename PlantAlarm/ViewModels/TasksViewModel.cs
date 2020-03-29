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
        private readonly Page View;

        public ICommand ShowNewTaskPageCommand { get; private set; }
        public ICommand ShowTaskDetailsPageCommandBase { get; private set; }

        private ObservableCollection<PlantTaskItem> plantTasks { get; set; }
        public ObservableCollection<PlantTaskItem> PlantTasks
        {
            get => new ObservableCollection<PlantTaskItem>(plantTasks.OrderBy(pti => pti.NextIncompleteOccurrence));
            set
            {
                plantTasks = value;
                OnPropertyChanged();
            }
        }

        public TasksViewModel(INavigation navigation, Page viewForViewModel)
        {
            View = viewForViewModel;
            Navigation = navigation;

            ShowTaskDetailsPageCommandBase = new Command(async (_task) =>
            {
                var task = _task as PlantTask;
                await Navigation.PushAsync(new NewTaskPage(true, task));
            });

            ShowNewTaskPageCommand = new Command(async () =>
            {
                await Navigation.PushAsync(new NewTaskPage(false));
            });

            this.RefreshTasks();

            MessagingCenter.Subscribe<object>(this as object, "TaskListChanged", (viewModel) =>
            {
                this.RefreshTasks();
            });
        }

        private void RefreshTasks()
        {
            var _plantTasks = new List<PlantTaskItem>();
            var tasks = PlantActivityService.GetAllTasks();
            foreach (var task in tasks)
            {
                var _plantTask = new PlantTaskItem(task);
                _plantTask.ShowTaskDetailsPageCommand = new Command(() => this.ShowTaskDetailsPageCommandBase.Execute(task));
                _plantTasks.Add(_plantTask);
            }

            PlantTasks = new ObservableCollection<PlantTaskItem>(_plantTasks);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    #region mini-viewmodels

    //ONLY INTERNAL USE
    public class PlantTaskItem
    {
        public ObservableCollection<PlantItemOfTask> Plants { get; set; }
        public PlantTask Task { get; set; }
        public DateTime? NextIncompleteOccurrence { get; set; }

        public string NextIncompleteOccurrenceString => NextIncompleteOccurrence.ExpressAsNextTaskOccurrenceText();
        public Color NextOccurrenceColor => NextIncompleteOccurrence?.Date == DateTime.Today ? Color.FromHex("#EA4665") : Color.FromHex("6F5F15");
        public FontAttributes NextOccurrenceFontStyle => NextIncompleteOccurrence?.Date == DateTime.Today ?
            FontAttributes.Bold | FontAttributes.Italic :
            FontAttributes.Italic;

        public ICommand ShowTaskDetailsPageCommand { get; set; }

        public PlantTaskItem(PlantTask pt)
        {
            this.Task = pt;

            var plants = PlantActivityService.GetPlantsOfTask(pt);
            var photos = PlantService.GetPrimaryPhotosOfPlants(plants);

            var plantItems = new ObservableCollection<PlantItemOfTask>();
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
