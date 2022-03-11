using System;
using Reactive.Bindings;
using System.Reactive.Linq;
using Plugin.CloudFirestore;
using Changa.Models;
using System.Threading.Tasks;
using System.Reactive.Subjects;
using Nito.AsyncEx;
using Reactive.Bindings.Notifiers;
using System.Reactive;
using Plugin.FirebaseAnalytics;
using Xamarin.Forms.GoogleMaps;
using Changa.ViewModels;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using Changa.Resx;

namespace Changa.Services
{
    public class ItemService : IItemService
    {
        private readonly IAccountService _accountService;
        private readonly IFirestore _firestore;
        private readonly AsyncLock _lock = new AsyncLock();
        private readonly CompositeDisposable _disposables = new CompositeDisposable();
        private readonly IUserService _userService;
        private readonly INoticeService _noticeService;
        private readonly ReactivePropertySlim<Item> _item = new ReactivePropertySlim<Item>();
        public ReadOnlyReactivePropertySlim<Item> Item { get; }

        public ReadOnlyReactivePropertySlim<bool> IsOwner { get; }
        private readonly ReactivePropertySlim<User> _owner = new ReactivePropertySlim<User>();
        public ReadOnlyReactivePropertySlim<User> Owner { get; }

        private readonly ReactivePropertySlim<Map> _myMap = new ReactivePropertySlim<Map>();
        public ReadOnlyReactivePropertySlim<Map> MyMap { get; }

        private readonly IBidListService _bidListService;
        private readonly ReactiveCollection<BidViewModel> _bids = new ReactiveCollection<BidViewModel>();
        public ReadOnlyReactiveCollection<BidViewModel> Bids { get; }

        private readonly ReactivePropertySlim<bool> _isBid = new ReactivePropertySlim<bool>();
        public ReadOnlyReactivePropertySlim<bool> IsBid { get; }

        private readonly ReactivePropertySlim<string> _tbItemName = new ReactivePropertySlim<string>();
        public ReadOnlyReactivePropertySlim<string> TbItemName { get; }

        private readonly ReactivePropertySlim<string> _userName = new ReactivePropertySlim<string>();
        public ReadOnlyReactivePropertySlim<string> UserName { get; }
        private readonly ReactivePropertySlim<int> _userReputation = new ReactivePropertySlim<int>();
        public ReadOnlyReactivePropertySlim<int> UserReputation { get; }
        private readonly ReactivePropertySlim<string> _userPhoneNumber = new ReactivePropertySlim<string>();
        public ReadOnlyReactivePropertySlim<string> UserPhoneNumber { get; }

        private ReactivePropertySlim<bool> _isLoaded = new ReactivePropertySlim<bool>();
        public ReadOnlyReactivePropertySlim<bool> IsLoaded { get; }

        private readonly Subject<string> _loadErrorNotifier = new Subject<string>();
        public IObservable<string> LoadErrorNotifier => _loadErrorNotifier;

        private readonly BusyNotifier _deletingNotifier = new BusyNotifier();
        public ReadOnlyReactivePropertySlim<bool> IsDeleting { get; }

        private readonly Subject<string> _deleteErrorNotifier = new Subject<string>();
        public IObservable<string> DeleteErrorNotifier => _deleteErrorNotifier;

        private readonly Subject<Unit> _deleteCompletedNotifier = new Subject<Unit>();
        public IObservable<Unit> DeleteCompletedNotifier => _deleteCompletedNotifier;

        //public ReadOnlyReactivePropertySlim<bool> IsAssigned { get; }
        private readonly ReactivePropertySlim<User> _assigned = new ReactivePropertySlim<User>();
        public ReadOnlyReactivePropertySlim<User> Assigned { get; }

        //public ReadOnlyReactivePropertySlim<string> OwnerPhoneNumber { get; }        

