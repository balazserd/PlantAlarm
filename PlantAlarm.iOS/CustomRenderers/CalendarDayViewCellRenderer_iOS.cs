using System;
using CoreGraphics;
using PlantAlarm.CustomControls;
using PlantAlarm.iOS.CustomRenderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CalendarDayViewCell), typeof(CalendarDayViewCellRenderer_iOS))]
namespace PlantAlarm.iOS.CustomRenderers
{
    public class CalendarDayViewCellRenderer_iOS : ViewCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var cell = base.GetCell(item, reusableCell, tv);

            var backgroundView = new UIView();

            backgroundView.BackgroundColor = UIColor.Clear;
            backgroundView.Layer.BackgroundColor = UIColor.Clear.CGColor;

            backgroundView.Layer.CornerRadius = 5;
            backgroundView.Layer.BorderColor = UIColor.FromRGB(92, 144, 176).CGColor;
            backgroundView.Layer.BorderWidth = 2;

            cell.SelectedBackgroundView = backgroundView;

            return cell;
        }
    }
}
