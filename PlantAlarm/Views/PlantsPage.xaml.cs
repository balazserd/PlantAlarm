using System;
using System.Collections.Generic;
using System.Threading;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class PlantsPage : ContentPage
    {
        public PlantsPage()
        {
            InitializeComponent();
            this.BindingContext = new PlantsViewModel();
        }
    }
}