        //private readonly BusyNotifier _notifier = new BusyNotifier();
        //public ReadOnlyReactivePropertySlim<bool> IsNotifier { get; }
        //private readonly Subject<string> _errorNotifier = new Subject<string>();
        //public IObservable<string> ErrorNotifier => _errorNotifier;
        //private readonly Subject<Unit> _completedNotifier = new Subject<Unit>();
        //public IObservable<Unit> CompletedNotifier => _completedNotifier;

        public ItemService(IAccountService accountService, INoticeService noticeService, IBidListService bidListService, IUserService userService)
        {
try { 
            _accountService = accountService;
            _firestore = CrossCloudFirestore.Current.Instance;
            _userService = userService;// new UserService();
            _noticeService = noticeService;
            _bidListService = bidListService;

            Item = _item.ToReadOnlyReactivePropertySlim();
            Owner = _owner.ToReadOnlyReactivePropertySlim();
            Assigned = _assigned.ToReadOnlyReactivePropertySlim();
            IsBid = _isBid.ToReadOnlyReactivePropertySlim();
            TbItemName = _tbItemName.ToReadOnlyReactivePropertySlim();
            MyMap = _myMap.ToReadOnlyReactivePropertySlim();
            
            Bids = _bidListService.Bids.ToReadOnlyReactiveCollection(i => new BidViewModel(i)).AddTo(_disposables);

            IsOwner = Observable.CombineLatest(Item, _accountService.UserId, (item, userId) => item != null && item.OwnerId == userId)
                                .ToReadOnlyReactivePropertySlim();

            //IsAssigned = Item
            //            .Select(item => item != null ? item.ObserveProperty(i => !string.IsNullOrEmpty(i.Assigned)) : Observable.Return<bool>(false))
            //            .Switch()
            //            .ToReadOnlyReactivePropertySlim();

            IsLoaded = _isLoaded.ToReadOnlyReactivePropertySlim();
            IsDeleting = _deletingNotifier.ToReadOnlyReactivePropertySlim();

            UserName = _userName.ToReadOnlyReactivePropertySlim();
            UserReputation = _userReputation.ToReadOnlyReactivePropertySlim();
            UserPhoneNumber = _userPhoneNumber.ToReadOnlyReactivePropertySlim();
            }
catch (Exception e)
{
    System.Diagnostics.Debug.WriteLine(e);
}
        }

        public async Task LoadAsync(string id)
        {
try
{
                _isLoaded.Value = false;

                var itemDocument = await _firestore.GetCollection(Models.Item.CollectionPath)
                                                   .GetDocument(id)
                                                   .GetDocumentAsync()
                                                   .ConfigureAwait(false);

                var item = itemDocument.ToObject<Item>();

                if (item != null)
                {
                    _item.Value = item;

                    var bidTask = _firestore.GetDocument($"{User.CollectionPath}/{_accountService.UserId}/{Bid.CollectionPath}/{id}")
                                             .GetDocumentAsync();

                    var isAssigned = (!string.IsNullOrEmpty(item.Assigned));                    
                    if (IsOwner.Value)
                    {
                        if (isAssigned)
                        {
                            _assigned.Value = await _userService.GetUserAsync(item.Assigned);
                            _userName.Value = AppResources.ASSIGNED_TO+" "+_assigned.Value.Name
                                                +" ("+_assigned.Value.Reputation+") ";
                            _userPhoneNumber.Value = _assigned.Value.PhoneNumber;
                        }
                        else
                        {
                            _userName.Value = AppResources.BY+" "+_accountService.UserName.Value
                                                +" ("+_accountService.UserReputation.Value+") ";
                        }                        
                    }                    
                    else
                    {
                        _owner.Value = await _userService.GetUserAsync(item.OwnerId);
                        _userName.Value = AppResources.BY+" "+_owner.Value.Name +" ("+_owner.Value.Reputation+") ";

                        if (isAssigned && (_accountService.UserId.Value == item.Assigned))
                            _userPhoneNumber.Value = _owner.Value.PhoneNumber;                            
                    }
                                        
                    if (!string.IsNullOrEmpty(item.OwnerId)) _owner.Value = await _userService.GetUserAsync(item.OwnerId);                                       

                    IMapService _mapService = new MapService();
                    _myMap.Value = _mapService.MapToLocation(item.Location);
                   
                    await _bidListService.LoadAsync(item.Id);

                    var bidDocument = await bidTask.ConfigureAwait(false);

                    _isBid.Value = bidDocument.Exists;

                    _isLoaded.Value = true;

                    GetTbItemName();

                    CrossFirebaseAnalytics.Current.LogEvent(EventName.ViewItem, new Parameter(ParameterName.ItemId, id));
                }
            }
catch (Exception e)
{
    System.Diagnostics.Debug.WriteLine(e);
    _loadErrorNotifier.OnNext(e.Message);
}
        }

