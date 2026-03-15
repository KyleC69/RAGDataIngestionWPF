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



using System.Collections.ObjectModel;
using System.Windows.Input;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using JetBrains.Annotations;

using MahApps.Metro.Controls;

using RAGDataIngestionWPF.Contracts.Services;
using RAGDataIngestionWPF.Properties;




namespace RAGDataIngestionWPF.ViewModels;





public sealed partial class ShellViewModel : ObservableObject
    {
    private readonly INavigationService _navigationService;
    private readonly IUserDataService _userDataService;








    public ShellViewModel(INavigationService navigationService, IUserDataService userDataService)
        {
        _navigationService = navigationService;
        _userDataService = userDataService;
        }








    [NotNull]
    public RelayCommand GoBackCommand => field ??= new RelayCommand(this.OnGoBack, this.CanGoBack);





    [NotNull]
    public ICommand LoadedCommand => field ??= new RelayCommand(this.OnLoaded);





    [NotNull]
    public ICommand MenuItemInvokedCommand => field ??= new RelayCommand(this.OnMenuItemInvoked);





    // TODO: Change the icons and titles for all HamburgerMenuItems here.
    public ObservableCollection<HamburgerMenuItem> MenuItems { get; } =
    [
            new HamburgerMenuGlyphItem { Label = Resources.ShellMainPage, Glyph = "\uE8A5", TargetPageType = typeof(MainViewModel) },
            new HamburgerMenuGlyphItem { Label = Resources.ShellBlankPage, Glyph = "\uE8A5", TargetPageType = typeof(BlankViewModel) },
            new HamburgerMenuGlyphItem { Label = Resources.ShellListDetailsPage, Glyph = "\uE8A5", TargetPageType = typeof(ListDetailsViewModel) },
            new HamburgerMenuGlyphItem { Label = Resources.ShellDataGridPage, Glyph = "\uE8A5", TargetPageType = typeof(DataGridViewModel) },
            new HamburgerMenuGlyphItem { Label = Resources.ShellWebViewPage, Glyph = "\uE8A5", TargetPageType = typeof(WebViewViewModel) }
    ];

    public ObservableCollection<HamburgerMenuItem> OptionMenuItems { get; } =
    [
            new HamburgerMenuGlyphItem { Label = Resources.ShellSettingsPage, Glyph = "\uE713", TargetPageType = typeof(SettingsViewModel) }
    ];





    [NotNull]
    public ICommand OptionsMenuItemInvokedCommand => field ??= new RelayCommand(this.OnOptionsMenuItemInvoked);





    [ObservableProperty]
    public partial HamburgerMenuItem SelectedMenuItem { get; set; }





    [ObservableProperty]
    public partial HamburgerMenuItem SelectedOptionsMenuItem { get; set; }





    [NotNull]
    public ICommand UnloadedCommand => field ??= new RelayCommand(this.OnUnloaded);








    private bool CanGoBack()
        {
        return _navigationService.CanGoBack;
        }








    private void NavigateTo([CanBeNull] Type targetViewModel)
        {
        if (targetViewModel != null)
            {
            _ = _navigationService.NavigateTo(targetViewModel.FullName);
            }
        }








    private void OnGoBack()
        {
        _navigationService.GoBack();
        }








    private void OnLoaded()
        {
        _navigationService.Navigated += this.OnNavigated;
        _userDataService.UserDataUpdated += this.OnUserDataUpdated;
        UserViewModel user = _userDataService.GetUser();
        HamburgerMenuImageItem userMenuItem = new()
            {
            Thumbnail = user.Photo,
            Label = user.Name,
            Command = new RelayCommand(this.OnUserItemSelected)
            };

        OptionMenuItems.Insert(0, userMenuItem);
        }








    private void OnMenuItemInvoked()
        {
        this.NavigateTo(SelectedMenuItem.TargetPageType);
        }








    private void OnNavigated(object sender, string viewModelName)
        {
        HamburgerMenuItem item = MenuItems
                .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
        if (item != null)
            {
            SelectedMenuItem = item;
            }
        else
            {
            SelectedOptionsMenuItem = OptionMenuItems
                    .FirstOrDefault(i => viewModelName == i.TargetPageType?.FullName);
            }

        GoBackCommand.NotifyCanExecuteChanged();
        }








    private void OnOptionsMenuItemInvoked()
        {
        this.NavigateTo(SelectedOptionsMenuItem.TargetPageType);
        }








    private void OnUnloaded()
        {
        _navigationService.Navigated -= this.OnNavigated;
        _userDataService.UserDataUpdated -= this.OnUserDataUpdated;
        HamburgerMenuImageItem userMenuItem = OptionMenuItems.OfType<HamburgerMenuImageItem>().FirstOrDefault();
        if (userMenuItem != null)
            {
            var unused = OptionMenuItems.Remove(userMenuItem);
            }
        }








    private void OnUserDataUpdated(object sender, UserViewModel user)
        {
        HamburgerMenuImageItem userMenuItem = OptionMenuItems.OfType<HamburgerMenuImageItem>().FirstOrDefault();
        if (userMenuItem != null)
            {
            userMenuItem.Label = user.Name;
            userMenuItem.Thumbnail = user.Photo;
            }
        }








    private void OnUserItemSelected()
        {
        this.NavigateTo(typeof(SettingsViewModel));
        }
    }