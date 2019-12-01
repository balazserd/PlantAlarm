using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public class TaskDetailsViewModel : INotifyPropertyChanged
    {
        private readonly INavigation NavigationStack = Application.Current.MainPage.Navigation;

        private PlantTask previousStateOfTask { get; set; }

        private PlantTask plantTask { get; set; }
        public PlantTask PlantTask
        {
            get => plantTask;
            set
            {
                plantTask = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(NextOccurrenceDate));
            }
        }

        private ObservableCollection<PlantItem> plants { get; set; }
        public ObservableCollection<PlantItem> Plants
        {
            get => plants;
            set
            {
                plants = value;
                OnPropertyChanged();
            }
        }

        private DateTime? nextOccurrenceDate { get; set; }
        public DateTime? NextOccurrenceDate
        {
            get => nextOccurrenceDate;
            set
            {
                nextOccurrenceDate = value;
                PlantTask.FirstOccurrenceDate = nextOccurrenceDate ?? throw new ArgumentNullException();
                OnPropertyChanged();
            }
        }

        private bool isEditing { get; set; }
        public bool IsEditing
        {
            get => isEditing;
            set
            {
                isEditing = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotEditing));
            }
        }
        public bool IsNotEditing => !IsEditing;

        public ICommand BackCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand EditCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }

        public TaskDetailsViewModel(PlantTask plantTask)
        {
            this.PlantTask = plantTask;

            BackCommand = new Command(async () =>
            {
                await NavigationStack.PopAsync();
            });
            EditCommand = new Command(() =>
            {
                this.IsEditing = true;
                this.previousStateOfTask = this.PlantTask;
            });
            CancelCommand = new Command(() =>
            {
                this.IsEditing = false;
                ReloadPlantTask();
            });
            SaveCommand = new Command(async () =>
            {
                this.IsEditing = false;
                await PlantActivityService.UpdateTask(this.PlantTask);
                ReloadPlantTask();
            });

            var plantsOfTask = PlantActivityService.GetPlantsOfTask(plantTask)
                .Select(plant => new PlantItem(plant));

            //Careful, do not call public property setter here, or you modify the base plantTask item!
            //This is just a setup, we should not do that here!
            this.nextOccurrenceDate = PlantActivityService.GetNextIncompleteActivityOfTask(plantTask)?.Time;

            this.Plants = new ObservableCollection<PlantItem>(plantsOfTask);
        }

        private void ReloadPlantTask()
        {
            var savedStateOfTask = PlantActivityService.GetAllTasks()
                    .Single(t => t.Id == this.PlantTask.Id);
            this.PlantTask = savedStateOfTask;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public class PlantItem : BindableObject
        {
            public Plant Plant { get; set; }
            public PlantPhoto PrimaryPhoto { get; set; }

            public PlantItem(Plant p)
            {
                this.Plant = p;
                this.PrimaryPhoto = PlantService.GetPrimaryPhotoOfPlant(p);
            }
        }
    }
}
