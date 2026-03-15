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



using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using JetBrains.Annotations;

using RAGDataIngestionWPF.Core.Contracts.Services;
using RAGDataIngestionWPF.Core.Helpers;
using RAGDataIngestionWPF.Properties;




namespace RAGDataIngestionWPF.ViewModels;





public sealed partial class LogInViewModel(IIdentityService identityService) : ObservableObject
    {
    private readonly IIdentityService _identityService = identityService;





    public bool IsBusy
        {
        get;
        set
            {
            _ = this.SetProperty(ref field, value);
            LoginCommand.NotifyCanExecuteChanged();
            }
        }





    [NotNull]
    public RelayCommand LoginCommand => field ??= new RelayCommand(this.OnLogin, () => !IsBusy);





    [ObservableProperty]
    public partial string StatusMessage { get; set; }








    private static string GetStatusMessage(LoginResultType loginResult)
        {
        return loginResult switch
            {
                LoginResultType.Unauthorized => Resources.StatusUnauthorized,
                LoginResultType.NoNetworkAvailable => Resources.StatusNoNetworkAvailable,
                LoginResultType.UnknownError => Resources.StatusLoginFails,
                _ => string.Empty
                };
        }








    private async void OnLogin()
        {
        IsBusy = true;
        StatusMessage = string.Empty;
        LoginResultType loginResult = await _identityService.LoginAsync();
        StatusMessage = GetStatusMessage(loginResult);
        IsBusy = false;
        }
    }