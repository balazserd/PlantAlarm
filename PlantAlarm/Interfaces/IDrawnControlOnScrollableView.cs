using System;
namespace PlantAlarm.Interfaces
{
    //This interface must be implemented for itemcontainer controls that appear in scrollviews, because if you scroll while pressing an item, the pressing can get "stuck".
    //So, all IDrawnControlsOnScrollableView items will be released from this stuck state if the RemoveSelectedState is called on them.
    public interface IDrawnControlOnScrollableView
    {
        void RemoveSelectedState();
        void AddSelectedState();
    }
}
