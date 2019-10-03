using System;
using System.Collections.Generic;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views.RootPages
{
    public partial class SettingsPage : SafeAreaRespectingPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            this.BindingContext = new SettingsViewModel(this);
        }
    }
}