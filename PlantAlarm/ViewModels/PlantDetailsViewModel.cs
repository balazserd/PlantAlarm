using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;
using PlantAlarm.Views;
using SkiaSharp;
using Xamarin.Forms;
using System.Threading.Tasks;
using PlantAlarm.Helpers;

namespace PlantAlarm.ViewModels
{
    public class PlantDetailsViewModel : INotifyPropertyChanged
    {
        private readonly INavigation NavigationStack = Application.Current.MainPage.Navigation;
        private readonly Page View;

        private Plant plant { get; set; }
        public Plant Plant
        {
            get => plant;
            set
            {
                plant = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ProgressPhotoViewModel> photoViewModels { get; set; }
        public ObservableCollection<ProgressPhotoViewModel> PhotoViewModels
        {
            get => photoViewModels;
            set
            {
                photoViewModels = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ExtendedPlantActivityViewModel> upcomingActivities { get; set; }
        public ObservableCollection<ExtendedPlantActivityViewModel> UpcomingActivities
        {
            get => upcomingActivities;
            set
            {
                upcomingActivities = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenPhotoCarouselCommand { get; private set; }
        public ICommand AddNewPhotoCommand { get; private set; }
        public ICommand BackCommand { get; set; }
        public ICommand ModifyPlantCommand { get; private set; }

        public PlantDetailsViewModel(Plant plant, Page view)
        {
            View = view;

            OpenPhotoCarouselCommand = new Command(async (_tappedPhoto) =>
            {
                var tappedPhoto = _tappedPhoto as PlantPhoto;
                await NavigationStack.PushAsync(new PlantDetailsPhotosPage(this.plant, tappedPhoto));
            });
            AddNewPhotoCommand = new Command(async () =>
            {
                var newPhoto = await View.GetNewPhoto();
                if (newPhoto == null) return; //User cancelled.

                var photoName = await MediaService.SavePhotoToLocalFolder(newPhoto);

                var newPlantPhoto = new PlantPhoto
                {
                    IsPrimary = false,
                    PlantFk = Plant.Id,
                    TakenAt = DateTime.Now,
                    Url = photoName
                };
                await PlantService.AddPlantPhotoAsync(newPlantPhoto); //Id gets populated here.
                newPlantPhoto.Url = MediaService.AppendLocalAppDataFolderToPhotoName(newPlantPhoto.Url); //Url modified to be absolute.

                PhotoViewModels.Insert(0, new ProgressPhotoViewModel(newPlantPhoto, OpenPhotoCarouselCommand));
                this.RefreshUpcomingActivities();
                MessagingCenter.Send(this as object, "PhotoAdded");
            });
            BackCommand = new Command(async() => await NavigationStack.PopAsync());
            ModifyPlantCommand = new Command(async () =>
            {
                await NavigationStack.PushAsync(new NewPlantPage(true, this.Plant));
            });

            this.Plant = plant;

            this.RefreshPhotos();

            MessagingCenter.Subscribe<object>(this as object, "PhotoRemoved", (viewModel) =>
            {
                this.RefreshPhotos();
            });
            MessagingCenter.Subscribe<object>(this as object, "PlantChanged", (newPlant) =>
            {
                this.Plant = plant;
            });
        }

        private void RefreshPhotos()
        {
            var photoVmList = PlantService.GetPhotosOfPlant(this.Plant)
                .Select(ph => new ProgressPhotoViewModel(ph, OpenPhotoCarouselCommand))
                .OrderByDescending(phVm => phVm.Photo.TakenAt);

            this.PhotoViewModels = new ObservableCollection<ProgressPhotoViewModel>(photoVmList);

            this.RefreshUpcomingActivities();
        }

        private void RefreshUpcomingActivities()
        {
            var activities = PlantActivityService.GetUpcomingActivitiesOfPlant(this.Plant);
            var extendedActivities = activities
                .Select(act =>
                {
                    var plantsOfActivity = PlantActivityService.GetPlantsOfActivity(act.PlantActivityItem);
                    var allPhotos = PlantService.GetAllPhotos();

                    act.PlantsInTask = plantsOfActivity
                        .Select(pl =>
                        {
                            var item = new ExtendedPlantActivityViewModel.PlantItem() { Plant = pl };
                            var photosOfPlant = allPhotos.Where(photo => photo.PlantFk == pl.Id);

                            item.Photo = photosOfPlant.FirstOrDefault(photo => photo.IsPrimary) ?? photosOfPlant.OrderByDescending(photo => photo.TakenAt).FirstOrDefault();

                            return item;
                        })
                        .ToList();

                    return act;
                })
                .ToList();

            this.UpcomingActivities = new ObservableCollection<ExtendedPlantActivityViewModel>(extendedActivities);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public class ProgressPhotoViewModel
        {
            public PlantPhoto Photo { get; set; }
            public ICommand TappedCommand { get; set; }

            public ProgressPhotoViewModel(PlantPhoto photo, ICommand tappedCommand)
            {
                this.Photo = photo;
                this.TappedCommand = new Command(() => tappedCommand.Execute(photo));
            }
        }
    }

    public class ExtendedPlantActivityViewModel : BindableObject
    {
        public PlantActivityItem PlantActivityItem { get; set; }
        public PlantTask PlantTask { get; set; }
        public List<PlantItem> PlantsInTask { get; set; }

        public class PlantItem : BindableObject
        {
            public Plant Plant { get; set; }

            private PlantPhoto photo { get; set; }
            public PlantPhoto Photo
            {
                get => photo;
                set
                {
                    photo = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasPhoto));
                    OnPropertyChanged(nameof(HasNoPhoto));
                }
            }

            public bool HasPhoto { get => Photo != null; }
            public bool HasNoPhoto { get => !HasPhoto; }

            public string Monogram { get => Plant.GetMonogram(); }
        }
    }
}
