using System;
using System.Collections.Generic;
using PlantAlarm.DatabaseModels;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class PlantDetailsPage : ContentPage
    {
        private PlantDetailsViewModel vm;

        public PlantDetailsPage(Plant plant)
        {
            InitializeComponent();
            vm = new PlantDetailsViewModel(plant);
        }
    }
}
