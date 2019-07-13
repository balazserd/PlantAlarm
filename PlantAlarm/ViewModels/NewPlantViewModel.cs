using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PlantAlarm.DatabaseModels;
using PlantAlarm.Services;
using PlantAlarm.Views;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Forms;

namespace PlantAlarm.ViewModels
{
    public class NewPlantViewModel : INotifyPropertyChanged
    {
        private INavigation Navigation { get; set; }
        private Page View { get; set; }

        public string SelectedCategoriesMessage
        {
            get => $"{Categories.Count} categories selected";
        }

        public Plant PlantToAdd { get; set; }
        public List<PlantCategory> Categories { get; private set; }

        public ICommand ShowCategorySelectorPageCommand { get; private set; }
        public ICommand AddPhotoCommand { get; set; }

        public ObservableCollection<PlantPhoto> Photos { get; private set; }

        public NewPlantViewModel(INavigation navigation, Page viewForViewModel)
        {
            View = viewForViewModel;
            Navigation = navigation;

            ShowCategorySelectorPageCommand = new Command(async() =>
            {
               await Navigation.PushAsync(new CategorySelectorPage());
            });
            AddPhotoCommand = new Command(async () =>
            {
                if (PlantToAdd == null)
                {
                    //Add the plant instantly, so that we will have an id for the plant photos.
                    PlantToAdd = new Plant { Name = "Placeholder Name" };
                    int plantId = await PlantService.AddPlantAsync(PlantToAdd);
                    PlantToAdd.Id = plantId;
                }

                //Create the actions for the add photo dialog.
                var actions = new List<string>();

                if (CrossMedia.Current.IsCameraAvailable && CrossMedia.Current.IsTakePhotoSupported) actions.Add("Take photo");
                if (CrossMedia.Current.IsPickPhotoSupported) actions.Add("Pick from device");

                string actionToTake = await View.DisplayActionSheet("Select an option", "Back", null, actions.ToArray());

                MediaFile image;
                switch (actionToTake)
                {
                    case "Take photo":
                        {
                            image = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                            {
                                PhotoSize = PhotoSize.Medium,
                                CompressionQuality = 75,
                                SaveToAlbum = true
                            });
                            break;
                        }
                    case "Pick from device":
                        {
                            image = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                            {
                                PhotoSize = PhotoSize.Medium,
                                CompressionQuality = 75,
                            });
                            break;
                        }
                    case "Back": return;
                    default: throw new InvalidOperationException("The operation received to perform is invalid.");
                }

                if (image == null) return;

                //Save the created image to the local folder. Must be done, or it can be removed by the user from their public picture folder.
                var guidString = Guid.NewGuid().ToString().Replace("-", "");
                var fullUrl = Path.Combine(PlantService.LocalPhotoFolder, guidString);

                await image.GetStreamWithImageRotatedForExternalStorage().CopyToAsync(File.Create(fullUrl));

                var photo = new PlantPhoto
                {
                    PlantFk = PlantToAdd.Id,
                    IsPrimary = false,
                    TakenAt = DateTime.Now,
                    Url = fullUrl
                };

                //Add the created object to the collection of photos.
                Photos.Add(photo);
            });

            Categories = new List<PlantCategory>();
            Photos = new ObservableCollection<PlantPhoto>();
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
