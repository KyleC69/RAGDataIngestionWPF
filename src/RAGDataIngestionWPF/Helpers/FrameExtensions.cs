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



using System.Windows;
using System.Windows.Controls;

using JetBrains.Annotations;




namespace RAGDataIngestionWPF.Helpers;





public static class FrameExtensions
    {

    public static void CleanNavigation([NotNull] this Frame frame)
        {
        while (frame.CanGoBack)
            {
            _ = frame.RemoveBackEntry();
            }
        }








    [CanBeNull]
    public static object GetDataContext([NotNull] this Frame frame)
        {
        return frame.Content is FrameworkElement element ? element.DataContext : null;

        }
    }