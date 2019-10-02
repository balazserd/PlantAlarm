using System;
using Plugin.Media.Abstractions;

namespace PlantAlarm.DependencyServices
{
    public interface IMediaManagementService
    {
        bool IsSavingPhotosToCameraRollPermitted();
        void RequestAccessToCameraRoll();
        void SavePhotoToCameraRoll(MediaFile photo);
        void ShowExplanatoryTextForDenyingPhotoAlbumRequest();
    }
}
