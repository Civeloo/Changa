using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using Changa.Services;
using Reactive.Bindings;
using Prism.Events;
using Changa.Events;
using System.Reactive.Linq;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using Changa.Models;
using Changa.Resx;

namespace Changa.ViewModels
{
    public class HomePageViewModel : ViewModelBase
    {
        private readonly IAccountService _accountService;
        private readonly IItemListService _itemListService;
        private readonly INoticeListService _noticeListService;
        private readonly INoticeService _noticeService;
        private readonly IItemService _itemService;
        private readonly IUserService _userService;
        
        private readonly IEventAggregator _eventAggregator;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly IPageDialogService _pageDialogService;

        public ReadOnlyReactiveCollection<ItemViewModel> Items { get; }
        public ReadOnlyReactiveCollection<NoticeViewModel> Notices { get; }

        public AsyncReactiveCommand<ItemViewModel> LoadMoreCommand { get; } = new AsyncReactiveCommand<ItemViewModel>();
        public AsyncReactiveCommand<NoticeViewModel> NoticeCommand { get; } = new AsyncReactiveCommand<NoticeViewModel>();
        public AsyncReactiveCommand GoToContributionPageCommand { get; } = new AsyncReactiveCommand();
        public AsyncReactiveCommand<ItemViewModel> GoToItemDetailPageCommand { get; } = new AsyncReactiveCommand<ItemViewModel>();

        public HomePageViewModel(INavigationService navigationService, IAccountService accountService, IItemListService itemListService, IItemService itemService, INoticeListService noticeListService, INoticeService noticeService, IUserService userService, IEventAggregator eventAggregator, IPageDialogService pageDialogService) : base(navigationService)
        {
            _accountService = accountService;
            _itemListService = itemListService;
            _noticeListService = noticeListService;
            _noticeService = noticeService;
            _itemService = itemService;
            _userService = userService;//new UserService();
            _eventAggregator = eventAggregator;
            _pageDialogService = pageDialogService;

            Title = AppResources.Home;
            ToolbarItemName = AppResources.Post;

            Notices = _noticeListService.Notices.ToReadOnlyReactiveCollection(i => new NoticeViewModel(i)).AddTo(_disposables);

            Items = _itemListService.Items.ToReadOnlyReactiveCollection(i => new ItemViewModel(i)).AddTo(_disposables);

            LoadMoreCommand.Subscribe(async item =>
            {
                if (item == Items?.LastOrDefault())
                {
                    await _itemListService.LoadAsync();
                }
            });

            NoticeCommand.Subscribe(async notice =>
            {
                await OnClickNotifyAsync(notice);
            });

            //GoToContributionPageCommand.Subscribe(async () => await NavigateAsync<ContributionPageViewModel>());
            GoToContributionPageCommand.Subscribe(async () =>
                {
                    if (await ExistPhoneAsync()) await NavigateAsync<ContributionPageViewModel>();                    
                }
            );

            GoToItemDetailPageCommand.Subscribe(async viewModel => await NavigateAsync<ItemDetailPageViewModel, string>(viewModel.Id.Value));

            _eventAggregator.GetEvent<DestoryEvent>().Subscribe(_itemListService.Close)
                            .AddTo(_disposables);

            _noticeListService.LoadAsync(_accountService.UserId.Value);
            _itemListService.LoadAsync();

            ExistPhoneAsync();
        }

        public override void Destroy()
        {
            base.Destroy();

            _itemListService.Close();
            _disposables.Dispose();
        }

        public async Task OnClickNotifyAsync(NoticeViewModel notice)
        {
            var good = await _pageDialogService.DisplayAlertAsync(notice.Title.Value, notice.Comment.Value, AppResources.Yes, AppResources.No);
            switch (notice.Action.Value)
            {
                //case "Job":
                //    if (good) await NavigateAsync<ItemDetailPageViewModel, string>(notice.ItemId.Value);
                //    break;
                case "Calificate":
                    {
                        await _userService.CalificateUserAync(notice.UserId.Value, good);
                        //await _noticeService.DeleteAsync(_accountService.UserId.Value, notice.Id.Value);
                    }
                    break;
                default:
                    if (good) await NavigateAsync<ItemDetailPageViewModel, string>(notice.ItemId.Value);                    
                    break;
            }
            await _noticeService.DeleteAsync(_accountService.UserId.Value, notice.Id.Value);
        }

        public async Task<bool> ExistPhoneAsync()
        {
            if (!string.IsNullOrEmpty(_accountService.UserPhoneNumber.Value))
                return true;
            else
            {
                await _pageDialogService.DisplayAlertAsync(AppResources.Phone, AppResources.Please_complete_your_phone_number, AppResources.Ok);
                await NavigateAsync<MyPageViewModel>();                
            }
            return false;
        }

    }
}
