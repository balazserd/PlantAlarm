using System;
using PlantAlarm.CustomControls;
using PlantAlarm.iOS.CustomRenderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(BorderlessDatePicker), typeof(BorderlessDatePickerRenderer_iOS))]
namespace PlantAlarm.iOS.CustomRenderers
{
    public class BorderlessDatePickerRenderer_iOS : DatePickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<DatePicker> e)
        {
            base.OnElementChanged(e);

            try
            {
                Control.Layer.BorderWidth = 0;
                Control.BorderStyle = UITextBorderStyle.None;
            }
            catch (NullReferenceException) { }
        }
    }
}
