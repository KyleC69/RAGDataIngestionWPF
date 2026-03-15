// Build Date: ${CurrentDate.Year}/${CurrentDate.Month}/${CurrentDate.Day}
// Solution: ${File.SolutionName}
// Project:   ${File.ProjectName}
// File:         ${File.FileName}
// Author: Kyle L. Crowder
// Build Num: ${CurrentDate.Hour}${CurrentDate.Minute}${CurrentDate.Second}
//
//
//
//



using Microsoft.Toolkit.Uwp.Notifications;

using RAGDataIngestionWPF.Contracts.Services;

using Windows.UI.Notifications;




namespace RAGDataIngestionWPF.Services;





public sealed partial class ToastNotificationsService : IToastNotificationsService
    {

    public void ShowToastNotification(ToastNotification toastNotification)
        {
        ToastNotificationManagerCompat.CreateToastNotifier().Show(toastNotification);
        }
    }