using System;
using Xamarin.Forms;

namespace PlantAlarm.CustomControls
{
    public class SquaredImageContainerStack : StackLayout
    {
        protected override void OnSizeAllocated(double width, double height)
        {
            this.HeightRequest = width;
            base.OnSizeAllocated(width, height);
        }
    }
}
