using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Navigation;
using Prism.Logging;
using Prism.Services;
using Reactive.Bindings;
using Changa.Services;
using System.Reactive.Linq;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using Xamarin.Forms;
using System.Threading;
using System.Reactive.Threading.Tasks;
using Plugin.Media;
using Plugin.FirebaseAnalytics;
using Xamarin.Forms.GoogleMaps;
using Xamarin.Essentials;
using Changa.Resx;

namespace Changa.ViewModels
{
    public class ItemDetailPageViewModel : ViewModelBase<string>
    {
        private readonly IItemService _itemService;
        private readonly IPageDialogService _pageDialogService;
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        public ReadOnlyReactivePropertySlim<Xamarin.Forms.GoogleMaps.Map> MyMap { get; }
        public ReadOnlyReactiveCollection<BidViewModel> Bids { get; }

        //public ReadOnlyReactivePropertySlim<string> ItemImage { get; }
        public ReadOnlyReactivePropertySlim<string> ItemTitle { get; }
        public ReadOnlyReactivePropertySlim<string> ItemComment { get; }
        public ReadOnlyReactivePropertySlim<double> ItemPrice { get; }
        public ReadOnlyReactivePropertySlim<string> ItemLocation { get; }
        public ReadOnlyReactivePropertySlim<int> ItemHours { get; }
        public ReadOnlyReactivePropertySlim<string> ItemStatus { get; }
        public ReadOnlyReactivePropertySlim<string> ItemAssigned { get; }
        public ReadOnlyReactivePropertySlim<bool> IsAssigned { get; }
        public ReadOnlyReactivePropertySlim<string> TbItemName { get; }
        //public ReadOnlyReactivePropertySlim<string> ToolbarItemName { get; }

        public ReadOnlyReactivePropertySlim<int> BidCount { get; }
        public ReadOnlyReactivePropertySlim<string> UserName { get; }
        public ReadOnlyReactivePropertySlim<int> UserReputation { get; }
        public ReadOnlyReactivePropertySlim<string> UserPhoneNumber { get; }
        //public ReadOnlyReactivePropertySlim<string> OwnerName { get; }
        //public ReadOnlyReactivePropertySlim<int> OwnerReputation { get; }
        //public ReadOnlyReactivePropertySlim<string> OwnerPhoneNumber { get; }
        public ReadOnlyReactivePropertySlim<string> OwnerImage { get; }
        public ReadOnlyReactivePropertySlim<bool> IsBid { get; }
        public ReadOnlyReactivePropertySlim<bool> IsOwner { get; }

        public ReactiveCommand BidOrUnBidCommand { get; }
        public AsyncReactiveCommand DeleteCommand { get; }

        public AsyncReactiveCommand AssignCommand { get; }

        public AsyncReactiveCommand PhoneCommand { get; }

