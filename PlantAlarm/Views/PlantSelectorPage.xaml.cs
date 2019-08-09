using System;
using System.Collections.Generic;
using System.Linq;
using PlantAlarm.CustomControls;
using PlantAlarm.DatabaseModels;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class PlantSelectorPage : ContentPage
    {
        public PlantSelectorPage(List<Plant> preSelectedPlants)
        {
            InitializeComponent();

            this.BindingContext = new PlantSelectorViewModel(preSelectedPlants);
        }
    }
}
