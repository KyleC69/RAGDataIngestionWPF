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



using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface IUserDataService
    {


    UserViewModel GetUser();


    void Initialize();


    event EventHandler<UserViewModel> UserDataUpdated;
    }