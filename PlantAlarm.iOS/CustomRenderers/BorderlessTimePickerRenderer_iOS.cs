using System;
using PlantAlarm.CustomControls;
using PlantAlarm.iOS.CustomRenderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(BorderlessTimePicker), typeof(BorderlessTimePickerRenderer_iOS))]
namespace PlantAlarm.iOS.CustomRenderers
{
    public class BorderlessTimePickerRenderer_iOS : TimePickerRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<TimePicker> e)
        {
            base.OnElementChanged(e);
            try
            {
                Control.Layer.BorderWidth = 0;
                Control.BorderStyle = UITextBorderStyle.None;
            }
            catch (NullReferenceException) {}
        }
    }
}
