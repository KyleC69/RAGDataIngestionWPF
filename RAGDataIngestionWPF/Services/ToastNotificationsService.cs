// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         ToastNotificationsService.cs
// Author: Kyle L. Crowder
// Build Num: 175111



using Windows.UI.Notifications;

using Microsoft.Toolkit.Uwp.Notifications;

using RAGDataIngestionWPF.Contracts.Services;




namespace RAGDataIngestionWPF.Services;





public partial class ToastNotificationsService : IToastNotificationsService
{

    public void ShowToastNotification(ToastNotification toastNotification)
    {
        ToastNotificationManagerCompat.CreateToastNotifier().Show(toastNotification);
    }
}