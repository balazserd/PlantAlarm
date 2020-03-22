using System;
using System.Collections.Generic;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views.RootPages
{
    public partial class TodoPage : SafeAreaRespectingPage
    {
        private readonly TodayViewModel vm;

        public TodoPage()
        {
            InitializeComponent();

            BindingContext = new TodayViewModel(this, this.DaysListView);
            vm = BindingContext as TodayViewModel;

            vm.BackToTodayCommand.Execute(null);
        }
    }
}
