using System;
using System.Collections.Generic;
using PlantAlarm.DatabaseModels;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class TaskDetailsPage : ContentPage
    {
        private readonly TaskDetailsViewModel vm;

        public TaskDetailsPage(PlantTask plantTask)
        {
            InitializeComponent();

            vm = new TaskDetailsViewModel(plantTask);
            this.BindingContext = vm;
        }
    }
}
