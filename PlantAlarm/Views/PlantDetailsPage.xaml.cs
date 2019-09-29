using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            this.BindingContext = new PlantDetailsViewModel(plant, this);
            vm = this.BindingContext as PlantDetailsViewModel;
        }
    }
}
