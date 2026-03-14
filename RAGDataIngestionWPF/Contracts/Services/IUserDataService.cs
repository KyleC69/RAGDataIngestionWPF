// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   RAGDataIngestionWPF
// File:         IUserDataService.cs
// Author: Kyle L. Crowder
// Build Num: 175107



using RAGDataIngestionWPF.ViewModels;




namespace RAGDataIngestionWPF.Contracts.Services;





public interface IUserDataService
{


    UserViewModel GetUser();


    void Initialize();


    event EventHandler<UserViewModel> UserDataUpdated;
}