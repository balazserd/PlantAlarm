using System;
using CoreGraphics;
using Foundation;
using PlantAlarm.iOS.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using PclOverlappingEffect = PlantAlarm.Effects.OverlappingEffect;

[assembly: ExportEffect(typeof(OverlappingEffect), nameof(OverlappingEffect))]
namespace PlantAlarm.iOS.Effects
{
    public class OverlappingEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            UICollectionView colView = (this.Control ?? this.Container).Subviews[0] as UICollectionView;

            var flowLayout = new CustomFlowLayout(40, 40);
            colView.CollectionViewLayout = flowLayout;
        }

        protected override void OnDetached()
        {
            
        }
    }

    public class CustomFlowLayout : UICollectionViewFlowLayout
    {
        public CustomFlowLayout(nfloat width, nfloat height)
        {
            ItemSize = new CGSize(width, height);
            ScrollDirection = UICollectionViewScrollDirection.Horizontal;
        }

        public override CGSize CollectionViewContentSize
        {
            get
            {
                var xSize = CollectionView.NumberOfItemsInSection(0) * ItemSize.Width;
                var ySize = CollectionView.NumberOfSections() * ItemSize.Height;

                var contentSize = new CGSize(xSize, ySize);

                if (CollectionView.Bounds.Size.Width > contentSize.Width)
                    contentSize.Width = CollectionView.Bounds.Size.Width;

                if (CollectionView.Bounds.Size.Height > contentSize.Height)
                    contentSize.Height = CollectionView.Bounds.Size.Height;

                return contentSize;
            }
        }

        public override UICollectionViewLayoutAttributes[] LayoutAttributesForElementsInRect(CGRect rect)
        {
            var attributesArray = base.LayoutAttributesForElementsInRect(rect);
            var numberOfItems = CollectionView.NumberOfItemsInSection(0);

            foreach (var attributes in attributesArray)
            {
                var xPosition = attributes.Center.X;
                var yPosition = attributes.Center.Y;

                if (attributes.IndexPath.Row == 0)
                    attributes.ZIndex = int.MaxValue; // Put the first cell on top of the stack
                else
                {
                    xPosition -= 10 * attributes.IndexPath.Row; //20 - value based on your cell width
                    attributes.ZIndex = numberOfItems - attributes.IndexPath.Row; //Other cells below the first one
                }

                attributes.Center = new CGPoint(xPosition, yPosition);
            }

            return attributesArray;
        }
    }
}
