using System;
using System.Collections.Generic;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views.RootPages
{
    public partial class PlantsPage : SafeAreaRespectingPage
    {
        public PlantsPage()
        {
            InitializeComponent();
            this.BindingContext = new PlantsViewModel();
        }
    }
}
