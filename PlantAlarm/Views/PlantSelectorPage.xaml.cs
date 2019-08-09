using System;
using System.Collections.Generic;
using System.Linq;
using PlantAlarm.CustomControls;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class PlantSelectorPage : ContentPage
    {
        public PlantSelectorPage()
        {
            InitializeComponent();

            this.BindingContext = new PlantSelectorViewModel();
        }
    }
}
