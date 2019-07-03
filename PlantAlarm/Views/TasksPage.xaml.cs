using System;
using System.Collections.Generic;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views
{
    public partial class TasksPage : ContentPage
    {
        public TasksPage()
        {
            InitializeComponent();

            this.BindingContext = new TasksViewModel(this.Navigation);
        }
    }
}
