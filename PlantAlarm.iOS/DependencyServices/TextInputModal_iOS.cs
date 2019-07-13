using System;
using PlantAlarm.iOS.DependencyServices;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(TextInputModal_iOS))]
namespace PlantAlarm.iOS.DependencyServices
{
    public class TextInputModal_iOS
    {
        public string ShowModal()
        {
            string returnValue = null;
            var modal = UIAlertController.Create("New Category", "Please enter the name for the new category you wish to add.", UIAlertControllerStyle.Alert);

            modal.AddTextField((textField) =>
            {
                textField.Placeholder = "ExampleCategory";
            });

            modal.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
            modal.AddAction(UIAlertAction.Create("Add", UIAlertActionStyle.Default, (action) => { returnValue = modal.TextFields[0].Text; }));

            return returnValue;
        }
    }
}
