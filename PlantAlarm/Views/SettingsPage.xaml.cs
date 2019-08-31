using System;
using System.Collections.Generic;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            this.BindingContext = new SettingsViewModel();
        }
    }
}