        public async Task BidOrUnBidAsync()
        {
            if (!IsBid.Value)
            {
                await BidAsync();
            }
            //else
            //{
            //    await UnBidAsync();
            //}
        }

        public async Task BidAsync()
        {
            if (Item.Value == null || IsBid.Value)
                return;

            _isBid.Value = true;
            Item.Value.BidCount++;

            using (await _lock.LockAsync())
            {
                try
                {
                    var success = await _firestore.RunTransactionAsync(transaction =>
                    {
                        var document = _firestore.GetCollection(Models.Item.CollectionPath)
                                                 .GetDocument(Item.Value.Id);

                        var item = transaction.GetDocument(document).ToObject<Item>();

                        if (item == null)
                            return false;

                        item.BidCount++;

                        var bidder = _firestore.GetCollection(User.CollectionPath)
                                     .GetDocument(_accountService.UserId.Value);
                        var bid = new Bid
                        {
                            Id = item.Id,
                            //ItemId = item.Id,
                            Timestamp = DateTime.Now.Ticks,
                            //Status = "Pending",
                            BidderName = _accountService.UserName.Value,
                            BidderImage = _accountService.UserImage.Value,
                            BidderReputation = _accountService.UserReputation.Value
                        };
                        document.GetCollection(Bid.CollectionPath)
                                    .GetDocument(_accountService.UserId.Value)
                                    .SetDataAsync(bid)
                                    .ConfigureAwait(false);

                        transaction.UpdateData(document, item);

                        return true;
                    }).ConfigureAwait(false);

                    if (success)
                    {
                        //var bid = new Bid
                        //{
                        //    Timestamp = DateTime.Now.Ticks,                            
                        //    BidderName = _accountService.UserName.Value,
                        //    BidderImage = _accountService.UserImage.Value,
                        //    BidderReputation = _accountService.UserReputation.Value
                        //};

                        //await _firestore.GetDocument($"{User.CollectionPath}/{_accountService.UserId.Value}/{Bid.CollectionPath}/{Item.Value.Id}")
                        //                .SetDataAsync(bid)
                        //                .ConfigureAwait(false);

                        await SetNoticeAsync(Owner.Value.Id , AppResources.Proposal, AppResources.New_proposals_for_your_work);

                        GetTbItemName();
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }
        }

        public async Task UnBidAsync()
        {
            if (Item.Value == null || !IsBid.Value)
                return;

            _isBid.Value = false;
            Item.Value.BidCount--;

            using (await _lock.LockAsync())
            {
                try
                {
                    var success = await _firestore.RunTransactionAsync(transaction =>
                    {
                        var document = _firestore.GetCollection(Models.Item.CollectionPath)
                                                 .GetDocument(Item.Value.Id);

                        var item = transaction.GetDocument(document).ToObject<Item>();

                        if (item == null)
                            return false;

                        item.BidCount--;
                        transaction.UpdateData(document, item);

                        return true;
                    }).ConfigureAwait(false);

                    if (success)
                    {
                        await _firestore.GetDocument($"{Models.Item.CollectionPath}/{Item.Value.Id}/{Models.Bid.CollectionPath}/{_accountService.UserId.Value}")
                                        .DeleteDocumentAsync()
                                        .ConfigureAwait(false); 
                        GetTbItemName();
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }
        }

        public async Task DeleteAsync()
        {
            try
            {
                using (_deletingNotifier.ProcessStart())
                {
                    await _firestore.GetCollection(Models.Item.CollectionPath)
                                    .GetDocument(Item.Value.Id)
                                    .DeleteDocumentAsync();

                    await _accountService.IncrementContributionCountAsync(-1);
                }
                _deleteCompletedNotifier.OnNext(Unit.Default);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                _deleteErrorNotifier.OnNext(e.Message);
            }
        }

        public async Task CalificateWorkAsync(bool good)
        {
            try
            {
                using (_deletingNotifier.ProcessStart())
                {
                    var id = Assigned.Value.Id;
                    var success = await _userService.CalificateUserAync(id, good);
                    if (success) success = await CompleteWorkAsync(Item.Value.Id);
                    if (success) await SetNoticeAsync(id, AppResources.Calificate, AppResources.Are_you_satisfied_with_the_job);
                }
                GetTbItemName();
                _deleteCompletedNotifier.OnNext(Unit.Default);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                _deleteErrorNotifier.OnNext(e.Message);
            }
        }

        public async Task AssignAsync(string id)
        {
            try
            {
                using (_deletingNotifier.ProcessStart())
                {
                    await AssignBidderAsync(id);
                }
                GetTbItemName();
                _deleteCompletedNotifier.OnNext(Unit.Default);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                _deleteErrorNotifier.OnNext(e.Message);
            }
        }

        public async Task AssignBidderAsync(string id)
        {
            if (Item.Value == null || Assigned.Value != null)
                return;

            using (await _lock.LockAsync())
            {
                try
                {
                    var success = await _firestore.RunTransactionAsync(transaction =>
                    {
                        var document = _firestore.GetCollection(Models.Item.CollectionPath)
                                                 .GetDocument(Item.Value.Id);

                        var item = transaction.GetDocument(document).ToObject<Item>();

                        if (item == null)
                            return false;

                        item.Assigned = id;

                        transaction.UpdateData(document, item);

                        return true;
                    }).ConfigureAwait(false);

                    if (success)
                    {
                        if (!string.IsNullOrEmpty(id)) _assigned.Value = await _userService.GetUserAsync(id);

                        await SetNoticeAsync(id, AppResources.Job, AppResources.Congratulations_you_have_a_new_job);

                        //GetTbItemName();
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }
        }

        public void GetTbItemName()
        {
            _tbItemName.Value = IsOwner.Value ? (Assigned.Value != null ? AppResources.Complete : AppResources.Delete)
                    : (IsBid.Value ? "" : AppResources.Bid);//"UnBid" : "Bid");
        }        

        public async Task<bool> CompleteWorkAsync(string ItemId)
        {
            return await _firestore.RunTransactionAsync(transaction =>
            {
                var document = _firestore.GetCollection(Models.Item.CollectionPath).GetDocument(ItemId);
                var item = transaction.GetDocument(document).ToObject<Item>();
                if (item == null) return false;
                item.Status = "Completed";
                transaction.UpdateData(document, item);
                return true;
            }).ConfigureAwait(false);
        }
        
        public async Task SetNoticeAsync(string userId, string action, string comment)
        {
            var notice = new Notice
            {
                Id = Item.Value.Id,
                Action = action,
                Comment = comment,
                ItemId = Item.Value.Id,
                Timestamp = DateTime.Now.Ticks,
                Title = Item.Value.Title,
                UserId = userId
            };
            await _noticeService.SetNotice(notice, 
                userId,//Assigned.Value.Id, 
                Item.Value.Id);
        }

    }
}
