using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using Reactive.Bindings;
using Changa.Services;
using Prism.Services;
using Changa.Resx;

namespace Changa.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private string Home = AppResources.Home;
        private string Logout = AppResources.Log_out;
        private string MyPage = AppResources.My_page;

        private readonly IAccountService _accountService;
        private readonly IPageDialogService _pageDialogService;

        public ReactivePropertySlim<string> UserName { get; }
        public ReadOnlyReactivePropertySlim<string> UserImage { get; }

        public List<string> Menus { get; }

        public AsyncReactiveCommand<string> ShowDetailPageCommand { get; } = new AsyncReactiveCommand<string>();

        public MainPageViewModel(INavigationService navigationService, IAccountService accountService, IPageDialogService pageDialogService) : base(navigationService)
        {
            _accountService = accountService;
            _pageDialogService = pageDialogService;

            UserName = _accountService.UserName;
            UserImage = _accountService.UserImage;

            Menus = new List<string>
            {
                Home,
                MyPage,
                Logout
            };

            ShowDetailPageCommand.Subscribe(async page =>
            {
                if (page == Home)
                    await NavigateAsync<HomePageViewModel>(wrapInNavigationPage: true);
                else if (page == MyPage)
                    await NavigateAsync<MyPageViewModel>(wrapInNavigationPage: true);
                else if (page == Logout)
                {
                    var ok = await _pageDialogService.DisplayAlertAsync(AppResources.Confirmation, AppResources.Are_you_sure_you_want_to_log_out, AppResources.Yes, AppResources.No);
                    if (ok)
                    {
                        _accountService.Logout();
                        await NavigateAsync<LoginPageViewModel>(wrapInNavigationPage: true, noHistory: true);
                    }
                }
                //switch (page)
                //{
                //    case (Home):
                //        await NavigateAsync<HomePageViewModel>(wrapInNavigationPage: true);
                //        break;
                //    case MyPage:
                //        await NavigateAsync<MyPageViewModel>(wrapInNavigationPage: true);
                //        break;
                //    case Logout:
                //        var ok = await _pageDialogService.DisplayAlertAsync(AppResources.Confirmation, AppResources.Are_you_sure_you_want_to_log_out, AppResources.Yes, AppResources.No);
                //        if (ok)
                //        {
                //            _accountService.Logout();
                //            await NavigateAsync<LoginPageViewModel>(wrapInNavigationPage: true, noHistory: true);
                //        }
                //        break;
                //}
            });
        }
    }
}
