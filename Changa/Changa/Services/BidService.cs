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
using System.Reactive.Disposables;

namespace Changa.Services
{
    public class BidService : IBidService
    {
        private readonly IAccountService _accountService;
        private readonly IFirestore _firestore;
        private readonly AsyncLock _lock = new AsyncLock();
        private readonly CompositeDisposable _disposables = new CompositeDisposable();

        private readonly ReactivePropertySlim<Bid> _bid = new ReactivePropertySlim<Bid>();
        public ReadOnlyReactivePropertySlim<Bid> Bid { get; }

        //public ReadOnlyReactivePropertySlim<bool> IsOwner { get; }
        private readonly ReactivePropertySlim<User> _owner = new ReactivePropertySlim<User>();
        public ReadOnlyReactivePropertySlim<User> Owner { get; }    

        private readonly ReactivePropertySlim<bool> _isBid = new ReactivePropertySlim<bool>();
        public ReadOnlyReactivePropertySlim<bool> IsBid { get; }

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


        public BidService()
        {            
            _firestore = CrossCloudFirestore.Current.Instance;

            Bid = _bid.ToReadOnlyReactivePropertySlim();
            Owner = _owner.ToReadOnlyReactivePropertySlim();            
            
            IsLoaded = _isLoaded.ToReadOnlyReactivePropertySlim();
            IsDeleting = _deletingNotifier.ToReadOnlyReactivePropertySlim();
        }

        public async Task LoadAsync(string id)
        {
            try
            {
                _isLoaded.Value = false;

                var document = await _firestore.GetCollection(Models.Bid.CollectionPath)
                                                   .GetDocument(id)
                                                   .GetDocumentAsync()
                                                   .ConfigureAwait(false);

                var bid = document.ToObject<Bid>();

                if (bid != null)
                {
                    _bid.Value = bid;

                    var task = _firestore.GetDocument($"{User.CollectionPath}/{_accountService.UserId}/{Models.Bid.CollectionPath}/{id}")
                                             .GetDocumentAsync();

                    if (!string.IsNullOrEmpty(bid.BidderId))
                    {
                        var ownerDocument = await _firestore.GetCollection(User.CollectionPath)
                                                            .GetDocument(bid.BidderId)
                                                            .GetDocumentAsync()
                                                            .ConfigureAwait(false);

                        _owner.Value = ownerDocument.ToObject<User>();
                    }                                       

                    _isBid.Value = document.Exists;

                    _isLoaded.Value = true;

                    CrossFirebaseAnalytics.Current.LogEvent(EventName.ViewItem, new Parameter(ParameterName.ItemId, id));
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
                _loadErrorNotifier.OnNext(e.Message);
            }
        }                
        
    }
}
