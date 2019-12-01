using System;
using System.Collections.Generic;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views.RootPages
{
    public partial class TasksPage : SafeAreaRespectingPage
    {
        public TasksPage()
        {
            InitializeComponent();

            this.BindingContext = new TasksViewModel(this.Navigation, this);
        }
    }
}
