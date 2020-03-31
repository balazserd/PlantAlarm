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

        void ExecuteSwipeLeftCommand(object sender, SwipedEventArgs e)
        {
            vm.SwipeCommand.Execute(true);
        }

        void ExecuteSwipeRightCommand(object sender, SwipedEventArgs e)
        {
            vm.SwipeCommand.Execute(false);
        }
    }
}