        public ItemDetailPageViewModel(INavigationService navigationService, IItemService itemService, IPageDialogService pageDialogService) : base(navigationService)
        {
try { 
            _itemService = itemService;
            _pageDialogService = pageDialogService;

            Title = AppResources.Work;

            //ItemImage = _itemService.Item
            //                        .Select(item => item != null ? item.ObserveProperty(i => i.Image) : Observable.Return<string>(null))
            //                        .Switch()
            //                        .ToReadOnlyReactivePropertySlim()
            //                        .AddTo(_disposables);

            ItemTitle = _itemService.Item
                                    .Select(item => item != null ? item.ObserveProperty(i => i.Title) : Observable.Return<string>(null))
                                    .Switch()
                                    .ToReadOnlyReactivePropertySlim()
                                    .AddTo(_disposables);

            ItemComment = _itemService.Item
                                      .Select(item => item != null ? item.ObserveProperty(i => i.Comment) : Observable.Return<string>(null))
                                      .Switch()
                                      .ToReadOnlyReactivePropertySlim()
                                      .AddTo(_disposables);

            ItemPrice = _itemService.Item
                                      .Select(item => item != null ? item.ObserveProperty(i => i.Price) : Observable.Return<double>(0))
                                      .Switch()
                                      .ToReadOnlyReactivePropertySlim()
                                      .AddTo(_disposables);

            ItemLocation = _itemService.Item
                                      .Select(item => item != null ? item.ObserveProperty(i => i.Location) : Observable.Return<string>(null))
                                      .Switch()
                                      .ToReadOnlyReactivePropertySlim()
                                      .AddTo(_disposables);

            MyMap = _itemService.MyMap.ToReadOnlyReactivePropertySlim().AddTo(_disposables);
            Bids = _itemService.Bids.ToReadOnlyReactiveCollection().AddTo(_disposables);

            ItemHours = _itemService.Item
                                      .Select(item => item != null ? item.ObserveProperty(i => i.Hours) : Observable.Return<int>(0))
                                      .Switch()
                                      .ToReadOnlyReactivePropertySlim()
                                      .AddTo(_disposables);

            ItemStatus = _itemService.Item
                                      .Select(item => item != null ? item.ObserveProperty(i => i.Status) : Observable.Return<string>(null))
                                      .Switch()
                                      .ToReadOnlyReactivePropertySlim()
                                      .AddTo(_disposables);

            ItemAssigned = _itemService.Item
                          .Select(item => item != null ? item.ObserveProperty(i => i.Assigned) : Observable.Return<string>(null))
                          .Switch()
                          .ToReadOnlyReactivePropertySlim()
                          .AddTo(_disposables);

            //IsAssigned = _itemService.Item
            //            .Select(item => item != null ? item.ObserveProperty(i => !string.IsNullOrEmpty(i.Assigned)) : Observable.Return<bool>(false))
            //            .Switch()
            //            .ToReadOnlyReactivePropertySlim()
            //            .AddTo(_disposables);

            TbItemName = _itemService.TbItemName
                                    .ToReadOnlyReactivePropertySlim()
                                    .AddTo(_disposables);

            BidCount = _itemService.Item
                                    .Select(item => item != null ? item.ObserveProperty(i => i.BidCount) : Observable.Return(0))
                                    .Switch()
                                    .ToReadOnlyReactivePropertySlim()
                                    .AddTo(_disposables);

            //OwnerName = _itemService.Owner
            //                        .Select(user => user != null ? user.ObserveProperty(u => u.Name) : Observable.Return<string>(null))
            //                        .Switch()
            //                        .ToReadOnlyReactivePropertySlim()
            //                        .AddTo(_disposables);

            UserName = _itemService.UserName
                        .ToReadOnlyReactivePropertySlim()
                        .AddTo(_disposables);
            UserReputation = _itemService.UserReputation
                        .ToReadOnlyReactivePropertySlim()
                        .AddTo(_disposables);
            UserPhoneNumber = _itemService.UserPhoneNumber
                        .ToReadOnlyReactivePropertySlim()
                        .AddTo(_disposables);

            OwnerImage = _itemService.Owner
                                     .Select(user => user != null ? user.ObserveProperty(u => u.Image) : Observable.Return<string>(null))
                                     .Switch()
                                     .ToReadOnlyReactivePropertySlim()
                                     .AddTo(_disposables);

            IsBid = _itemService.IsBid;

            IsOwner = _itemService.IsOwner;

            _itemService.DeleteCompletedNotifier
                        .ObserveOn(SynchronizationContext.Current)
                        .SelectMany(_ => _pageDialogService.DisplayAlertAsync(null, AppResources.Process_is_complete, AppResources.Ok).ToObservable())
                        .ObserveOn(SynchronizationContext.Current)
                        .SelectMany(_ => GoBackAsync())
                        .Subscribe()
                        .AddTo(_disposables);

            _itemService.DeleteErrorNotifier
                        .ObserveOn(SynchronizationContext.Current)
                        .SelectMany(_ => _pageDialogService.DisplayAlertAsync(AppResources.Error, AppResources.Failed_to_process, AppResources.Ok).ToObservable())
                        .Subscribe()
                        .AddTo(_disposables);

            _itemService.IsDeleting
                        .Skip(1)
                        .Where(b => b)
                        .ObserveOn(SynchronizationContext.Current)
                        .SelectMany(_ => NavigateAsync<LoadingPageViewModel>())
                        .Subscribe()
                        .AddTo(_disposables);

            _itemService.IsDeleting
                        .Skip(1)
                        .Where(b => !b)
                        .ObserveOn(SynchronizationContext.Current)
                        .SelectMany(_ => GoBackAsync())
                        .Subscribe()
                        .AddTo(_disposables);           

            DeleteCommand = new[]
            {                
                //_itemService.IsOwner,
                _itemService.IsLoaded
            }.CombineLatestValuesAreAllTrue()
             .ObserveOn(SynchronizationContext.Current)
             .ToAsyncReactiveCommand()
             .AddTo(_disposables);

            DeleteCommand.Subscribe(async () =>
            {
                if ( TbItemName.Value == AppResources.Bid || TbItemName.Value == AppResources.UnBid ) await _itemService.BidOrUnBidAsync();
                else
                if (TbItemName.Value == AppResources.Delete)
                {
                    var ok = await _pageDialogService.DisplayAlertAsync(AppResources.Confirmation, AppResources.Are_you_sure_you_want_to_delete, AppResources.Yes, AppResources.No);
                    if (ok)
                    {
                        await _itemService.DeleteAsync();
                    }
                }
                else
                {
                    var ok = await _pageDialogService.DisplayAlertAsync(AppResources.Confirmation, AppResources.Are_you_sure_you_want_to_complete, AppResources.Yes, AppResources.No);
                    if (ok)
                    {
                        var good = await _pageDialogService.DisplayAlertAsync(AppResources.Calificate, AppResources.Are_you_satisfied_with_the_work, AppResources.Yes, AppResources.No);
                        await _itemService.CalificateWorkAsync(good);
                    }
                }
            }
            
            );
            
            AssignCommand = new[]
            {                
                _itemService.IsLoaded,                
                _itemService.IsOwner               
                
            }.CombineLatestValuesAreAllTrue()
             .ObserveOn(SynchronizationContext.Current)
             .ToAsyncReactiveCommand()
             .AddTo(_disposables);

            AssignCommand.Subscribe(async () =>
            {
                if (string.IsNullOrEmpty(ItemAssigned.Value))
                {
                    var ok = await _pageDialogService.DisplayAlertAsync(AppResources.Confirmation, AppResources.Are_you_sure_you_want_to_assign, AppResources.Yes, AppResources.No);
                    if (ok)
                    {
                        await _itemService.AssignAsync(Bids[0].Id.Value);
                    }
                }
                
            });

            PhoneCommand = new[]
            {
                _itemService.IsLoaded

            }.CombineLatestValuesAreAllTrue()
             .ObserveOn(SynchronizationContext.Current)
             .ToAsyncReactiveCommand()
             .AddTo(_disposables);
            PhoneCommand.Subscribe(async () =>
            {
                if (!string.IsNullOrEmpty(UserPhoneNumber.Value))
                {
                    try
                    {
                        PhoneDialer.Open(UserPhoneNumber.Value);
                    }
                    catch (Exception ex)
                    {
                        ProcessException(ex);
                    }
                }
            });

            }
catch (Exception e)
{
    System.Diagnostics.Debug.WriteLine(e);
}
        }

        public override void Prepare(string parameter)
        {
try { 
            _itemService.LoadAsync(parameter);
}
catch (Exception e)
{
    System.Diagnostics.Debug.WriteLine(e);
}
        }

        public override void Destroy()
        {
            base.Destroy();

            _disposables.Dispose();
        }

        void ProcessException(Exception ex)
        {
            if (ex != null)
                Application.Current.MainPage.DisplayAlert(AppResources.Error, ex.Message, AppResources.Ok);
        }
    }
}
