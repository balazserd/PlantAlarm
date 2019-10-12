using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using PlantAlarm.ViewModels;
using Xamarin.Forms;

namespace PlantAlarm.Views.RootPages
{
    public partial class PlantsPage : SafeAreaRespectingPage
    {
        private readonly PlantsViewModel vm;

        public PlantsPage()
        {
            InitializeComponent();
            this.BindingContext = new PlantsViewModel();
            vm = this.BindingContext as PlantsViewModel;
        }

        public void RefreshData(object sender, EventArgs e)
        {
            vm.RefreshSource();
        }
    }
}
