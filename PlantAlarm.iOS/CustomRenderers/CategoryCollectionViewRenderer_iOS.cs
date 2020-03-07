using System;
using PlantAlarm.CustomControls;
using PlantAlarm.iOS.CustomRenderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CategoryCollectionView), typeof(CategoryCollectionViewRenderer_iOS))]
namespace PlantAlarm.iOS.CustomRenderers
{
    public class CategoryCollectionViewRenderer_iOS : CollectionViewRenderer
    { 
        protected override void OnElementChanged(ElementChangedEventArgs<GroupableItemsView> e)
        {
            var backgroundView = new UIView();
            backgroundView.BackgroundColor = UIColor.Clear;
            backgroundView.Layer.BackgroundColor = UIColor.Clear.CGColor;

            base.OnElementChanged(e);
            if (Control.PreferredFocusEnvironments[0] is UICollectionView collectionView)
            {
                foreach (var cell in collectionView.VisibleCells)
                {
                    cell.SelectedBackgroundView = backgroundView;
                }
            }
        }
    }
}
