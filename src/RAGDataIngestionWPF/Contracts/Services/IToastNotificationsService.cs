// Build Date: 2026/03/21
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IToastNotificationsService.cs
// Author: Kyle L. Crowder
// Build Num: 140854



using Windows.UI.Notifications;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface IToastNotificationsService
{
    void ShowToastNotification(ToastNotification toastNotification);


    void ShowToastNotificationSample();
}