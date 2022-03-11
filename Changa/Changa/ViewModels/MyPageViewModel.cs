using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using Changa.Services;
using Prism.Events;
using Reactive.Bindings;
using Changa.Events;
using System.Reactive.Disposables;
using Reactive.Bindings.Extensions;
using System.Reactive.Linq;
using Xamarin.Forms;
using System.Reactive.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Xamarin.Forms.GoogleMaps;
using Changa.Resx;

namespace Changa.ViewModels
{
    public class MyPageViewModel : ViewModelBase
    {
        //private readonly IUserItemListService _userItemListService;
        private readonly IAccountService _accountService;
        //private readonly IEventAggregator _eventAggregator;
        private readonly IPageDialogService _pageDialogService;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ReadOnlyReactiveCollection<ItemViewModel> Items { get; }
        public ReactivePropertySlim<string> UserName { get; }
        public ReactivePropertySlim<int> UserReputation { get; }
        public ReactivePropertySlim<string> UserLocation { get; }
        public ReactivePropertySlim<string> UserPostalCode { get; }
        public ReactivePropertySlim<string> UserPhoneNumber { get; }
        public ReactivePropertySlim<string> UserEmail { get; }
        public ReadOnlyReactivePropertySlim<string> UserImage { get; }
        public ReadOnlyReactivePropertySlim<int> ContributionCount { get; }

        public ReadOnlyReactivePropertySlim<Map> MyMap { get; }

        public AsyncReactiveCommand SaveCommand { get; } = new AsyncReactiveCommand();
        //public AsyncReactiveCommand<ItemViewModel> LoadMoreCommand { get; } = new AsyncReactiveCommand<ItemViewModel>();
        //public AsyncReactiveCommand<ItemViewModel> GoToItemDetailPageCommand { get; } = new AsyncReactiveCommand<ItemViewModel>();

        //public MyPageViewModel(INavigationService navigationService, IUserItemListService userItemListService, IAccountService accountService, IPageDialogService pageDialogService, IEventAggregator eventAggregator) : base(navigationService)
        public MyPageViewModel(INavigationService navigationService, IAccountService accountService, IPageDialogService pageDialogService) : base(navigationService)
        {
            //_userItemListService = userItemListService;
            _accountService = accountService;
            _pageDialogService = pageDialogService;
            //_eventAggregator = eventAggregator;

            Title = AppResources.My_page;

            //_userItemListService.SetUserId(_accountService.UserId.Value);
            //Items = _userItemListService.Items.ToReadOnlyReactiveCollection(i => new ItemViewModel(i)).AddTo(_disposables);

            UserName = _accountService.UserName;
            UserReputation = _accountService.UserReputation;
            UserLocation = _accountService.UserLocation;
            UserPostalCode = _accountService.UserPostalCode;
            UserPhoneNumber = _accountService.UserPhoneNumber;
            UserEmail = _accountService.UserEmail;
            UserImage = _accountService.UserImage; 

            ContributionCount = _accountService.ContributionCount;

            //LoadMoreCommand.Subscribe(async item =>
            //{
            //    if (item == Items?.LastOrDefault())
            //    {
            //        await _userItemListService.LoadAsync();
            //    }
            //});

            MyMap = _accountService.MyMap
                .ToReadOnlyReactivePropertySlim()
                .AddTo(_disposables);

            _accountService.CompletedNotifier
                                .ObserveOn(SynchronizationContext.Current)
                                .SelectMany(_ => _pageDialogService.DisplayAlertAsync(null, AppResources.Has_been_completed, AppResources.Ok).ToObservable())
                                .ObserveOn(SynchronizationContext.Current)
                                .SelectMany(_ => GoBackAsync())//NavigateAsync<HomePageViewModel>())
                                .Subscribe()
                                .AddTo(_disposables);

            _accountService.ErrorNotifier
                                .ObserveOn(SynchronizationContext.Current)
                                .SelectMany(_ => _pageDialogService.DisplayAlertAsync(AppResources.Error, AppResources.Failed, AppResources.Ok).ToObservable())
                                .Subscribe()
                                .AddTo(_disposables);

            _accountService.IsNotifier
                                .Skip(1)
                                .Where(b => b)
                                .ObserveOn(SynchronizationContext.Current)
                                .SelectMany(_ => NavigateAsync<LoadingPageViewModel>())
                                .Subscribe()
                                .AddTo(_disposables);

            _accountService.IsNotifier
                                .Skip(1)
                                .Where(b => !b)
                                .ObserveOn(SynchronizationContext.Current)
                                .SelectMany(_ => GoBackAsync())
                                .Subscribe()
                                .AddTo(_disposables);

            SaveCommand.Subscribe(async () => await _accountService.SaveAsync());

            _accountService.GetMap();

            //GoToItemDetailPageCommand.Subscribe(async viewModel => await NavigateAsync<ItemDetailPageViewModel, string>(viewModel.Id.Value));

            //_eventAggregator.GetEvent<DestoryEvent>().Subscribe(_userItemListService.Close).AddTo(_disposables);

            //_userItemListService.LoadAsync();
            
        }

        public override void Destroy()
        {
            base.Destroy();

            //_userItemListService.Close();
            _disposables.Dispose();
        }
    }
}
