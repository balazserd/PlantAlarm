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

        void SetCollectionViewHeight(object sender, EventArgs e)
        {
            var colView = sender as CollectionView;
            var gridLayout = colView.ItemsLayout as GridItemsLayout;

            var horizontalMarginsTotalSize = ImageCollectionView.Margin.Left + ImageCollectionView.Margin.Right;
            var colViewHeight = ((this.Width - 2 * gridLayout.HorizontalItemSpacing - horizontalMarginsTotalSize) / 3) * 2 + gridLayout.VerticalItemSpacing;

            colView.HeightRequest = colViewHeight;
        }
    }
}
