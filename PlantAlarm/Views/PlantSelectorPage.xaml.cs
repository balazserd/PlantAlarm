using System;
using System.Collections.Generic;
using System.Linq;
using PlantAlarm.CustomControls;
using PlantAlarm.DatabaseModels;
using PlantAlarm.ViewModels;
using PlantAlarm.Views.RootPages;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class PlantSelectorPage : SafeAreaRespectingPage
    {
        public PlantSelectorPage(List<Plant> preSelectedPlants)
        {
            InitializeComponent();

            this.BindingContext = new PlantSelectorViewModel(preSelectedPlants);
        }
    }
}
