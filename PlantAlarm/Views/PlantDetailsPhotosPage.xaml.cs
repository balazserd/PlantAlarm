using System;
using System.Collections.Generic;
using PlantAlarm.DatabaseModels;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class PlantDetailsPhotosPage : ContentPage
    {
        private readonly PlantDetailsPhotosViewModel vm;
        public PlantDetailsPhotosPage(Plant plant, PlantPhoto selectedPhoto)
        {
            InitializeComponent();

            this.BindingContext = new PlantDetailsPhotosViewModel(plant, selectedPhoto);
            vm = this.BindingContext as PlantDetailsPhotosViewModel;
        }

        public void ScrollToCurrentItem(object sender, CurrentItemChangedEventArgs args)
        {
            var carouselView = sender as CarouselView;
            carouselView.ScrollTo(args.CurrentItem); //TODO this does not seem to scroll there for now! Let's give it some time
        }
    }
}
