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



using Windows.UI.Notifications;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface IToastNotificationsService
    {
    void ShowToastNotification(ToastNotification toastNotification);


    void ShowToastNotificationSample();
    }