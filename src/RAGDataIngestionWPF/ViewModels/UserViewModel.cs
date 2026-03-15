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



using System.Windows.Media.Imaging;

using CommunityToolkit.Mvvm.ComponentModel;




namespace RAGDataIngestionWPF.ViewModels;





public sealed partial class UserViewModel : ObservableObject
    {
    [ObservableProperty]
    public partial string Name { get; set; }





    [ObservableProperty]
    public partial BitmapImage Photo { get; set; }





    [ObservableProperty]
    public partial string UserPrincipalName { get; set; }
    }