using System;
using PlantAlarm.CustomControls;
using PlantAlarm.iOS.CustomRenderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(RoundedImage), typeof(RoundedImageRenderer_iOS))]
namespace PlantAlarm.iOS.CustomRenderers
{
    public class RoundedImageRenderer_iOS : ImageRenderer
    {
        public RoundedImageRenderer_iOS()
        {
            Layer.CornerRadius = 3;
            ClipsToBounds = true;
        }
    }
}
