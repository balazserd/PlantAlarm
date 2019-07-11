using System;
using System.Collections.Generic;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class NewPlantPage : ContentPage
    {
        public NewPlantPage()
        {
            InitializeComponent();

            this.BindingContext = new NewPlantViewModel(this.Navigation, this);
        }
    }
}
